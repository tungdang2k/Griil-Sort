using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupNoAds : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_removeAdsPriceText;

    private void Start()
    {
        if (AdsManager.Instance.IsAdsRemoved)
        {
            HidePopup();
        }
    }

    public void UpdateRemoveAdsPrice(string price)
    {
        if (m_removeAdsPriceText != null)
            m_removeAdsPriceText.text = price;
    }
   

    public void HidePopup()
    {
        gameObject.SetActive(false);
    }
    public void OnRemoveAds()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        Debug.Log("Remove Ads Clicked");
        ProductIAPManager.Instance.BuyProduct(IAPProductKey.RemoveAds);
    }


}   
