using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PopupBonus : MonoBehaviour
{
    [SerializeField] private GameObject m_popupRoot;
    [SerializeField] private float m_displayTime = 2f;

    private void Start()
    {
        m_popupRoot.SetActive(false);
        List<GrillStation> bonusGrills = GameManager.Instance.GetBonusGrills();
        if (bonusGrills != null && bonusGrills.Count > 0)
        {
            Show(bonusGrills);
        }
    }

    public void Show(List<GrillStation> bonusGrills)
    {
        // Hiện popup
        m_popupRoot.SetActive(true);
        m_popupRoot.transform.localScale = Vector3.zero;
        m_popupRoot.transform
            .DOScale(Vector3.one, 0.35f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Chờ vài giây rồi tự ẩn + bật grill
                DOVirtual.DelayedCall(m_displayTime, () =>
                {
                    m_popupRoot.transform
                        .DOScale(Vector3.zero, 0.2f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            m_popupRoot.SetActive(false);

                            // Bật grill bonus sau khi popup ẩn
                            foreach (var grill in bonusGrills)
                            {
                                grill.SetBonusGrill();
                            }

                        });
                });
            });
    }
}
