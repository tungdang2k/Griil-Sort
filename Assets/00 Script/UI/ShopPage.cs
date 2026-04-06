using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopPage : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI m_coin1000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin5000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin10000PriceText;
    [SerializeField] private TextMeshProUGUI m_removeAds;


    public void UpdateButtonPrice(string productId, string price)
    {
        if (productId == ProductIAPManager.Instance.coin1000)
        {
            m_coin1000PriceText.text = price;
        }
        else if (productId == ProductIAPManager.Instance.coin5000)
        {
            m_coin5000PriceText.text = price;
        }
        else if (productId == ProductIAPManager.Instance.coin10000)
        {
            m_coin10000PriceText.text = price;
        }
        else if (productId == ProductIAPManager.Instance.removeAds)
        {
            m_removeAds.text = price;

        }
    }


    public void Coin1000()
    {
        ProductIAPManager.Instance.BuyProduct(IAPProductKey.Coin1000);
    }

    public void Coin5000()
     {
        ProductIAPManager.Instance.BuyProduct(IAPProductKey.Coin5000);
    }
    public void Coin10000()
    {
        ProductIAPManager.Instance.BuyProduct(IAPProductKey.Coin10000);
    }
    
     public void Coin50000()
     {
        
    }

   

}
