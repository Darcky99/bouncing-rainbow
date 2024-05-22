using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace KobGamesSDKSlim
{
    public class iAPManagerBase : Singleton<iAPManager>
    {
        [PropertyOrder(-102), BoxGroup, ShowInInspector, DisplayAsString, HideLabel, EnableGUI, GUIColor(0, 1, 0)]
        private string m_AdsDetailsStatus
        {
#if UNITY_PURCHASING
            get => $"UNITY_PURCHASING: True";
#else
            get => $"UNITY_PURCHASING: False";
#endif
        }

#if UNITY_PURCHASING
        [ReadOnly, ShowInInspector] protected bool m_IsRestore = true;

        public void SetIsRestore(bool i_Value)
        {
            m_IsRestore = i_Value;
        }

        public void OnPurchaseComplete(Product i_Product)
        {
            if (i_Product != null)
            {
                if (m_IsRestore)
                {
                    OnIAP_PurchaseRestoreComplete(i_Product);
                }
                else
                {
                    OnIAP_PurchaseComplete(i_Product);
                }

                ProcessPurchasedProduct(i_Product, m_IsRestore);
            }
        }

        public void OnPurchaseFailed(Product i_Product, PurchaseFailureReason i_Reason)
        {
            if (m_IsRestore)
            {
                OnIAP_PurchaseRestoreFailed(i_Product, i_Reason);
            }
            else
            {
                OnIAP_PurchaseFailed(i_Product, i_Reason);
            }
        }

        protected virtual void OnIAP_PurchaseComplete(Product i_Product)
        {
            Managers.Instance.Analytics.OnIAP_PurchaseComplete(i_Product);
        }

        protected virtual void OnIAP_PurchaseRestoreComplete(Product i_Product)
        {
            Managers.Instance.Analytics.OnIAP_PurchaseRestoreComplete(i_Product);
        }

        protected virtual void OnIAP_PurchaseFailed(Product i_Product, PurchaseFailureReason i_Reason)
        {
            Managers.Instance.Analytics.OnIAP_PurchaseFailed(i_Product, i_Reason);
        }

        protected virtual void OnIAP_PurchaseRestoreFailed(Product i_Product, PurchaseFailureReason i_Reason)
        {
            Managers.Instance.Analytics.OnIAP_PurchaseRestoreFailed(i_Product, i_Reason);
        }

        protected virtual void ProcessPurchasedProduct(Product i_Product, bool i_IsRestore) { }

        public void InitiatePurchase(string i_ProductId)
        {
            SetIsRestore(false);

            CodelessIAPStoreListener.Instance.InitiatePurchase(i_ProductId);
        }

        public void InititateRestore()
        {
            // Note: no need to restore for GooglePlay, happens automatically on launch, hence why only ios code

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SetIsRestore(true);

                CodelessIAPStoreListener.Instance.GetStoreExtensions<IAppleExtensions>().RestoreTransactions((i_Success) => { Debug.Log($"iAPManagerBase: Restore Status: {i_Success}"); });
            }
            else
            {
                Debug.Log($"{nameof(iAPManagerBase)}-{nameof(InititateRestore)} Platform: {Application.platform} not supported for restore");
            }
        }
#endif

        private void Reset()
        {
            AddIAPListener();
        }

        [ShowInInspector, ReadOnly]
        private static int s_TryAdddIAPListenerCount = 0;

        private const string k_IAPListenerType = "UnityEngine.Purchasing.IAPListener";

        [Button]
        public void RemoveIAPListener()
        {
            s_TryAdddIAPListenerCount = 0;

            if (this.GetComponent(k_IAPListenerType))
            {
                DestroyImmediate(this.GetComponent(k_IAPListenerType), true);
            }
        }

        [Button]
        public void AddIAPListener()
        {
#if UNITY_EDITOR && UNITY_PURCHASING
            var iAPListener = this.GetComponent(k_IAPListenerType);

            var type = Utils.GetTypeByString(k_IAPListenerType);

            if (iAPListener == null)
            {
                iAPListener = this.gameObject.AddComponent(type);
            }

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (iAPListener == null)
                {
                    iAPListener = this.GetComponent(k_IAPListenerType);
                }

                var iAPListenerGO = iAPListener.GetComponent<IAPListener>();
                if (iAPListenerGO != null)
                {

                    if (iAPListenerGO.onPurchaseComplete?.GetPersistentEventCount() == 0)
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(iAPListenerGO.onPurchaseComplete, OnPurchaseComplete);

                    if (iAPListenerGO.onPurchaseFailed?.GetPersistentEventCount() == 0)
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(iAPListenerGO.onPurchaseFailed, OnPurchaseFailed);
                }

                if (s_TryAdddIAPListenerCount < 5 && (iAPListener == null || iAPListenerGO.onPurchaseComplete == null || iAPListenerGO.onPurchaseFailed == null))
                {
                    s_TryAdddIAPListenerCount++;
                    AddIAPListener();
                }
            };
#endif
        }
    }
}