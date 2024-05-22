using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim.Collectable
{
    public class CollectableManager : Singleton<CollectableManager>
    {
        [SerializeField, ReadOnly] private Canvas m_HUDCanvas;
        [SerializeField, ReadOnly] private Canvas m_WorldCanvas;

        [SerializeField, ReadOnly] private TypeCollectableUpdaterDictionary m_DefaultCollectableTargetDictionary;
        [SerializeField, ReadOnly] private UniversalCollectableSender m_CollectableSender;

        [SerializeField] private bool m_IsOverrideCollectableParent;
        [SerializeField, ShowIf(nameof(m_IsOverrideCollectableParent))] private Transform m_DefaultCollectableParent;

        [Serializable] public class TypeCollectableUpdaterDictionary : UnitySerializedDictionary<eCollectableType, CollectableUpdater> { }
        public CollectableUpdater DefaultCollectableTarget(eCollectableType i_Type) => m_DefaultCollectableTargetDictionary[i_Type];

        public Canvas HUDCanvas => m_HUDCanvas;

        #region Editor
        [Button]
        private void SetRefs()
        {
            m_WorldCanvas = transform.GetComponent<Canvas>();
            m_WorldCanvas.worldCamera = Camera.main;

            if (transform.parent == null) return;//Need to fix some weird errors

            m_HUDCanvas = transform.parent.GetComponentInChildren<HUDManager>().GetComponent<Canvas>();

            m_CollectableSender = m_HUDCanvas.transform.FindDeepChild<UniversalCollectableSender>("Collectable Sender");

            CollectableUpdater[] collectables = null;
            if (m_IsOverrideCollectableParent && m_DefaultCollectableParent != null) 
                collectables = m_DefaultCollectableParent.GetComponentsInChildren<CollectableUpdater>(true);
            else
                collectables = m_HUDCanvas.transform.GetComponentsInChildren<CollectableUpdater>();

            m_DefaultCollectableTargetDictionary = new TypeCollectableUpdaterDictionary();
            foreach (var item in collectables)
            {
                if (m_DefaultCollectableTargetDictionary.ContainsKey(item.CollectableType))
                {
                    Debug.LogError($"There are several default collectable targets with {item.CollectableType} type");

                    continue;
                }

                m_DefaultCollectableTargetDictionary.Add(item.CollectableType, item);
            }
        }

        private void OnValidate()
        {
            SetRefs();
        }
        #endregion

        #region Send UI
        private void sendCollectables(eCollectableType i_CollectableType, int i_SendAmount, eCollectableSendAnimType i_SendType, Vector2 i_ScreenPosition)
        {
            m_CollectableSender.Send(i_CollectableType, i_SendAmount, i_SendType, i_ScreenPosition);
        }

        [Button]
        public void SendCollectables(eCollectableType i_CollectableType, int i_SendAmount, eCollectableSendAnimType i_AnimType, Vector3 i_Position, bool i_IsScreenSpace = false)
        {
            sendCollectables(i_CollectableType, i_SendAmount, i_AnimType, i_IsScreenSpace ? i_Position : Camera.main.WorldToScreenPoint(i_Position));
        }
        #endregion

        #region Show earn message

        [Button]
        public void ShowEarnMessage(eCollectableType i_CollectableType, int i_EarnAmount, eCollectableEarnMessageAnimType i_AnimType, Vector3 i_Position, bool i_IsWhite = true)
        {
            var earnMessage = PoolManager.Instance.Dequeue(ePoolType.CollectableEarnMessage).GetComponent<CollectableEarnMessage>();

            earnMessage.transform.SetParent(m_WorldCanvas.transform);
            earnMessage.ShowMessage(i_CollectableType, i_EarnAmount, i_AnimType, i_Position, i_IsWhite);
        }

        #endregion
    }

}
