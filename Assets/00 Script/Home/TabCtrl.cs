using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TabCtrl : MonoBehaviour
{
    [SerializeField] private Image[] m_tabImage;
    [SerializeField] private GameObject[] m_pages;
    [SerializeField] private Image[] m_backGroundPages;
  

    float m_width;
    void Start()
    {
        SetTab(1);
    }

    public void OnActiveTab(int index)
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        SetTab(index);
    }

    public void SetTab(int index)
    {
       
        for (int i = 0; i < m_pages.Length; i++)
        {
            bool isActive = i == index;

            m_pages[i].SetActive(isActive);
            m_backGroundPages[i].gameObject.SetActive(isActive);
            // COLOR
            m_tabImage[i].DOKill();
            m_tabImage[i].DOColor(isActive ? Color.white : Color.gray, 0.15f);

            // SCALE
            RectTransform rect = m_tabImage[i].rectTransform;
            rect.DOKill();
            rect.DOScale(isActive ? 1.2f : 1f, 0.15f)
                .SetEase(Ease.OutBack);
        }

        
    }
}
