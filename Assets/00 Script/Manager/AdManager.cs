using UnityEngine;
using UnityEngine.Advertisements;
public class AdManager : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string m_androidGameId = "6079219";
    [SerializeField] bool m_testMode = true;
    [SerializeField] BannerPosition m_bannerPosition = BannerPosition.BOTTOM_CENTER;
    [SerializeField] string m_bannerAdUnitId = "ad-banner";
    void Awake()
    {
        InitializeAds();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void InitializeAds()
    {

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(m_androidGameId, m_testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        RequestBanner();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void RequestBanner()
    {
        // Set the banner position:
        Advertisement.Banner.SetPosition(m_bannerPosition);

        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        // Load the Ad Unit with banner content:
        Advertisement.Banner.Load(m_bannerAdUnitId, options);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner loaded");

        ShowBannerAd();
    }

    // Implement code to execute when the load errorCallback event triggers:
    void OnBannerError(string message)
    {
        Debug.Log($"Banner Error: {message}");
        // Optionally execute additional code, such as attempting to load another ad.
        RequestBanner();
    }

    void ShowBannerAd()
    {
        // Set up options to notify the SDK of show events:
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        // Show the loaded Banner Ad Unit:
        Advertisement.Banner.Show(m_bannerAdUnitId, options);
    }

    // Implement a method to call when the Hide Banner button is clicked:
    void HideBannerAd()
    {
        // Hide the banner:
        Advertisement.Banner.Hide();
    }

    void OnBannerClicked() { }
    void OnBannerShown() { }
    void OnBannerHidden() { }
}
