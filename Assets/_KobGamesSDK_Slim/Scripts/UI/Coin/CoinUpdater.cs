using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;

namespace KobGamesSDKSlim.UI
{
    /// <summary>
    /// Automatically updates Coins when Storage is changed
    /// </summary>
    public class CoinUpdater : CoinContainer
    {
        [SerializeField] protected bool AutoPopAnimation = true;


        protected override void OnEnable()
        {
            base.OnEnable();

            StorageManager.OnCoinsAmountChanged += CoinsChanged;

            SetCoins(StorageManager.Instance.CoinsAmount, false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StorageManager.OnCoinsAmountChanged -= CoinsChanged;
        }


        protected virtual void CoinsChanged(int i_Value)
        {
            SetCoins(i_Value, AutoPopAnimation);
        }
    }
}

