#if ENABLE_APPSFLYER
using AppsFlyerSDK;
#endif
#if ENABLE_FACEBOOK
using Facebook.Unity;
#endif
#if ENABLE_FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
#endif
#if ENABLE_GAMEANALYTICS
using GameAnalyticsSDK;
#endif
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using Product = UnityEngine.Purchasing.Product;
#endif
#if ENABLE_BEPIC || ENABLE_KOBIC
using KobGamesSDKSlim.Bepic;
#endif

namespace KobGamesSDKSlim
{
    [ExecutionOrder(eExecutionOrder.AnalyticsManager)]
    public class AnalyticsManagerBase : Singleton<AnalyticsManager>
#if ENABLE_APPSFLYER
        , IAppsFlyerConversionData
#endif
    {
        protected const string k_AM_LevelStart = "level_start";
        protected const string k_AM_LevelFinish = "level_finish";
        protected const string k_AM_SkinUnlock = "skin_unlock";
        protected const string k_AM_iAPPurchased = "payment_succeed";
        protected const string k_AM_iAPWindow = "no_ads_window";
        protected const string k_AM_Store = "store";
        protected const string k_AM_SlotMachine = "slot_machine";
        protected const string k_AM_RateUs = "rate_us";
        protected const string k_AM_AdsWatched = "video_ads_watch";

        protected const string k_AF_AppLaunch = "app_launch";
        protected const string k_AF_LevelStart = "level_start";
        protected const string k_AF_LevelComplete = "level_complete";
        protected const string k_AF_LevelFail = "level_fail";
        protected const string k_AF_LevelCompleteAppsFlyerFormat = "af_level_{0}";
        protected const string k_AF_Purchase = "af_purchase";
        protected const string k_AF_AdRewardVideo = "ad_reward";
        protected const string k_AF_AdInterstitial = "ad_intersititial";
        protected const string k_AF_AdBanner = "ad_banner";
        protected const string k_AdRewardVideo = "ad_reward";
        protected const string k_AdInterstitial = "ad_intersititial";
        protected const string k_AdBanner = "ad_banner";
        protected const string k_AF_SpentCredits = "af_spent_credits";
        protected const string k_AdShow = "ad_show";

        public bool SendGameLaunchEvent = true;
        public bool SendGameLaunchEventAppsFlyer = false;
        public bool SendAdsBannersEvents = false;
        public bool SendAdsEventsAppsFlyer = false;
        public bool SendiAPEventsAppsFlyer = false;

        #region AnalyticsServices
        [EnumToggleButtons]
        public eAnalyticsServices AnalyticsServices = eAnalyticsServices.ALL;

        public bool IsGoogleAnalyticsEnabled { get { return (AnalyticsServices & eAnalyticsServices.GoogleAnalytics) == eAnalyticsServices.GoogleAnalytics; } }
        public bool IsFirebaseAnalyticsEnabled { get { return (AnalyticsServices & eAnalyticsServices.Firebase) == eAnalyticsServices.Firebase; } }
        public bool IsGameAnalyticsEnabled { get { return (AnalyticsServices & eAnalyticsServices.GameAnalytics) == eAnalyticsServices.GameAnalytics; } }
        public bool IsFacebookAnalyticsEnabled { get { return (AnalyticsServices & eAnalyticsServices.Facebook) == eAnalyticsServices.Facebook; } }
        public bool IsAppsFlyerEnabled { get { return GameSettings.Instance.General.AppsFlyerId != string.Empty; } }
        public bool IsAdjustEnabled { get { return GameSettings.Instance.General.AdjustId != string.Empty; } }
        public bool IsAppMetricaEnabled { get { return GameSettings.Instance.General.AppMetricaId != string.Empty; } }
        #endregion

        public static Action OnFirebaseInitializedEvent = delegate { };

        public StorageManager StorageData { get { return Managers.Instance.Storage; } }
        public bool IsCurrentLevelZeroBased = true;

        [ShowInInspector] public int CurrentLevel { get { return IsCurrentLevelZeroBased ? Managers.Instance.Storage.CurrentLevel + 1 : Managers.Instance.Storage.CurrentLevel; } }
        [ShowInInspector] public int HighScoreLevel { get { return IsCurrentLevelZeroBased ? Managers.Instance.Storage.HighScoreLevel + 1 : Managers.Instance.Storage.HighScoreLevel; } }

#if ENABLE_FACEBOOK
        protected string m_GameLaunchDetails { get { return $"GameLaunch: {StorageData.GameLaunchCount} HighScoreLevel: {HighScoreLevel} TotalAttempts: {StorageData.TotalAttempts} TotalVideosWatched: {StorageData.TotalVideosWatched} FbIds: {Facebook.Unity.Settings.FacebookSettings.AppIds?.GetItem(0)} {Facebook.Unity.Settings.FacebookSettings.AppIds?.GetItem(1)} Name: {Application.productName} Config: {Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value}"; } }
#else
        protected string m_GameLaunchDetails { get { return $"GameLaunch: {StorageData.GameLaunchCount} HighScoreLevel: {HighScoreLevel} TotalAttempts: {StorageData.TotalAttempts} TotalVideosWatched: {StorageData.TotalVideosWatched} Name: {Application.productName} Config: {Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value}"; } }
#endif
        protected string m_SessionDetails { get { return $"CurrentLevel: {CurrentLevel} SessionAttempts: {StorageData.GameOverSessionAttempts} TotalAttempts: {StorageData.TotalAttempts}"; } }

        protected bool m_IsGameAnalyticsInitialized = false;
        protected bool m_IsFirebaseInitialized = false;
        protected bool m_IsFacebookAnalyticsInitialized = false;
        protected bool m_IsAppsFlyerInitialized = false;

#if ENABLE_FIREBASE
        private List<Parameter> m_ParametersList = new List<Parameter>();
#endif

        protected Dictionary<string, object> m_AppMetricaParams = new Dictionary<string, object>();
        protected Dictionary<string, object> m_AnalyticsDictionary = new Dictionary<string, object>();
        protected Dictionary<string, string> m_FbPropertyDictionary = new Dictionary<string, string>();
        private kv m_DummyKV = new kv();

        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            InitAnalyticServices();
        }

