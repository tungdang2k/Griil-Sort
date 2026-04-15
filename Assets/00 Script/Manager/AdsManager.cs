using UnityEngine;
using UnityEngine.Advertisements;
public class AdsManager : Singleton<AdsManager>, IUnityAdsInitializationListener
{
    [SerializeField] string androidGameId = "6079219";
    [SerializeField] bool testMode = true;

    public BannerAds bannerAds;
    public InterstitialAds interstitialAds;
    public RewardedAds rewardedAds;

    public static System.Action OnAdsRemoved;

    protected override void Awake()
    {
        base.Awake();
        Advertisement.Initialize(androidGameId, testMode, this);
    }

    public bool IsAdsRemoved => PlayerPrefs.GetInt(CONSTANTS.ADS_REMOVED_KEY, 0) == 1;

    public void SetAdsRemoved()
    {
        PlayerPrefs.SetInt(CONSTANTS.ADS_REMOVED_KEY, 1);
        PlayerPrefs.Save();
        HideBanner();
        OnAdsRemoved?.Invoke();
    }


    public void OnInitializationComplete()
    {
        if (IsAdsRemoved)
        {
            return;
        }

        bannerAds.Init();
        interstitialAds.Init();
        rewardedAds.Init();
    }

    public void TryShowInterstitial(System.Action onComplete)
    {
        if (IsAdsRemoved)
        {
            onComplete?.Invoke();
            return;
        }
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

    public void ShowBanner() {
        if (IsAdsRemoved) return;
        bannerAds.Show();
    } 

    public void HideBanner()
    {
        bannerAds.Hide();
    } 


    public void ShowRewarded(System.Action onReward)
    {
        rewardedAds.Show(onReward);
    }
}
