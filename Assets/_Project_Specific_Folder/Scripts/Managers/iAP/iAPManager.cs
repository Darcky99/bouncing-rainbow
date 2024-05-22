using KobGamesSDKSlim;
using System;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class iAPManager : iAPManagerBase
{
#if UNITY_PURCHASING
    // Consumable: Coins (example)
    //public const string k_100Coins = "coins_pack_100";

    // Non-Consumable: Ads, Levels
    public const string k_NoAds = "no_ads";
    
    public static Action OnRemoveAdsPurchased = delegate { };

    public void IsRestore()
    {
        Debug.Log($"{nameof(iAPManager)}-{nameof(IsRestore)} SetRestore to True");

        base.SetIsRestore(true);
    }

    protected override void ProcessPurchasedProduct(Product i_Product, bool i_IsRestore)
    {
        base.ProcessPurchasedProduct(i_Product, i_IsRestore);

        Debug.Log($"{nameof(iAPManager)}-{nameof(ProcessPurchasedProduct)} ProductId: {i_Product.definition.id} IsResotre: {i_IsRestore}");

        switch (i_Product.definition.id)
        {
            //case k_100Coins:
            //    if (!i_IsRestore) // Non-Consumable products should be consumed on purchase and not on restore
            //        ScoreManager.Instance.UpdateCoinHighScoreLevel(100);
            //    break;
            case k_NoAds:
                Managers.Instance.Storage.RemovedAds = true;
#if ENABLE_ADS
                Managers.Instance.Ads.HideBanner();
#endif
                OnRemoveAdsPurchased.InvokeSafe();
                break;
        }
    }

    public void InitiateRemoveAdsPurchase()
    {
        base.InitiatePurchase(k_NoAds);
    }
#endif
}