        public override void Start()
        {
            base.Start();

#if ENABLE_BEPIC || ENABLE_KOBIC
            PPWrapper.Init();
#endif

#if ENABLE_ADS
            if (SendAdsBannersEvents)
            {
                AdsManager.BannerEventsGlobal.ShownEvent += LogBannerShownSuccess;
                AdsManager.BannerEventsGlobal.FailedEvent += LogBannerShownFailed;
            }
            AdsManager.InterstitialEventsGlobal.ShownEvent += LogInterstitialShownSuccess;
            AdsManager.InterstitialEventsGlobal.ShownFailEvent += LogInterstitialShownFailed;
            AdsManager.RewardVideoEventsGlobal.SuccessEvent += LogRewardVideoShownSuccess;
            AdsManager.RewardVideoEventsGlobal.FailEvent += LogRewardVideoShownFailed;
#endif
            OnAppLaunch();
        }

        /// <summary>
        /// Called only once per game launch
        /// </summary>
        public void OnAppLaunch()
        {
            bool isIOS = false;

#if UNITY_IOS
            isIOS = true;
#endif

            // Delay init, to allow 15 seconds to get ATT consent approval by user - only for IOS
            Invoke(nameof(InitAppsFlyer), GameConfig.Instance.IsIOSATT && isIOS ? 15 : 0);

            // If firebase is disabled we call it manually, so other analytics services could behave accordingly 
            if (!IsFirebaseAnalyticsEnabled)
            {
                logAppLaunchEventDelayed();
            }
        }

        public virtual void InitAppsFlyer()
        {
#if ENABLE_APPSFLYER
            string appId = Application.platform == RuntimePlatform.IPhonePlayer ? GameSettings.Instance.General.AppStoreId : GameSettings.Instance.General.GooglePlayId;

            Debug.Log($"AppsFlyer: Key: {GameSettings.Instance.General.AppsFlyerId} appId: {appId} (AppleId: {GameSettings.Instance.General.AppStoreId} AndroidId: {GameSettings.Instance.General.GooglePlayId})");

            AppsFlyer.setIsDebug(false);
            AppsFlyer.initSDK(GameSettings.Instance.General.AppsFlyerId, appId, this);

            AppsFlyer.startSDK();

            m_IsAppsFlyerInitialized = true;
#endif
        }

        public virtual void InitAnalyticServices()
        {
#if ENABLE_FACEBOOK
            // Facebook
            if (IsFacebookAnalyticsEnabled)
            {
                if (!FB.IsInitialized)
                {
                    FacebookInit(OnFacebookAnalyticsInitialized);
                }
            }
#endif

#if ENABLE_GAMEANALYTICS
            // GameAnalytics
            if (IsGameAnalyticsEnabled)
            {
                if (!m_IsGameAnalyticsInitialized)
                {
                    m_IsGameAnalyticsInitialized = true;

                    GameAnalytics.SettingsGA.CustomDimensions01 = GameSettings.Instance.Analytics.GameAnalyticsCustomDimensions01;
                    GameAnalytics.SettingsGA.CustomDimensions02 = GameSettings.Instance.Analytics.GameAnalyticsCustomDimensions02;
                    GameAnalytics.SettingsGA.CustomDimensions03 = GameSettings.Instance.Analytics.GameAnalyticsCustomDimensions03;
                    GameAnalytics.Initialize();

                    // Allowing to send up to 100 errors during a game session (default was 10)
                    GameAnalyticsSDK.Events.GA_Debug.MaxErrorCount = 100;

                    OnGameAnalyticsInitialized();
                }
            }
#endif

            // Firebase
            if (IsFirebaseAnalyticsEnabled)
            {
                if (!m_IsFirebaseInitialized)
                {
                    OnFirebaseInitializedEvent += logAppLaunchEventDelayed;
                    RemoteSettingsManager.OnFirebaseRemotConfigUpdated += OnFirebaseRemotConfigUpdated;

                    InitFirebase();
                }
            }
        }

        public virtual void OnFirebaseRemotConfigUpdated(bool i_UpdateResult)
        {
            this.SetProperty("ConfigVersion", Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value);

            //Debug.Log("Analytics: OnFirebaseRemotConfigUpdated");
#if ENABLE_GAMEANALYTICS
            if (IsGameAnalyticsEnabled && m_IsGameAnalyticsInitialized)
            {
                GameAnalytics.SetCustomDimension01($"ConfigVersion_{Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value}");
                GameAnalytics.NewDesignEvent($"ConfigVersion_{Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value}");

                //Debug.Log("Analytics: OnGameAnalyticsInitialized");
            }
#endif

            //#if ENABLE_FACEBOOK
            //            if (IsFacebookAnalyticsEnabled && m_IsFacebookAnalyticsInitialized)
            //            {
            //                m_AnalyticsDictionary.Clear();
            //                m_AnalyticsDictionary.Add("Version", Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value);
            //                FB.LogAppEvent("ConfigVersion", null, m_AnalyticsDictionary);

            //                //Debug.Log("Analytics: OnFacebookAnalyticsInitialized");
            //            }
            //#endif
        }

        public virtual void OnGameAnalyticsInitialized() { }

        public virtual void OnFacebookAnalyticsInitialized() { }

