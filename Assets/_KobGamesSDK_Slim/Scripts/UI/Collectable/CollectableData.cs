using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace KobGamesSDKSlim.Collectable
{
    [System.Serializable]
    public class CollectableData
    {
        public int InitialValue;

        public Sprite CollectableSprite;

        public Material ParticlesMaterial;
        public Vector3 ParticlesSize = Vector3.one;

        [HideInInspector] public eCollectableType Type;
        [ShowInInspector] public int CurrentValue => StorageManager.Instance.GetCollectable(Type);
    }
}

