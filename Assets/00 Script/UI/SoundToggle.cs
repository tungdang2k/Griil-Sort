using UnityEngine;

public class AudioToggleBinder : MonoBehaviour
{
    public enum AudioType
    {
        Sound,
        Music
    }

    [SerializeField] private UISwitcher.UISwitcher m_switcher;
    [SerializeField] private AudioType m_type;

    private void OnEnable()
    {
        if (m_type == AudioType.Sound)
        {
            AudioManager.Instance.OnSoundChanged += UpdateUI;
            m_switcher.isOn = AudioManager.Instance.SoundOn;
        }
        else
        {
            AudioManager.Instance.OnMusicChanged += UpdateUI;
            m_switcher.isOn = AudioManager.Instance.MusicOn;
        }

        m_switcher.OnValueChanged += OnSwitchChanged;
    }

    private void OnDisable()
    {
        if (m_type == AudioType.Sound)
            AudioManager.Instance.OnSoundChanged -= UpdateUI;
        else
            AudioManager.Instance.OnMusicChanged -= UpdateUI;

        m_switcher.OnValueChanged -= OnSwitchChanged;
    }

    private void UpdateUI(bool value)
    {
        if (m_switcher.isOn == value) return;
        m_switcher.isOn = value;
    }

    private void OnSwitchChanged(bool value)
    {
        if (m_type == AudioType.Sound)
            AudioManager.Instance.SetSound(value);
        else
            AudioManager.Instance.SetMusic(value);
    }
}