        private void logAppLaunchEventDelayed()
        {
            // Give a chance to all analytics system to init
            Invoke(nameof(LogAppLaunchEvent), 2);
        }

        protected virtual void LogAppLaunchEvent()
        {
            Debug.Log($"{this.name}-{Utils.GetFuncName()}(): {m_GameLaunchDetails}");
#if ENABLE_GAMEANALYTICS
            Debug.Log($"{this.name}-{Utils.GetFuncName()}(): GameAnalytics iOS: '{GameSettings.Instance.Analytics.GAIOSName}'  GameAnalytics Android: '{GameSettings.Instance.Analytics.GAAndroidName}'");
#endif

            if (SendGameLaunchEvent)
            {
                LogEvent(Utils.GetFuncName(), m_GameLaunchDetails, "None", 0,
                    kv(nameof(StorageData.GameLaunchCount), StorageData.GameLaunchCount),
                    kv(nameof(StorageData.HighScoreLevel), StorageData.HighScoreLevel),
                    kv(nameof(StorageData.TotalAttempts), StorageData.TotalAttempts),
                    kv(nameof(StorageData.TotalVideosWatched), StorageData.TotalVideosWatched),
                    kv("FacebookAppIds", GetFacebookAppIds()));

#if ENABLE_APPSFLYER
                if (SendGameLaunchEventAppsFlyer)
                {
                    LogAppsFlyerEvent(k_AF_AppLaunch, new Dictionary<string, string>() { { nameof(StorageData.GameLaunchCount), StorageData.GameLaunchCount.ToString() } });
                }
#endif
            }

            //this.SetProperty("UDID", SystemInfo.deviceUniqueIdentifier);
        }

        public void InitFirebase()
        {
#if ENABLE_FIREBASE
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {                
                if (task.Result == DependencyStatus.Available)
                {
                    Debug.Log($"Firebase-Init: Available");

                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                    m_IsFirebaseInitialized = true;
                    
                    OnFirebaseInitialized();                    

                    //Old
                    //DoOnMainThread.QueueOnMainThread(OnFirebaseInitialized);
                }
                else
                {
                    Debug.LogError($"Firebase-Init: Could not resolve all Firebase dependencies: {task.Result}");

                    m_IsFirebaseInitialized = false;
                    // Firebase Unity SDK is not safe to use here.
                }
            });
#endif
        }

        private void OnFirebaseInitialized()
        {
            Debug.Log($"Firebase-OnFirebaseInitialized() Setting Crashlytics");

#if ENABLE_FIREBASE
            //Crashlytics.SetUserId(SystemInfo.deviceUniqueIdentifier);
#endif
            Debug.Log($"Firebase-OnFirebaseInitialized() Invoking OnFirebaseInitializedEvent");

            OnFirebaseInitializedEvent.InvokeSafe();
        }

#if ENABLE_FACEBOOK
        public void FacebookInit(Action i_OnInitialized = null)
        {
            Debug.LogFormat("FB INIT: {0}", FB.IsInitialized);

            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                //Handle FB.Init
                FB.Init(() =>
                {
                    m_IsFacebookAnalyticsInitialized = true;
                    FB.ActivateApp();

                    i_OnInitialized.InvokeSafe();
                });
            }
        }
#endif

        public virtual void LevelLoaded()
        {
            // TBD - do we need to add LevelLoaded analytics event?
            //

            Managers.Instance.InteractionManager.RateUsPopup.TryShowRateUsOnSpecificLevel(CurrentLevel, ePopupMode.LevelLoad);
        }

        public virtual void LevelStarted()
        {
            LogEvent(Utils.GetFuncName(), "LevelId", CurrentLevel.ToString("00"), 0,
                    kv(nameof(StorageData.CurrentScore), StorageData.CurrentScore),
                    kv(nameof(StorageData.HighScore), StorageData.HighScore),
                    kv(nameof(StorageData.CurrentLevel), CurrentLevel),
                    kv(nameof(StorageData.HighScoreLevel), HighScoreLevel),
                    kv(nameof(StorageData.CurrentLevelAttempts), StorageData.CurrentLevelAttempts)
                );

#if ENABLE_GAMEANALYTICS
            //GameAnalytics.SetCustomDimension03("GamePlayed");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, CurrentLevel.ToString("00"));
#endif


#if ENABLE_APPMETRICA
            LogAppMetricaEvent(
                k_AM_LevelStart, 
                new Tuple<string, object>("fps", FPSManager.FPSAverageLastUpdate),
                new Tuple<string, object>("fps_ticks", FPSManager.FPSTicksLastUpdate));
#endif
        }

        public virtual void LevelCompleted()
        {
            LogEvent(Utils.GetFuncName(), "LevelId", CurrentLevel.ToString("00"), 0,
                    kv(nameof(StorageData.CurrentScore), StorageData.CurrentScore),
                    kv(nameof(StorageData.HighScore), StorageData.HighScore),
                    kv(nameof(StorageData.CurrentLevel), CurrentLevel),
                    kv(nameof(StorageData.HighScoreLevel), HighScoreLevel),
                    kv(nameof(StorageData.CurrentLevelAttempts), StorageData.CurrentLevelAttempts)
                );

#if ENABLE_GAMEANALYTICS
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, CurrentLevel.ToString("00"));
#endif


#if ENABLE_APPMETRICA
            LogAppMetricaEvent(k_AM_LevelFinish,
                new Tuple<string, object>("result", "win"),
                new Tuple<string, object>("fps", FPSManager.FPSAveragePreviousLevel),
                new Tuple<string, object>("fps_ticks", FPSManager.FPSTickPreviousLevel));
#endif

#if ENABLE_BEPIC
            PPWrapper.LogLevelCompleted(CurrentLevel);
