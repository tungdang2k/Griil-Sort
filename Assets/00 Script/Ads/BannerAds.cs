using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAds : MonoBehaviour
{
    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;
    [SerializeField] string bannerAdUnitId = "ad-banner";

    public void Init()
    {
        Advertisement.Banner.SetPosition(bannerPosition);

        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(bannerAdUnitId, options);
    }

    void OnBannerLoaded()
    {

        if (!AdsManager.Instance.IsAdsRemoved)
        {
            Show();
        }
    }

    void OnBannerError(string message)
    {
        Debug.Log("Banner Error: " + message);
    }

    public void Show()
    {
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(bannerAdUnitId, options);
    }

    public void Hide()
    {
        Advertisement.Banner.Hide();
        Debug.Log("Banner hide");
    }

    void OnBannerClicked()
    {
        Debug.Log("Banner clicked");
    }

    void OnBannerShown()
    {
        //Debug.Log("Banner shown");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner hidden");
    }
}
