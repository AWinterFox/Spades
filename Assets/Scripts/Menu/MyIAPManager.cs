using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Purchasing;

public class MyIAPManager : MonoBehaviour, IStoreListener
{

    private IStoreController controller;
    private IExtensionProvider extensions;

    void Start()
    {
        Debug.Log("badm");
    }

    public MyIAPManager()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct("jackpack", ProductType.Consumable, new IDs
        {
            {"jackpack", GooglePlay.Name},
            {"jackpack", MacAppStore.Name}
        });
        builder.AddProduct("queenpack", ProductType.Consumable, new IDs
        {
            {"queenpack", GooglePlay.Name},
            {"queenpack", MacAppStore.Name}
        });
        builder.AddProduct("kingpack", ProductType.Consumable, new IDs
        {
            {"kingpack", GooglePlay.Name},
            {"kingpack", MacAppStore.Name}
        });

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
    }
}