#endif
            Managers.Instance.InteractionManager.RateUsPopup.TryShowRateUsOnSpecificLevel(CurrentLevel, ePopupMode.LevelComplete);
        }

        public virtual void LevelFailed()
        {
            LogEvent(Utils.GetFuncName(), "LevelId", CurrentLevel.ToString("00"), 0,
                   kv(nameof(StorageData.CurrentScore), StorageData.CurrentScore),
                   kv(nameof(StorageData.HighScore), StorageData.HighScore),
                   kv(nameof(StorageData.CurrentLevel), CurrentLevel),
                   kv(nameof(StorageData.HighScoreLevel), HighScoreLevel),
                   kv(nameof(StorageData.GameOverSessionAttempts), StorageData.GameOverSessionAttempts),
                   kv(nameof(StorageData.TotalAttempts), StorageData.TotalAttempts),
                   kv(nameof(StorageData.CurrentLevelAttempts), StorageData.CurrentLevelAttempts)
                );

#if ENABLE_GAMEANALYTICS
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, CurrentLevel.ToString("00"));
#endif

#if ENABLE_APPMETRICA
            LogAppMetricaEvent(k_AM_LevelFinish,
                new Tuple<string, object>("result", "lose"),
                new Tuple<string, object>("fps", FPSManager.FPSAveragePreviousLevel),
                new Tuple<string, object>("fps_ticks", FPSManager.FPSTickPreviousLevel));
#endif
        }

        public virtual void LevelContinued()
        {
            LogEvent(Utils.GetFuncName(), "LevelId", CurrentLevel.ToString("00"), 0,
                   kv(nameof(StorageData.CurrentScore), StorageData.CurrentScore),
                   kv(nameof(StorageData.HighScore), StorageData.HighScore),
                   kv(nameof(StorageData.CurrentLevel), CurrentLevel),
                   kv(nameof(StorageData.HighScoreLevel), HighScoreLevel),
                   kv(Managers.Instance.RemoteSettings.FirebaseRemoteConfig.MaxContinueAmount.Key, Managers.Instance.RemoteSettings.FirebaseRemoteConfig.MaxContinueAmount.Value),
                   kv(nameof(Managers.Instance.GameManager.CurrContinueAmount), Managers.Instance.GameManager.CurrContinueAmount));
        }

        #region iAP_Events
#if UNITY_PURCHASING
        public virtual void OnIAP_PurchaseComplete(Product i_Product)
        {
            if (i_Product != null)
            {
                string logMessage = string.Format("OnPurchaseComplete - IsRestore: {0} Product Id: {1} availableToPruchase: {2} hasReceipt: {3} transactionID: {4} metadata: {5}",
                         false,
                         i_Product.definition.storeSpecificId,
                         i_Product.availableToPurchase,
                         i_Product.hasReceipt,
                         i_Product.transactionID,
                         i_Product.metadata.localizedPrice);

                double decimalPrice = 0;
                try
                {
                    decimalPrice = System.Convert.ToDouble(i_Product.metadata.localizedPrice);
                }
                catch { }

                Debug.Log(logMessage);
                LogEvent("iAP", string.Format("Purchase Complete-{0}", i_Product.definition.storeSpecificId), logMessage, 0,
                    kv("State", "Success"),
                    kv("Type", i_Product.definition.storeSpecificId));

#if ENABLE_GAv4
                GoogleAnalyticsV4.instance.LogItem(i_Product.transactionID, i_Product.definition.storeSpecificId, "0", "iAP", decimalPrice, 1);
#endif

#if ENABLE_APPSFLYER
                if (SendiAPEventsAppsFlyer)
                {
                    LogAppsFlyerEvent(k_AF_Purchase, new Dictionary<string, string>() {
                        { "ProductID", i_Product.definition.storeSpecificId },
                        { "HasReceipt", i_Product.hasReceipt.ToString() },
                        { "TransactionID", i_Product.transactionID.ToString() },
                        { "Price", i_Product.metadata.localizedPrice.ToString() }
                    });
                }
#endif
            }
        }

        public virtual void OnIAP_PurchaseFailed(Product i_Product, PurchaseFailureReason i_Reason)
        {
            if (i_Product != null)
            {
                string logMessage = string.Format("OnPurchaseFailed - IsRestore: {0}, Reason: {1}, Product Id: {2} availableToPruchase: {3} hasReceipt: {4} transactionID: {5} metadata: {6}",
                        false,
                        i_Reason.ToString(),
                        i_Product.definition.storeSpecificId,
                        i_Product.availableToPurchase,
                        i_Product.hasReceipt,
                        i_Product.transactionID,
                        i_Product.metadata.localizedPrice);

                Debug.Log(logMessage);
                LogEvent("iAP", string.Format("Purchase Failed-{0}", i_Product.definition.storeSpecificId), logMessage, 0,
                    kv("State", "Failed"),
                    kv("Type", i_Product.definition.storeSpecificId));
            }
        }

        public virtual void OnIAP_PurchaseRestoreComplete(Product i_Product)
        {
            if (i_Product != null)
            {
                string logMessage = string.Format("OnPurchaseRestoreComplete - IsRestore: {0} Product Id: {1} availableToPruchase: {2} hasReceipt: {3} transactionID: {4} metadata: {5}",
                         true,
                         i_Product.definition.storeSpecificId,
                         i_Product.availableToPurchase,
                         i_Product.hasReceipt,
                         i_Product.transactionID,
                         i_Product.metadata.localizedPrice);

                Debug.Log(logMessage);
                LogEvent("iAP", string.Format("Restore Complete-{0}", i_Product.definition.storeSpecificId), logMessage, 0,
                    kv("State", "RestoreComplete"),
                    kv("Type", i_Product.definition.storeSpecificId));
            }
        }

        public virtual void OnIAP_PurchaseRestoreFailed(Product i_Product, PurchaseFailureReason i_Reason)
        {
            if (i_Product != null)
            {
                string logMessage = string.Format("OnPurchaseRestoreFailed - IsRestore: {0}, Reason: {1}, Product Id: {2} availableToPruchase: {3} hasReceipt: {4} transactionID: {5} metadata: {6}",
                    true,
                    i_Reason.ToString(),
                    i_Product.definition.storeSpecificId,
                    i_Product.availableToPurchase,
                    i_Product.hasReceipt,
                    i_Product.transactionID,
                    i_Product.metadata.localizedPrice);

                Debug.Log(logMessage);
                LogEvent("iAP", string.Format("Restore Failed-{0}", i_Product.definition.storeSpecificId), logMessage, 0,
                    kv("State", "RestoreFailed"),
                    kv("Type", i_Product.definition.storeSpecificId));
            }
        }
