using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string adUnitId = "Interstitial-ads";
    [SerializeField] float cooldown = 30f;

    float lastShowTime;
    bool isReady = false;
    System.Action onCompleteCallback;  // ← thêm

    public void Init() => Load();
    public void Load() => Advertisement.Load(adUnitId, this);

    public bool CanShow() => isReady && Time.time - lastShowTime > cooldown;

    public void ShowWithCallback(System.Action onComplete)  // ← thêm
    {
        onCompleteCallback = onComplete;
        Advertisement.Show(adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string id)
    {
        if (id == adUnitId) isReady = true;
    }

    public void OnUnityAdsShowComplete(string id, UnityAdsShowCompletionState state)
    {
        if (id == adUnitId)
        {
            lastShowTime = Time.time;
            isReady = false;
            Load();
            onCompleteCallback?.Invoke();  // ← gọi callback SAU khi ads xong
            onCompleteCallback = null;
        }
    }

    public void OnUnityAdsFailedToLoad(string id, UnityAdsLoadError error, string msg)
    {
        Debug.Log("Interstitial Load Fail: " + msg);
    }
    public void OnUnityAdsShowFailure(string id, UnityAdsShowError error, string msg)
    {
        onCompleteCallback?.Invoke();  // ← ads lỗi vẫn cho chơi
        onCompleteCallback = null;
    }
    public void OnUnityAdsShowStart(string id) { }
    public void OnUnityAdsShowClick(string id) { }
}
