using TMPro;
using UnityEngine;

public class UIPanel : MonoBehaviour
{


    [SerializeField] private GameObject m_panelToActivate;
    [SerializeField] private TextMeshProUGUI m_textLevel;
    [SerializeField] private TextMeshProUGUI m_textDifficulty;
    private void Start()
    {
        if (m_textLevel == null || m_textDifficulty == null ) return;
        m_textLevel.text = "LEVEL " + GameManager.Instance.CurrentLevel;
        m_textDifficulty.text = GameManager.Instance.Difficulty.ToString();
    }
    public void OnActivePanel()
    {
        if (m_panelToActivate == null) return;
        AudioManager.Instance.PlaySFX(SFXType.Click);
        m_panelToActivate.SetActive(!m_panelToActivate.activeSelf);
    }


    public void OnExitGame()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        Application.Quit();
    }


}


