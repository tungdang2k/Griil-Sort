using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource m_musicSource;
    [SerializeField] private AudioSource m_sfxSource;

    [SerializeField] private SoundData[] m_sfxArr;

    private Dictionary<SFXType, AudioClip> m_sfxDict;
    private Dictionary<SFXType, AudioSource> m_loopSources = new();

    protected override void Awake()
    {
        base.Awake();

        m_sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var sfx in m_sfxArr)
            m_sfxDict[sfx.type] = sfx.clip;
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
}

[System.Serializable]
public class SoundData
{
    public SFXType type;
    public AudioClip clip;
}
