using UnityEngine;

public class PauseCtrl : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    void Awake()
    {
        pausePanel.gameObject.SetActive(false);
        //OnClickPauseBtn();
    }

    public void OnClickPauseBtn()
    {
        bool isPaused = !pausePanel.activeSelf;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

}
