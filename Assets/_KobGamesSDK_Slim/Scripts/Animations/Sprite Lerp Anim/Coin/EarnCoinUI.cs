using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim.Animation
{
    public class EarnCoinUI : EarnObjectUI
    {
        public int CoinCost { get; private set; }

        public void SetCoinCost(int i_CoinCost)
        {
            CoinCost = i_CoinCost;
        }

        protected override void OnAnimComplete()
        {
            StorageManager.Instance.CoinsAmount += CoinCost;

            PoolManager.Instance.Queue(ePoolType.CoinUI, gameObject);

            base.OnAnimComplete();
        }
    }
}