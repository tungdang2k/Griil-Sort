using UnityEngine;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private GameObject m_panelToActivate;
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
