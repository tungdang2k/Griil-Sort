using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupNoAds : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_removeAdsPriceText;
    [SerializeField] private ProductIAPManager m_productIAPManager;

    private void Start()
    {
        if (AdsManager.Instance.IsAdsRemoved)
        {
            HidePopup();
        }

        if (m_productIAPManager == null)
        {
            m_productIAPManager = FindFirstObjectByType<ProductIAPManager>();
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

        m_productIAPManager.BuyProduct(IAPProductKey.RemoveAds);
    }


}   