#endif
        #endregion

        public virtual void LogRateUsEvent(string i_RateUsResult, int i_NumberOfStars)
        {
            LogEvent("RateUs", i_RateUsResult, "None", 0);

            LogAppMetricaEvent(k_AM_RateUs, new Tuple<string, object>("rate_result", i_NumberOfStars));
        }

        public virtual void LogRateUsEvent(string i_RateUsResult)
        {
            LogEvent("RateUs", i_RateUsResult, "None", 0);

            LogAppMetricaEvent(k_AM_RateUs, new Tuple<string, object>("rate_result", i_RateUsResult));
        }

        public virtual void LogOpenUrlEvent()
        {
            LogEvent("OpenUrlAppPage", "None", "None", 0);
        }

        // Unity will call OnApplicationPause(false) when an app is resumed
        // from the background
        void OnApplicationPause(bool i_PauseStatus)
        {
            LogScreen("OnApplicationPause_" + (i_PauseStatus == true ? "True" : "False"));
            LogEvent("OnApplicationPause", (i_PauseStatus == true ? "True" : "False"));

#if ENABLE_FACEBOOK
            // Check the pauseStatus to see if we are in the foreground            
            if (!i_PauseStatus && FB.IsInitialized)
            {
                //App resume
                FB.ActivateApp();
            }
#endif
        }

        public virtual void LogEvent(string i_EventCategory, string i_EventAction, string i_EventLabel = "None", long i_Value = 0, params kv[] i_FirebaseEventTypes)
        {
            Debug.LogFormat("Analytics.LogEvent: {0} {1} {2} {3} FirebaseParams: {4}", i_EventCategory, i_EventAction, i_EventLabel, i_Value, i_FirebaseEventTypes.Length);

            if (Application.isEditor) return;

#if ENABLE_FIREBASE
            if (IsFirebaseAnalyticsEnabled)
            {
                if (m_IsFirebaseInitialized)
                {
                    if (i_FirebaseEventTypes.Length > 0)
                    {
                        m_ParametersList.Clear();

                        setParametersList(ref m_ParametersList, i_FirebaseEventTypes);

                        FirebaseAnalytics.LogEvent(i_EventCategory, m_ParametersList.ToArray());
                    }
                    else
                    {
                        FirebaseAnalytics.LogEvent(i_EventCategory, i_EventAction, i_EventLabel);
                    }
                }
            }
#endif

#if ENABLE_GAv4
            if (IsGoogleAnalyticsEnabled)
            {
                if (GoogleAnalyticsV4.instance != null)
                {
                    GoogleAnalyticsV4.instance.LogEvent(i_EventCategory, i_EventAction, i_EventLabel, i_Value);
                }
                else
                {
                    Debug.LogFormat("{0}:{1}, GoogleAnalyticsV4.instance is null", this.name, nameof(LogEvent));
                }
            }
#endif

            //#if ENABLE_FACEBOOK
            //            if (IsFacebookAnalyticsEnabled)
            //            {
            //                if (m_IsFacebookAnalyticsInitialized)
            //                {
            //                    if (m_AnalyticsDictionary == null)
            //                        m_AnalyticsDictionary = new Dictionary<string, object>();

            //                    m_AnalyticsDictionary.Clear();

            //                    if (i_EventLabel == "None")
            //                    {
            //                        m_AnalyticsDictionary.Add(i_EventCategory, i_EventAction);
            //                    }
            //                    else
            //                    {
            //                        m_AnalyticsDictionary.Add(i_EventAction, i_EventLabel);
            //                    }

            //                    if (i_FirebaseEventTypes.Length > 0)
            //                    {
            //                        setFbParametersList(ref m_AnalyticsDictionary, i_FirebaseEventTypes);
            //                    }

            //                    FB.LogAppEvent(i_EventCategory, null, m_AnalyticsDictionary);
            //                }
            //            }
            //#endif

#if ENABLE_GAMEANALYTICS
            if (IsGameAnalyticsEnabled)
            {
                if (m_IsGameAnalyticsInitialized)
                {
                    if (i_EventCategory == nameof(LogAppLaunchEvent))
                    {
                        LogGameAnalyticsEvent("LogAppLaunchEvent", "GameLaunchCount", StorageData.GameLaunchCount.ToString(), StorageData.TotalAttempts);
                    }
                    else
                    {
                        LogGameAnalyticsEvent(i_EventCategory, i_EventAction, i_EventLabel, i_Value, i_FirebaseEventTypes);
                    }
                }
            }
#endif
        }


        public virtual void LogGameAnalyticsEvent(string i_EventCategory, string i_EventAction = null, string i_EventLabel = null, float i_Value = 0, params kv[] i_FirebaseEventTypes)
        {
#if ENABLE_GAMEANALYTICS
            if (IsGameAnalyticsEnabled)
            {
                if (m_IsGameAnalyticsInitialized)
                {
                    if (i_EventAction != null && i_EventLabel != null)
                    {
                        GameAnalytics.NewDesignEvent($"{i_EventCategory}:{i_EventAction}:{i_EventLabel}", i_Value);
                    }
                    else if (i_EventAction != null)
                    {
                        GameAnalytics.NewDesignEvent($"{i_EventCategory}:{i_EventAction}", i_Value);
                    }
                    else
                    {
                        GameAnalytics.NewDesignEvent($"{i_EventCategory}", i_Value);
                    }
                }
            }
#endif
        }
        
        protected string buildGADesignEventName(params string[] elements)
        {
            if(elements.Length > 5)
                Debug.LogError("Too many elements for GA. Max should be 5");
            
            string eventName = "";
            for (int i = 0; i < elements.Length; i++)
            {
                eventName += elements[i];
                if (i < elements.Length - 1)
                    eventName += ":";
            }

            return eventName;
        }

        public virtual void LogGAException(string i_FuctionName, string i_ExMessage, string i_StackTrace)
        {
#if ENABLE_GAMEANALYTICS
            if (IsGameAnalyticsEnabled)
            {
                GameAnalyticsSDK.Events.GA_Debug.MaxErrorCount += 5;
                GameAnalyticsSDK.Events.GA_Debug.HandleLog($"{i_FuctionName} Error: {i_ExMessage}", i_StackTrace, LogType.Exception);
            }
#endif
        }

        public void InitAppMetricaParamsDic()
        {
            m_AppMetricaParams.Clear();
            m_AppMetricaParams.Add("level", CurrentLevel);
        }


        private string m_AppMetricaTupleLogs = string.Empty;
        public virtual void LogAppMetricaEvent(string i_EventName, params Tuple<string, object>[] i_KeyPairs)
        {
#if ENABLE_APPMETRICA
            m_AppMetricaTupleLogs = string.Empty;

            Debug.Log($"AppMetrica: {i_EventName} LevelId: {CurrentLevel}");

            InitAppMetricaParamsDic();

            if (i_KeyPairs != null && i_KeyPairs.Length > 0)
            {
                foreach (var kp in i_KeyPairs)
                {
                    m_AppMetricaParams.Add(kp.Item1, kp.Item2);
                    m_AppMetricaTupleLogs += $" ({kp.Item1}: {kp.Item2}) ";
                }
            }

            if (m_AppMetricaTupleLogs != string.Empty)
            {
                Debug.Log($"AppMetrica-Vars: [{m_AppMetricaTupleLogs}]");
            }

            AppMetrica.Instance.ReportEvent(i_EventName, m_AppMetricaParams);
            AppMetrica.Instance.SendEventsBuffer();
#endif
        }

        public void LogScreen(string i_ScreenName)
        {
#if ENABLE_FIREBASE
            if (IsFirebaseAnalyticsEnabled)
            {
                if (m_IsFirebaseInitialized)
                {
                    FirebaseAnalytics.SetCurrentScreen(i_ScreenName, "Custom");

                    Debug.LogFormat("Analytics.LogEvent Screen: {0}", i_ScreenName);
                }
            }
#endif
        }

        public void LogLevelProgress(params kv[] i_Params)
        {
            LogEvent(Utils.GetFuncName(), "LevelId", CurrentLevel.ToString("00"), 0, i_Params);
        }

        public void LogOpenFromNotification(string i_Name)
        {
            LogEvent(Utils.GetFuncName(), i_Name);
        }

        public void LogEventCritical(string i_EventAction, string i_EventLabel, long i_Value)
        {
#if ENABLE_FIREBASE
            if (m_IsFirebaseInitialized)
            {
                FirebaseAnalytics.LogEvent("Critical", i_EventAction, i_EventLabel);
            }
#endif

#if ENABLE_GAv4
            if (GoogleAnalyticsV4.instance != null)
            {
                GoogleAnalyticsV4.instance.LogEvent("Critical", i_EventAction, i_EventLabel, i_Value);
            }
            else
            {
                Debug.LogFormat("{0}:{1}, GoogleAnalyticsV4.instance is null", this.name, nameof(LogEventCritical));
            }
#endif
        }

        // Ads
        private const string k_Type = "Type";
        private const string k_Placement = "Placement";
        private const string k_Success = "Success";
        protected virtual void LogBannerShownSuccess(string i_PlacementName)
        {
            if (SendAdsBannersEvents)
            {
                //Debug.Log("YO LogBannerShown");

                LogEvent(k_AdShow, "banner", "true", 0,
                    kv(k_Type, "banner"),
                    kv(k_Placement, i_PlacementName),
                    kv(k_Success, "true"));

#if ENABLE_APPSFLYER
            if (SendAdsEventsAppsFlyer)
            {
                LogAppsFlyerEvent(k_AF_AdBanner, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Placement, i_PlacementName } });
                LogAppsFlyerEvent(k_AdShow, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Type, "banner" }, { k_Placement, i_PlacementName }, { k_Success, "true" } });
            }
