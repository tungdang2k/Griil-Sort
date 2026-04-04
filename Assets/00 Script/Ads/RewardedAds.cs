using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string m_adUnitId = "Rewarded-ads";

    bool isReady = false;
    Action onReward;

    public void Init()
    {
        Load();
    }

    public void Load()
    {
        Advertisement.Load(m_adUnitId, this);
    }

    public void Show(Action rewardCallback)
    {
        if (!isReady) return;

        onReward = rewardCallback;
        Advertisement.Show(m_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string id)
    {
        if (id == m_adUnitId)
            isReady = true;
    }

    public void OnUnityAdsFailedToLoad(string id, UnityAdsLoadError error, string msg)
    {
        Debug.Log("Rewarded Load Fail: " + msg);
    }

    public void OnUnityAdsShowComplete(string id, UnityAdsShowCompletionState state)
    {
        if (id == m_adUnitId)
        {
            if (state == UnityAdsShowCompletionState.COMPLETED)
            {
                onReward?.Invoke();
            }

            isReady = false;
            Load();
        }
    }

    public void OnUnityAdsShowFailure(string id, UnityAdsShowError error, string msg) { }
    public void OnUnityAdsShowStart(string id) { }
    public void OnUnityAdsShowClick(string id) { }
}
