using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseCtrl : MonoBehaviour
{

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