#endif
            }
        }

#if ENABLE_ADS
        protected virtual void LogBannerShownFailed(string i_PlacementName, IAdNetworkError i_IronSourceError)
        {
            if (SendAdsBannersEvents)
            {
                //Debug.Log("YO LogBannerShown");

                LogEvent(k_AdShow, "banner", "false", 0,
                    kv(k_Type, "banner"),
                    kv(k_Placement, i_PlacementName),
                    kv(k_Success, "false"),
                    kv("ErrCode", i_IronSourceError.getCode()),
                    kv("ErrDesc", i_IronSourceError.getDescription()));

#if ENABLE_APPSFLYER
            if (SendAdsEventsAppsFlyer)
            {
                LogAppsFlyerEvent(k_AdShow, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Type, "banner" }, { k_Placement, i_PlacementName }, { k_Success, "false" }, { "ErrCode", i_IronSourceError.getCode().ToString() }, { "ErrDesc", i_IronSourceError.getDescription() } });
            }
#endif
            }
        }
#endif

#if ENABLE_ADS
        protected virtual void LogInterstitialShownSuccess(string i_PlacementName)
        {
            //Debug.Log("YO LogInterstitialShown");

            LogEvent(k_AdShow, "interstitial", "true", 0,
                    kv(k_Type, "interstitial"),
                    kv(k_Placement, i_PlacementName),
                    kv(k_Success, "true"));

#if ENABLE_GAMEANALYTICS
            GameAnalytics.NewDesignEvent($"{k_AdShow}:interstitial:{i_PlacementName}");
#endif

#if ENABLE_APPSFLYER
            if (SendAdsEventsAppsFlyer)
            {
                LogAppsFlyerEvent(k_AF_AdInterstitial, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Placement, i_PlacementName } });
                LogAppsFlyerEvent(k_AdShow, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Placement, i_PlacementName }, { k_Success, "true" } });
            }
