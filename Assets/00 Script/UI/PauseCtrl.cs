using UnityEngine;

public class PauseCtrl : MonoBehaviour
{
    [SerializeField] private GameObject m_pausePanel;
    

    public void OnClickPauseBtn()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        bool isPaused = !m_pausePanel.activeSelf;
        m_pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

}
