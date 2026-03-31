using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private GameObject m_panelToActivate;
    [SerializeField] private CanvasGroup m_canvasGroup;

    private void Start()
    {
        if(m_canvasGroup == null)
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void OnActivePanel(bool affectTimeScale = false)
    {
       
        if (m_panelToActivate == null) return;

        AudioManager.Instance.PlaySFX(SFXType.Click);

        bool isActive = m_panelToActivate.activeSelf;

        if (!isActive)
        {
            m_panelToActivate.SetActive(true);
            m_canvasGroup.alpha = 0;
            m_panelToActivate.transform.localScale = Vector3.zero;

            m_panelToActivate.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            m_canvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
            if (affectTimeScale)
                Time.timeScale = 0f;
        }
        else
        {
            m_panelToActivate.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true);
            m_canvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
            {
                m_panelToActivate.SetActive(false);
            });
            if (affectTimeScale)
                Time.timeScale = 1f;
        }

    }


    public void OnExitGame()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        Application.Quit();
    }


}


