using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPage : MonoBehaviour
{
    [SerializeField] private ProductIAPManager m_productIAPManager;

    [SerializeField] private ScrollRect m_scrollRect;

    [SerializeField] private TextMeshProUGUI m_coin1000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin5000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin10000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin25000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin50000PriceText;
    [SerializeField] private TextMeshProUGUI m_coin100000PriceText;

    [SerializeField] private TextMeshProUGUI m_legendarybundle;
    [SerializeField] private TextMeshProUGUI m_bigbundle;
    [SerializeField] private TextMeshProUGUI m_startedbundle;
    [SerializeField] private TextMeshProUGUI m_smallbundle;

    private Dictionary<string, TextMeshProUGUI> m_priceTextMap;
    private Dictionary<string, TextMeshProUGUI> PriceTextMap
    {
        get
        {
            if (m_priceTextMap == null)
                InitPriceTextMap();
            return m_priceTextMap;
        }
    }
    private void Awake()
    {
        if (m_productIAPManager == null)
        {
            m_productIAPManager = FindFirstObjectByType<ProductIAPManager>();
        }


        InitPriceTextMap();
    }

    private void Start()
    {
       
        Canvas.ForceUpdateCanvases();
        m_scrollRect.verticalNormalizedPosition = 1f;
    }

    private void InitPriceTextMap()
    { 
        var p = m_productIAPManager;
        m_priceTextMap = new Dictionary<string, TextMeshProUGUI>
        {
            { p.coin1000,       m_coin1000PriceText   },
            { p.coin5000,       m_coin5000PriceText   },
            { p.coin10000,      m_coin10000PriceText  },
            { p.coin25000,      m_coin25000PriceText  },
            { p.coin50000,      m_coin50000PriceText  },
            { p.coin100000,     m_coin100000PriceText },
            { p.legendarybundle,m_legendarybundle     },
            { p.bigbundle,      m_bigbundle           },
            { p.startedbundle,  m_startedbundle       },
            { p.smallbundle,    m_smallbundle         },
        };
    }


    public void UpdateButtonPrice(string productId, string price)
    {

        if (PriceTextMap.TryGetValue(productId, out var text))
        {
            text.text = price;
        }
        
    }
    public void Coin1000() => m_productIAPManager.BuyProduct(IAPProductKey.Coin1000);
    public void Coin5000() => m_productIAPManager.BuyProduct(IAPProductKey.Coin5000);
    public void Coin10000() => m_productIAPManager.BuyProduct(IAPProductKey.Coin10000);
    public void Coin25000() => m_productIAPManager.BuyProduct(IAPProductKey.Coin25000);
    public void Coin50000() => m_productIAPManager.BuyProduct(IAPProductKey.Coin50000);
    public void Coin100000() => m_productIAPManager.BuyProduct(IAPProductKey.Coin100000);
    public void Legendarybundle() => m_productIAPManager.BuyProduct(IAPProductKey.Legendarybundle);
    public void Bigbundle() => m_productIAPManager.BuyProduct(IAPProductKey.Bigbundle);
    public void Startedbundle() => m_productIAPManager.BuyProduct(IAPProductKey.Startedbundle);
    public void Smallbundle() => m_productIAPManager.BuyProduct(IAPProductKey.Smallbundle);
}