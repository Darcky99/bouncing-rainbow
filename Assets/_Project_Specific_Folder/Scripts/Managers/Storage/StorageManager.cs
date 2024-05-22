using KobGamesSDKSlim.Collectable;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class StorageManager : StorageManagerBase
    {
        // Examples
        [Title("Project Specific")]
        [ShowInInspector, PropertyOrder(10)]
        public int ExtendedVariablePersistent { get { return PlayerPrefs.GetInt(nameof(ExtendedVariablePersistent), 0); } set { PlayerPrefs.SetInt(nameof(ExtendedVariablePersistent), value); } }

        [PropertyOrder(30)]
        public int ExtendedVariableNonPersistent;

        #region Collectables
        public new delegate void CollectableAmountChangedEvent(int i_CoinsAmount);
        public static event CollectableAmountChangedEvent OnCoinsAmountChanged = delegate { };

        public override void SetCollectable(eCollectableType i_CollectableType, int i_Amount)
        {
            base.SetCollectable(i_CollectableType, i_Amount);

            switch (i_CollectableType)
            {
                case eCollectableType.Coin:
                    OnCoinsAmountChanged?.Invoke(i_Amount);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}