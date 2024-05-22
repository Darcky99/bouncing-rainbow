using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public delegate void FirebaseRemoteConfigUpdatedEvent(bool i_IsUpdated);

    public class RemoteSettingsManager : Singleton<RemoteSettingsManager>
    {
        public static event FirebaseRemoteConfigUpdatedEvent OnFirebaseRemotConfigUpdated = delegate { };

        public float FirebaseRemoteConfigTimeout = 3; 

        public bool IgnoreRemoteConfigInEditor = false;

        [Title("Firebase RemoteSettings"), PropertyOrder(1), HideIf(nameof(IgnoreRemoteConfigInEditor))]
        public FirebaseRemoteConfig FirebaseRemoteConfig;

        [Button, PropertyOrder(1)]
        public void SetProductionMode()
        {
            FirebaseRemoteConfig.FetchTimeSpanHours = 6;
        }

        [Button, PropertyOrder(1)]
        public void SetTestMode()
        {
            FirebaseRemoteConfig.FetchTimeSpanHours = 0;
        }

        [Button, PropertyOrder(1)]
        public void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            FirebaseRemoteConfig.Awake();
        }

        public void OnEnable()
        {
            AnalyticsManagerBase.OnFirebaseInitializedEvent += OnFirebaseInitialized;

            Debug.Log($"{nameof(RemoteSettingsManager)}: OnEnable");
        }

        public override void OnDisable()
        {
            base.OnDisable();

            AnalyticsManagerBase.OnFirebaseInitializedEvent -= OnFirebaseInitialized;
        }

        public void OnFirebaseInitialized()
        {
            Debug.Log($"{nameof(RemoteSettingsManager)}: OnFirebaseInitialized");
            
            FirebaseRemoteConfig.OnFirebaseInitialized();

            if (!Application.isEditor || !IgnoreRemoteConfigInEditor)
            {
                FirebaseRemoteConfig.Fetch(onFirebaseRemotConfigUpdatedCompletion);
            }

            Invoke(nameof(onFirebaseRemotConfigUpdatedCompletionDelayed), FirebaseRemoteConfigTimeout);
        }

        private bool m_IsFirebaseRemoteCalled = false;
        private void onFirebaseRemotConfigUpdatedCompletion(bool i_IsUpdated)
        {
            if (!m_IsFirebaseRemoteCalled)
            {
                m_IsFirebaseRemoteCalled = true;
                CancelInvoke(nameof(onFirebaseRemotConfigUpdatedCompletionDelayed));

                OnFirebaseRemotConfigUpdated.Invoke(i_IsUpdated);
            }
        }

        private void onFirebaseRemotConfigUpdatedCompletionDelayed()
        {
            onFirebaseRemotConfigUpdatedCompletion(false);
        }

        //        private void byeApp()
        //        {
        //            Debug.LogError("Amazing Game");

        //            Application.Quit();

        //#if UNITY_EDITOR
        //            UnityEditor.EditorApplication.isPlaying = false;
        //#endif
        //        }
    }
}
