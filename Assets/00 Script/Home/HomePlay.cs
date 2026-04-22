using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomePlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_levelText;
    [SerializeField] private GameObject m_btnAdsRemove;

    private int m_level;
    [SerializeField] private int m_showAdEveryNLevels = 3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (AdsManager.Instance.IsAdsRemoved)
        {
            HideRemoveAdsButton();
        }

        if (PlayerPrefs.HasKey("LEVEL") == false)
        {
            PlayerPrefs.SetInt("LEVEL", 1);
        }
        m_level = PlayerPrefs.GetInt("LEVEL");
        m_levelText.text = "Level " + m_level;
    }

    public void HideRemoveAdsButton()
    {
        if (m_btnAdsRemove != null)
            m_btnAdsRemove.SetActive(false);
    }

    public void OnPlayGame()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);

        int levelsPlayed = PlayerPrefs.GetInt("LEVELS_PLAYED_SINCE_AD", 0) + 1;
        PlayerPrefs.SetInt("LEVELS_PLAYED_SINCE_AD", levelsPlayed);

        bool shouldShowAd = levelsPlayed >= m_showAdEveryNLevels;

        if (shouldShowAd)
        {
            PlayerPrefs.SetInt("LEVELS_PLAYED_SINCE_AD", 0);
            AdsManager.Instance.TryShowInterstitial(() =>
            {
                StartGame();
            });
        }
        else
        {
            StartGame();
        }
    }
    private void StartGame()
    {
        GameManager.Instance.StartLevel();
        LoadingSceneManager.Instance.SwichToScene(CONSTANTS.MAINSCENE);
    }

   
   
}
