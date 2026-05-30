using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] GameObject m_winPanel;
    [SerializeField] GameObject m_losePanel;
    [SerializeField] UIPanel m_panelGuide;
    [SerializeField] HandTurorial m_handTutorial;

    private void Start()
    {
        if(m_handTutorial == null)
        {
            m_handTutorial = FindFirstObjectByType<HandTurorial>();
        }
        

        CheckTutorial();
    }

    private void OnEnable()
    {
        if (!GameManager.HasInstance) return;
        GameManager.Instance.OnWin += ShowWin;
        GameManager.Instance.OnOutOfTime += ShowLose;
    }

    private void OnDisable()
    {
        if (!GameManager.HasInstance) return;
        GameManager.Instance.OnWin -= ShowWin;
        GameManager.Instance.OnOutOfTime -= ShowLose;
    }

    private void ShowWin()
    {
        if(m_winPanel == null) return;
        m_winPanel.SetActive(true);
    }

    private void ShowLose()
    {
        if (m_losePanel == null) return;
        m_losePanel.SetActive(true);
    }

    private void CheckTutorial()
    {
        if (GameManager.Instance.CurrentLevel == 1)
        {
            m_panelGuide.OnPanelClosed -= HandleGuideClosed;
            m_panelGuide.OnPanelClosed += HandleGuideClosed;

            m_panelGuide.OnActivePanel();
        }
    }

    private void HandleGuideClosed()
    {
        m_handTutorial.ShowHandGuide();
    }
     
}