#endif

#if ENABLE_APPMETRICA
            LogAppMetricaEvent(k_AM_AdsWatched,
                                        new Tuple<string, object>("ad_type", "interstitial"),
                                        new Tuple<string, object>("placement", i_PlacementName),
                                        new Tuple<string, object>("result", "watched"));
#endif
        }
#endif

#if ENABLE_ADS
        protected virtual void LogInterstitialShownFailed(string i_PlacementName, IAdNetworkError i_IronSourceError)
        {
            //Debug.Log("YO LogInterstitialShown");

            LogEvent(k_AdShow, "interstitial", "false", 0,
                    kv(k_Type, "interstitial"),
                    kv(k_Placement, i_PlacementName),
                    kv(k_Success, "false"));

#if ENABLE_APPSFLYER
            if (SendAdsEventsAppsFlyer)
            {
                LogAppsFlyerEvent(k_AdShow, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Placement, i_PlacementName }, { k_Success, "false" }, { "ErrCode", i_IronSourceError.getCode().ToString() }, { "ErrDesc", i_IronSourceError.getDescription() } });
            }
#endif

#if ENABLE_APPMETRICA
            LogAppMetricaEvent(k_AM_AdsWatched,
                                        new Tuple<string, object>("ad_type", "interstitial"),
                                        new Tuple<string, object>("placement", i_PlacementName),
                                        new Tuple<string, object>("result", "failed"));
#endif
        }
#endif

        protected virtual void LogRewardVideoShownSuccess(string i_PlacementName)
        {
            //Debug.Log("YO LogRewardVideoShown, " + i_PlacementName);

            LogEvent(k_AdShow, "rewardVideo", "True", 0,
                kv(k_Type, "reward"),
                kv(k_Placement, i_PlacementName),
                kv(k_Success, true));

#if ENABLE_GAMEANALYTICS
            GameAnalytics.NewDesignEvent($"{k_AdShow}:rewardVideo:{i_PlacementName}");
#endif

#if ENABLE_APPSFLYER
            if (SendAdsEventsAppsFlyer)
            {
                LogAppsFlyerEvent(k_AF_AdRewardVideo, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Placement, i_PlacementName } });
                LogAppsFlyerEvent(k_AdShow, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Type, "reward" }, { k_Placement, i_PlacementName }, { k_Success, "true" } });
            }
#endif

#if ENABLE_APPMETRICA
            LogAppMetricaEvent(k_AM_AdsWatched,
                                        new Tuple<string, object>("ad_type", "rewarded"),
                                        new Tuple<string, object>("placement", i_PlacementName),
                                        new Tuple<string, object>("result", "watched"));
#endif
        }

#if ENABLE_ADS
        protected virtual void LogRewardVideoShownFailed(string i_PlacementName, IAdNetworkError i_IronSourceError)
        {
            LogEvent(k_AdShow, "rewardVideo", "False", 0,
                kv(k_Type, "reward"),
                kv(k_Placement, i_PlacementName),
                kv(k_Success, false),
                kv("ErrCode", i_IronSourceError.getCode()),
                kv("ErrDesc", i_IronSourceError.getDescription()));

#if ENABLE_APPSFLYER
            if (SendAdsEventsAppsFlyer)
            {
                LogAppsFlyerEvent(k_AdShow, new Dictionary<string, string>() { { "Network", "ironSource" }, { k_Type, "reward" }, { k_Placement, i_PlacementName }, { k_Success, "false" }, { "ErrCode", i_IronSourceError.getCode().ToString() }, { "ErrDesc", i_IronSourceError.getDescription() } });
            }
#endif

#if ENABLE_APPMETRICA
            LogAppMetricaEvent(k_AM_AdsWatched,
                                        new Tuple<string, object>("ad_type", "rewarded"),
                                        new Tuple<string, object>("placement", i_PlacementName),
                                        new Tuple<string, object>("result", "failed"));
#endif
        }
#endif

#if ENABLE_FIREBASE
        private void setParametersList(ref List<Parameter> i_ParametersList, params kv[] i_KeyValueEvents)
        {
            foreach (var kv in i_KeyValueEvents)
            {
                if (kv.IsString)
                {
                    i_ParametersList.Add(new Parameter(kv.key, kv.valueString));
                }
                else if (kv.IsLong)
                {
                    i_ParametersList.Add(new Parameter(kv.key, kv.valueLong));
                }
                else if (kv.IsDouble)
                {
                    i_ParametersList.Add(new Parameter(kv.key, kv.valueDouble));
                }
            }
        }
