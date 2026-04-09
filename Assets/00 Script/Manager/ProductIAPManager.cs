using System.Collections.Generic;
using System.ComponentModel.Design;
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
    public string removeAds = "remove_ads";
    public static bool IsInitialized { get; private set; } = false;

    private static StoreController m_storeController;

   protected override async void Awake()
    {
        if (m_shopPage == null)
            m_shopPage = FindFirstObjectByType<ShopPage>();

        await InitIAP();

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
        var intialProductToFetch = new List<ProductDefinition>();

        intialProductToFetch.Add(new ProductDefinition(coin1000, ProductType.Consumable));
        intialProductToFetch.Add(new ProductDefinition(coin5000, ProductType.Consumable));
        intialProductToFetch.Add(new ProductDefinition(coin10000, ProductType.Consumable));
        intialProductToFetch.Add(new ProductDefinition(removeAds, ProductType.NonConsumable));

        return intialProductToFetch;
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

        if (productKey == IAPProductKey.Coin1000)
        {
            m_storeController.PurchaseProduct(coin1000);
        }
        else if (productKey == IAPProductKey.Coin5000)
        {
            m_storeController.PurchaseProduct(coin5000);

        }
        else if (productKey == IAPProductKey.Coin10000)
        {
            m_storeController.PurchaseProduct(coin10000);
        }
        else if (productKey == IAPProductKey.RemoveAds)
        {
            m_storeController.PurchaseProduct(removeAds);

        }
     }

    private void OnPurchasePending(PendingOrder order)
    {
        Debug.Log("pending order: " + order);
        m_storeController.ConfirmPurchase(order);

    }
    private void OnPurchaseDeferred(DeferredOrder deferredOrder)
    {
        Debug.Log("deferred order: " + deferredOrder?.Info);

        // Show UI to inform the user that their purchase is pending and will be processed later.
    }

    private void OnPurchaseConfirmed(Order order)
    {
        if(order?.Info?.PurchasedProductInfo !=null && order.Info.PurchasedProductInfo.Count>  0)
        {
            string productId = order.Info.PurchasedProductInfo[0].productId;

            if (productId == coin1000)
            {
                GoldManager.Instance.AddGold(1000);
            }
            else if (productId == coin5000)
            {
                GoldManager.Instance.AddGold(5000);

            }
            else if (productId == coin10000)
            {
                GoldManager.Instance.AddGold(10000);
            }
            else if (productId == removeAds)
            {
                AdsManager.Instance.SetAdsRemoved();
                m_popupNoAds?.HidePopup();
                m_homePlay?.HideRemoveAdsButton();

            }
         }

    }

}
