using Crystal;
using UnityEngine;
using UnityEngine.Advertisements;
using static UnityEditor.Progress;
public class AdsManager : Singleton<AdsManager>, IUnityAdsInitializationListener
{
    [SerializeField] string androidGameId = "6079219";
    [SerializeField] bool testMode = true;

    public BannerAds bannerAds;
    public InterstitialAds interstitialAds;
    public RewardedAds rewardedAds;


    protected override void Awake()
    {
        base.Awake();
        Advertisement.Initialize(androidGameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Ads Initialized");

        bannerAds.Init();
        interstitialAds.Init();
        rewardedAds.Init();
    }

    public void TryShowInterstitial(System.Action onComplete)
    {
        if (interstitialAds.CanShow())
        {
            interstitialAds.ShowWithCallback(onComplete);
        }
        else
        {
            onComplete?.Invoke(); 
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Ads Init Failed: {error} - {message}");
    }

    // ===== API dùng trong game =====

    public void ShowBanner() => bannerAds.Show();

    public void HideBanner() => bannerAds.Hide();

 

    public void ShowRewarded(System.Action onReward)
    {
        rewardedAds.Show(onReward);
    }
}