#endif


        private void setFbParametersList(ref Dictionary<string, object> i_ParametersDic, params kv[] i_KeyValueEvents)
        {
            foreach (var kv in i_KeyValueEvents)
            {
                if (i_ParametersDic.ContainsKey(kv.key))
                    continue;

                if (kv.IsString)
                {
                    i_ParametersDic.Add(kv.key, kv.valueString);
                }
                else if (kv.IsLong)
                {
                    i_ParametersDic.Add(kv.key, kv.valueLong);
                }
                else if (kv.IsDouble)
                {
                    i_ParametersDic.Add(kv.key, kv.valueDouble);
                }
            }
        }

        protected string GetFacebookAppIds()
        {
#if ENABLE_FACEBOOK

            if (Facebook.Unity.Settings.FacebookSettings.AppIds.Count == 1)
            {
                return Facebook.Unity.Settings.FacebookSettings.AppIds[0];
            }
            else if (Facebook.Unity.Settings.FacebookSettings.AppIds.Count >= 2)
            {
                return Facebook.Unity.Settings.FacebookSettings.AppIds[0] + " " + Facebook.Unity.Settings.FacebookSettings.AppIds[1];
            }
#endif
            return string.Empty;
        }

#if ENABLE_APPSFLYER
        public void LogAppsFlyerEvent(string i_EventName, Dictionary<string, string> i_EventValue)
        {
            if (m_IsAppsFlyerInitialized)
            {
                AppsFlyer.sendEvent(i_EventName, i_EventValue);
            }
        }

        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }
#endif


        public void SetProperty(string i_Key, string i_Value)
        {
#if ENABLE_FIREBASE
            if (IsFirebaseAnalyticsEnabled && m_IsFirebaseInitialized)
            {
                FirebaseAnalytics.SetUserProperty(i_Key, i_Value);
            }
#endif

            //#if ENABLE_FACEBOOK
            //            if (IsFacebookAnalyticsEnabled && m_IsFacebookAnalyticsInitialized)
            //            {
            //                if (m_FbPropertyDictionary == null) m_FbPropertyDictionary = new Dictionary<string, string>();

            //                if (!m_FbPropertyDictionary.ContainsKey(i_Key))
            //                {
            //                    m_FbPropertyDictionary.Add(i_Key, i_Value);
            //                    //FB.Mobile.UserID = SystemInfo.deviceUniqueIdentifier;
            //                    FB.Mobile.UpdateUserProperties(m_FbPropertyDictionary);
            //                }
            //            }
            //#endif
        }

        #region KV HELPER
        public kv kv(string i_Key, string i_Value)
        {
            m_DummyKV.key = i_Key;
            m_DummyKV.SetValueString(i_Value);
            return m_DummyKV;
        }

        public kv kv(string i_Key, int i_Value)
        {
            m_DummyKV.key = i_Key;
            m_DummyKV.SetValueLong(i_Value);
            return m_DummyKV;
        }

        public kv kv(string i_Key, float i_Value)
        {
            m_DummyKV.key = i_Key;
            m_DummyKV.SetValueDouble(i_Value);
            return m_DummyKV;
        }

        public kv kv(string i_Key, bool i_Value)
        {
            m_DummyKV.key = i_Key;
            m_DummyKV.SetValueString(i_Value.ToString());
            return m_DummyKV;
        }

        #endregion

        //        #region HandleException
        //#if !UNITY_EDITOR
        //        protected void HandleException(string condition, string stackTrace, LogType type)
        //        {
        //            if (type == LogType.Exception)
        //            {
        //                Debug.LogError($"UnHandledException {type}: {condition}\n{stackTrace}");
        //                LogEvent("UnHandledException", $"{type}: {condition}\n{stackTrace}\n", $"Session Details: {m_SessionDetails}");
        //            }
        //        }
        //#endif
        //        #endregion
    }

    [Flags]
    public enum eAnalyticsServices
    {
        GoogleAnalytics = 1 << 2, // 2
        Firebase = 1 << 3, // 4
        GameAnalytics = 1 << 4, // 8
        Facebook = 1 << 5, // 16
        ALL = GoogleAnalytics | Firebase | GameAnalytics | Facebook
    }

    //public static class KVExtensions
    //{
    //    public static kv GetKV(this kv[] KVs, string i_Key)
    //    {
    //        for (int i=0; i<KVs.Length;i++)
    //        {
    //            if (KVs[i] != null)
    //                return null;
    //        }

    //        return null;
    //    }
    //}

    public struct kv
    {
        public string key;

        public bool IsString;
        public string valueString;

        public bool IsLong;
        public long valueLong;

        public bool IsDouble;
        public double valueDouble;

        public void SetValueString(string i_String)
        {
            ClearValues();

            IsString = true;

            valueString = i_String;
        }

        public void SetValueLong(long i_Long)
        {
            ClearValues();

            IsLong = true;

            valueLong = i_Long;
        }

        public void SetValueDouble(double i_Double)
        {
            ClearValues();

            IsDouble = true;

            valueDouble = i_Double;
        }

        public void ClearValues()
        {
            IsString = false;
            valueString = string.Empty;

            IsLong = false;
            valueLong = 0;

            IsDouble = false;
            valueDouble = 0;
        }
    }

    //#if !ENABLE_ADS
    //    // Dummy class to avoid script error in case Ads are not enabled
    //    public class IronSourceError
    //    {
    //        public int getCode() { return 0; }
    //        public string getDescription() { return string.Empty; }
    //        public int getErrorCode() { return 0; }
    //    }
    //#endif
}
