using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] GameObject m_winPanel;
    [SerializeField] GameObject m_losePanel;

    private void OnEnable()
    {
        GameManager.Instance.OnWin += ShowWin;
        GameManager.Instance.OnOutOfTime += ShowLose;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnWin -= ShowWin;
        GameManager.Instance.OnOutOfTime -= ShowLose;
    }

    void ShowWin()
    {
        if(m_winPanel == null) return;
        m_winPanel.SetActive(true);
    }

    void ShowLose()
    {
        if (m_losePanel == null) return;
        m_losePanel.SetActive(true);
    }
}
