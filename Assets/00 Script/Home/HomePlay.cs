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
        m_level = PlayerPrefs.GetInt("LEVEL");
        m_levelText.text = "Level " + m_level;
    }

    public void OnPlayeGame()
    {
        GameManager.Instance.StartLevl();
        SceneManager.LoadScene("MainScene");
        Debug.Log("Play Game");
    }

}
