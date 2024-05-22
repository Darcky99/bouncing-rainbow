using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class PoolConfig
    {
        public bool UseList = false;
        [HideIf(nameof(UseList))] public GameObject Prefab;
        [ShowIf(nameof(UseList))] public List<GameObject> PrefabList;
        public int InitialQuantity;
    }
}
