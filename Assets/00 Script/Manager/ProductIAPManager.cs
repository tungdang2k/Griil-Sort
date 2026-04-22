using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class ProductIAPManager : Singleton<ProductIAPManager>
{
    [SerializeField] private ShopPage m_shopPage;
    [SerializeField] private PopupNoAds m_popupNoAds;
    [SerializeField] private HomePlay m_homePlay;
    public string coin1000 = "1000_coin";
    public string coin5000 = "5000_coin";
    public string coin10000 = "10000_coin";
    public string coin25000 = "25000_coin";
    public string coin50000 = "50000_coin";
    public string coin100000 = "100000_coin";

    public string legendarybundle = "legendary_bundle";
    public string bigbundle = "big_bundle";
    public string startedbundle = "started_bundle";
    public string smallbundle = "small_bundle";

    public string removeAds = "remove_ads";
    private HashSet<string> m_processedOrderIds = new HashSet<string>();


    public static bool IsInitialized { get; private set; } = false;

    private static StoreController m_storeController;
    private bool m_isProcessingPurchase = false;
    protected override async void Awake()
    {
        if (m_shopPage == null)
            m_shopPage = FindFirstObjectByType<ShopPage>();

        await InitIAP();

    }

    private void Start()
    {
        
    }

   

    private async Task InitIAP()
    {
        try
        {
            var options = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(options);
            m_storeController = UnityIAPServices.StoreController();

            m_storeController.OnStoreConnected += OnStoreConnected; // ✅
            m_storeController.OnStoreDisconnected += OnStoreDisconnected;
            m_storeController.OnProductsFetched += OnProductsFetched;
            m_storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            m_storeController.OnPurchasesFetched += OnPurchasesFetched;
            m_storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            m_storeController.OnPurchasePending += OnPurchasePending;
            m_storeController.OnPurchaseFailed += OnPurchaseFailed;
            m_storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            m_storeController.OnPurchaseDeferred += OnPurchaseDeferred;

            await m_storeController.Connect(); // FetchProducts sẽ gọi trong callback
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to initialize IAP: " + ex.Message);
        }
    }

    private void OnStoreConnected() 
    {
        m_storeController.FetchProducts(BuildProductDefinitions());
    }

    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        if (failedOrder?.Info?.PurchasedProductInfo == null
            || failedOrder.Info.PurchasedProductInfo.Count == 0)
        {
            return;
        }

        var productId = failedOrder.Info.PurchasedProductInfo[0].productId;
        var reason = failedOrder.FailureReason;
        var message = failedOrder.Details;
    }

     private List<ProductDefinition> BuildProductDefinitions()
    {
        return new List<ProductDefinition>
    {
        new ProductDefinition(coin1000,       ProductType.Consumable),
        new ProductDefinition(coin5000,       ProductType.Consumable),
        new ProductDefinition(coin10000,      ProductType.Consumable),
        new ProductDefinition(coin25000,      ProductType.Consumable),
        new ProductDefinition(coin50000,      ProductType.Consumable),
        new ProductDefinition(coin100000,     ProductType.Consumable),
        
        new ProductDefinition(legendarybundle, ProductType.Consumable),
        new ProductDefinition(bigbundle,       ProductType.Consumable),
        new ProductDefinition(startedbundle,   ProductType.Consumable),
        new ProductDefinition(smallbundle,     ProductType.Consumable),

        new ProductDefinition(removeAds,      ProductType.NonConsumable),
    };
    }


    private void OnProductsFetched(List<Product> products)
    {
        m_storeController.FetchPurchases();

        foreach(var product in products)
        {
            string price = product.metadata.localizedPriceString + " " + product.metadata.isoCurrencyCode;
            // pass price to UI

            if (product.definition.id == removeAds)
            {
                m_popupNoAds?.UpdateRemoveAdsPrice(price);
            }
            else
            {
                m_shopPage?.UpdateButtonPrice(product.definition.id, price);
            }
            
        }
    }


    private void OnProductsFetchFailed(ProductFetchFailed  reason)
    {
        Debug.Log(" products fetch Failed: " + reason);
    }

    private void OnPurchasesFetched (Orders orders)
    {
        IsInitialized = true;

        if (orders?.ConfirmedOrders == null || orders.ConfirmedOrders.Count == 0)
        {
            
            return;
        }

        foreach (var order in orders.ConfirmedOrders)
        {
            if (order?.Info?.PurchasedProductInfo == null) continue;

            foreach (var productInfo in order.Info.PurchasedProductInfo)
            {
                string productId = productInfo.productId;

                if (productId == removeAds ||
                    productId == startedbundle ||
                    productId == bigbundle ||
                    productId == legendarybundle ||
                    productId == smallbundle)
                {
                    AdsManager.Instance.SetAdsRemoved();
                    m_homePlay?.HideRemoveAdsButton();
                }
            }
        }

    }


    private void OnPurchasesFetchFailed (PurchasesFetchFailureDescription reason)
    {
        Debug.Log(" purchases fetch Failed: " + reason);
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.Log("Store Disconnected: " + description.message);
    }

    public void BuyProduct(IAPProductKey productKey)
    {
        if (!IsInitialized)
        {
            Debug.Log("IAP not initialized yet.");
            return;
        }
        switch (productKey)
        {
            case IAPProductKey.Coin1000: m_storeController.PurchaseProduct(coin1000); break;
            case IAPProductKey.Coin5000: m_storeController.PurchaseProduct(coin5000); break;
            case IAPProductKey.Coin10000: m_storeController.PurchaseProduct(coin10000); break;
            case IAPProductKey.Coin25000: m_storeController.PurchaseProduct(coin25000); break;
            case IAPProductKey.Coin50000: m_storeController.PurchaseProduct(coin50000); break;
            case IAPProductKey.Coin100000: m_storeController.PurchaseProduct(coin100000); break;
            case IAPProductKey.RemoveAds: m_storeController.PurchaseProduct(removeAds); break;
            case IAPProductKey.Startedbundle: m_storeController.PurchaseProduct(startedbundle); break;
            case IAPProductKey.Bigbundle: m_storeController.PurchaseProduct(bigbundle); break;
            case IAPProductKey.Legendarybundle: m_storeController.PurchaseProduct(legendarybundle); break;
            case IAPProductKey.Smallbundle: m_storeController.PurchaseProduct(smallbundle); break;
        }

    }

    private void OnPurchasePending(PendingOrder order)
    {

        m_storeController.ConfirmPurchase(order);

    }
    private void OnPurchaseDeferred(DeferredOrder deferredOrder)
    {
        //Debug.Log("deferred order: " + deferredOrder?.Info);

        // Show UI to inform the user that their purchase is pending and will be processed later.
    }

    private void OnPurchaseConfirmed(Order order)
    {
       
        if (m_isProcessingPurchase)
        {
            Debug.LogWarning("[IAP] Duplicate OnPurchaseConfirmed blocked!");
            return;
        }
        m_isProcessingPurchase = true;

        if (order?.Info?.PurchasedProductInfo == null ||
            order.Info.PurchasedProductInfo.Count == 0)
        {
            m_isProcessingPurchase = false;
            return;
        }

        string productId = order.Info.PurchasedProductInfo[0].productId;

        if (productId == coin1000) GoldManager.Instance.AddGold(1000);
        else if (productId == coin5000) GoldManager.Instance.AddGold(5000);
        else if (productId == coin10000) GoldManager.Instance.AddGold(10000);
        else if (productId == coin25000) GoldManager.Instance.AddGold(25000);
        else if (productId == coin50000) GoldManager.Instance.AddGold(50000);
        else if (productId == coin100000) GoldManager.Instance.AddGold(100000);
        else if (productId == removeAds)
        {
            AdsManager.Instance.SetAdsRemoved();
            m_popupNoAds?.HidePopup();
            m_homePlay?.HideRemoveAdsButton();
        }
        else if (productId == startedbundle)
        {
            GoldManager.Instance.AddGold(2500);
            AddPowerupToPlayer(2);
            GrantRemoveAdsFromBundle(); // ✅ Tự động grant remove_ads
        }
        else if (productId == bigbundle)
        {
            GoldManager.Instance.AddGold(10000);
            AddPowerupToPlayer(3);
            GrantRemoveAdsFromBundle();
        }
        else if (productId == legendarybundle)
        {
            GoldManager.Instance.AddGold(25000);
            AddPowerupToPlayer(5);
            GrantRemoveAdsFromBundle();
        }
        else if (productId == smallbundle)
        {
            GoldManager.Instance.AddGold(5000);
            AddPowerupToPlayer(2);
            GrantRemoveAdsFromBundle();
        }


        m_isProcessingPurchase = false;
    }

    private void GrantRemoveAdsFromBundle()
    {
        // Nếu chưa remove ads thì purchase remove_ads product
        if (!AdsManager.Instance.IsAdsRemoved)
        {
            m_storeController.PurchaseProduct(removeAds);
            
        }
        else
        {
            // Đã remove ads rồi, chỉ cần update UI
            m_homePlay?.HideRemoveAdsButton();
        }
    }

    private void AddPowerupToPlayer( int num)
    {
        PowerUpUsesManager.AddUses(CONSTANTS.MAGNET, num);
        PowerUpUsesManager.AddUses(CONSTANTS.SHUFFLE, num);
        PowerUpUsesManager.AddUses(CONSTANTS.ADDTIME, num);
    }


}
