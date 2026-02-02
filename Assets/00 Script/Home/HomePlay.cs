using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomePlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_levelText;
    private int m_level;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(PlayerPrefs.HasKey("LEVEL") == false)
        {
            PlayerPrefs.SetInt("LEVEL", 1);
        }
        m_level = PlayerPrefs.GetInt("LEVEL");
        m_levelText.text = "Level " + m_level;
    }

    public void OnPlayeGame()
    {
        GameManager.Instance.StartLevel();
        LoadingSceneManager.Instance.SwichToScene(CONSTANTS.MAINSCENE);
        AudioManager.Instance.PlaySFX(SFXType.Click);
        Debug.Log("Play Game");
    }

}
