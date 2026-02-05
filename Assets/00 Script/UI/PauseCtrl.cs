using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    public void OnRestart()
    {
        AudioManager.Instance.PlaySFX(SFXType.Restart);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
        Time.timeScale =  1f;
    }

    public void OnHome()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        GameManager.Instance.GoHome();
    }

}
