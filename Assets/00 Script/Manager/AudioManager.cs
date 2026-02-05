using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public event Action<bool> OnSoundChanged;
    public event Action<bool> OnMusicChanged;
    public bool SoundOn => m_soundOn;
    public bool MusicOn => m_musicOn;
    [SerializeField] private AudioSource m_musicSource;
    [SerializeField] private AudioSource m_sfxSource;
    [SerializeField] private SoundData[] m_sfxArr;

    private Dictionary<SFXType, AudioClip> m_sfxDict;
    private Dictionary<SFXType, AudioSource> m_loopSources = new();

    private bool m_musicOn;
    private bool m_soundOn;
    protected override void Awake()
    {
        base.Awake();
        LoadSetting();
        m_sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var sfx in m_sfxArr)
            m_sfxDict[sfx.type] = sfx.clip;
    }


    void LoadSetting()
    {
        m_musicOn = PlayerPrefs.GetInt(CONSTANTS.MUSIC_KEY, 1) == 1;
        m_soundOn = PlayerPrefs.GetInt(CONSTANTS.SOUND_KEY, 1) == 1;

        UpdateAudioState();
    }

    void SaveSetting()
    {
        PlayerPrefs.SetInt(CONSTANTS.MUSIC_KEY, m_musicOn ? 1 : 0);
        PlayerPrefs.SetInt(CONSTANTS.SOUND_KEY, m_soundOn ? 1 : 0);
    }

    void UpdateAudioState()
    {
        m_musicSource.mute = !m_musicOn;
        m_sfxSource.mute = !m_soundOn;
    }

    public void SetMusic(bool value)
    {
        m_musicOn = value;
        UpdateAudioState();
        SaveSetting();
        OnMusicChanged?.Invoke(value);
    }   

    public void SetSound(bool value)
    {
        m_soundOn = value;
        UpdateAudioState();
        SaveSetting();
        OnSoundChanged?.Invoke(value);
    }

    public void PlaySFX(SFXType type)
    {
        
        if (!m_sfxDict.ContainsKey(type)) return;
        m_sfxSource.PlayOneShot(m_sfxDict[type]);
    }

    public void PlayLoop(SFXType type)
    {
        if (m_loopSources.ContainsKey(type)) return;

        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.clip = m_sfxDict[type];
        src.loop = true;
        src.Play();

        m_loopSources[type] = src;
    }

    public void StopLoop(SFXType type)
    {
        if (!m_loopSources.ContainsKey(type)) return;

        m_loopSources[type].Stop();
        Destroy(m_loopSources[type]);
        m_loopSources.Remove(type);
    }



}

public enum SFXType
{
    Click,
    Smoke,
    Win,
    Drag,
    Merge,
    Coin,
    Lose,
    TimeBonus,
    Shuffle,
    Restart,
}

[System.Serializable]
public class SoundData
{
    public SFXType type;
    public AudioClip clip;
}
