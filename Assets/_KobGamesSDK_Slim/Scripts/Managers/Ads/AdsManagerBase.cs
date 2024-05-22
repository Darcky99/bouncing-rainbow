using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
#if ENABLE_ADS
using static KobGamesSDKSlim.BannerEvents;
#endif
using static KobGamesSDKSlim.InterstitialEvents;
using static KobGamesSDKSlim.RewardVideoEvents;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace KobGamesSDKSlim
{
    public enum eRewardVideoCallResult
    {
        Success,
        AdsMediationNotInitialized,
        AdsMediationIsDisabledByRemoteSettings,
        RewardVideoIsDisabledByRemoteSettings,
        RewardVideoIsNotAvailable,
        OpenedRVConfirmationScreen
    }
#if ENABLE_ADS
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    [ExecutionOrder(eExecutionOrder.AdsManager)]
    public class AdsManagerBase : Singleton<AdsManager>
    {
        [Title("AdsMediation Configuration")]
        public bool GDPRConsent = true;

        [Title("General")]
        public bool AdsAlwaysAvailableInEditor = false;

        [Title("Banner")]
        public bool AutoShowBanner = true;
        public bool ShowDummyBgPanelEditor = true;
        public bool ShowDummyBgPanel = false;
        public float BannerDelayStart = 1f;
        public int RemoteConfigTimeoutSeconds = 0; //seconds

        [Title("Interstitials")]
        public bool ShowInterstitialPreview = true;
        [InlineEditor] public AdsInterstitialPreview AdsInterstitialPreview;

        [Title("Mediation Networks"), ShowInInspector, HideReferenceObjectPicker]
        public IMediationNetwork IMediationNetwork;
        [ReadOnly] public bool IsInit = false;

        [ShowInInspector, HideReferenceObjectPicker]
        public InterstitialHandler InterstitialHandler = new InterstitialHandler();

        // Delegates
        private BannerEvents m_BannerEventsDummy = new BannerEvents();
        private InterstitialEvents m_InterstitialEventsDummy = new InterstitialEvents();
        private RewardVideoEvents m_RewardVideoCallbacksDummy = new RewardVideoEvents();

        public static BannerEvents BannerEventsGlobal = new BannerEvents();
        public static InterstitialEvents InterstitialEventsGlobal = new InterstitialEvents();
        public static RewardVideoEvents RewardVideoEventsGlobal = new RewardVideoEvents();

        public static Action OnAdsMediationInitialized = () => { };
        public static Action OnBannerShown = () => { };
        public static Action OnBannerHidden = () => { };
        public static Action<bool> OnRewardVideoAvailabilityChange = (i_State) => { };
        public bool IsBannerVisible { get; private set; }

        [Title("Ads Debug Buttons")]
        [Button, PropertyOrder(20), LabelText("Show Interstitial Timed Based")] public void ShowInterstitialTimedBased_Debug() { InterstitialHandler.TryShowInterstitial(InterstitialHandler.InterstitialType.TimeBased); }
        [Button, PropertyOrder(20), LabelText("Show Interstitial Normal")] public void ShowInterstitialNormal_Debug() { InterstitialHandler.TryShowInterstitial(InterstitialHandler.InterstitialType.LevelCompleted); }
        [Button, PropertyOrder(20), LabelText("Show RewardVideo")] public void ShowRewardVideo_Debug() { ShowRewardVideo(null, null, "Debug"); }
        [Title("Ads Debug Buttons Invokes")]
        [Button, PropertyOrder(20)] public void InvokeInterstitialShown() { ShowInterstitial(); }
        [Button, PropertyOrder(20)] public void InvokeBannerShown() { ShowBanner(); }
        [Button, PropertyOrder(20)] public void InvokeBannerHidden() { HideBanner(); }
        [Button, PropertyOrder(20)] public void InvokeDestroyBanner() { DestroyBanner(); }

        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            // Making sure AdsAlwaysEnabledOnEditor is false out of editor
            AdsAlwaysAvailableInEditor = AdsAlwaysAvailableInEditor && Application.isEditor;

            SetRefs();

            RemoteSettingsManager.OnFirebaseRemotConfigUpdated += OnFirebaseRemotConfigUpdated;

            // On Game Launch let's set cooldown so we don't show interstitial right away
            // after game launch for advanced users
            Managers.Instance.Storage.InterstitialCoolDownCounter = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialCoolDown.Value;

            // Register Global Callbacks
            BannerEventsGlobal.ShownEvent += onBannerShown;
            BannerEventsGlobal.HiddenEvent += onBannerHidden;
            InterstitialEventsGlobal.ShownEvent += onInterstitialShown;
            RewardVideoEventsGlobal.CloseEvent += onRewardVideoClosed;

            OnRewardVideoAvailabilityChange += onRewardVideoAvailabilityChange;

            Invoke(nameof(RemoteConfigTimeout), RemoteConfigTimeoutSeconds);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (this.gameObject.scene.name == null)
                return;

            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Utils.EditorDelayRun(SetRefs);
            }
#endif
        }

        public void SetIMediationRefs()
        {
#if ENABLE_IRONSOURCE
                IMediationNetwork = this.GetComponentInChildren<IronSourceMediation>(true);
#elif ENABLE_MAX
            IMediationNetwork = this.GetComponentInChildren<MAXMediation>(true);
#elif ENABLE_SUPERERA
                IMediationNetwork = this.GetComponentInChildren<SupereraMediation>(true);
#elif ENABLE_BEPIC
                IMediationNetwork = this.GetComponentInChildren<BepicMediation>(true);
#elif ENABLE_KOBIC
                //IMediationNetwork = this.GetComponentInChildren<IronSourceMediation>(true);
#endif
        }

        [Button]
        public void SetRefs()
        {
            if (this == null) return;

            if (IMediationNetwork == null)
            {
                SetIMediationRefs();

#if UNITY_EDITOR
                // Let's try to add automatically
                if (IMediationNetwork == null)
                {
                    if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        if (GameSettings.Instance.AdsMediation.IsIronSourceEnabled)
                        {
                            AddIronSourceMediationPrefab();
                            Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Added ironSource Prefab automatically under AdsManager.");
                        }
                        else if (GameSettings.Instance.AdsMediation.IsMAXEnabled)
                        {
                            AddMaxMediationPrefab();
                            Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Added MAX Prefab automatically under AdsManager.");
                        }
                    }

                    SetIMediationRefs();
                }
#endif

                if (IMediationNetwork == null)
                {
                    Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Can't find reference for IMediationNetwork, Please fix.");
#if UNITY_EDITOR
                    m_ForceExitPlayMode = true;
#endif
                }
            }



#if UNITY_EDITOR
            if (this.AdsInterstitialPreview == null)
            {
                AddInterstitialPreviewPrefab();

                this.AdsInterstitialPreview = this.GetComponentInChildren<AdsInterstitialPreview>();
            }

#if ENABLE_BEPIC
                AddBepicAdMockupPrefab();
#endif
#endif

            int unityMainThreadDispatcherCount = this.GetComponents<UnityMainThreadDispatcher>().Length;
            if (unityMainThreadDispatcherCount == 0)
            {
                this.gameObject.AddComponent<UnityMainThreadDispatcher>();
            }
        }

        private const float k_RVAvailbilityCounterCheckerTimer = 2;
        private float m_RVAvailbilityCounterChecker = k_RVAvailbilityCounterCheckerTimer;
        private bool m_RVAvailabilityState = false;
        private static bool m_ForceExitPlayMode = false;
        public void Update()
        {
#if UNITY_EDITOR
            if (m_ForceExitPlayMode)
            {
                m_ForceExitPlayMode = false;

                UnityEditor.EditorApplication.ExitPlaymode();
                Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Forcing exit out of play mode.");
            }

#if !ENABLE_SHORTCUTS
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                ShowBanner();
            }

            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                HideBanner();
            }

            m_WasDebugKeyPressed = Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R);
#endif
#endif

            if (IsInit)
            {
                m_RVAvailbilityCounterChecker -= Time.deltaTime;
                if (m_RVAvailbilityCounterChecker <= 0)
                {
                    //Debug.LogError("Checking RV...");

                    // Reset Counter
                    m_RVAvailbilityCounterChecker = k_RVAvailbilityCounterCheckerTimer;

                    // Did state changed?
                    if (m_RVAvailabilityState != IsRewardVideoLoaded)
                    {
                        m_RVAvailabilityState = IsRewardVideoLoaded;

                        // Invoke event
                        OnRewardVideoAvailabilityChange.InvokeSafe(m_RVAvailabilityState);
                    }
                }
            }
        }
      
        private void onRewardVideoAvailabilityChange(bool i_State)
        {
            Debug.Log($"{this.name} : {Utils.GetFuncName()}() Reward Video Availability Changed to: {i_State}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            RemoteSettingsManager.OnFirebaseRemotConfigUpdated -= OnFirebaseRemotConfigUpdated;
        }

        private void RemoteConfigTimeout()
        {
            Debug.Log($"{this.name} : {Utils.GetFuncName()}() RemoteConfigTimeout Occur");

            InitMediation();
        }

        public void OnFirebaseRemotConfigUpdated(bool i_UpdateResult)
        {
            Debug.Log($"{this.name} : {Utils.GetFuncName()}() RemoteConfig Ready");

            CancelInvoke(nameof(RemoteConfigTimeout));

            InitMediation();
        }

        public bool IsAdsMediationRemotelyEnabled
        {
            get
            {
                if (AdsAlwaysAvailableInEditor)
                {
                    return true;
                }

                if (Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsAdsEnabled.Value == false)
                {
                    Debug.Log($"{this.name} : {Utils.GetFuncName()}, Ads are disabled by remote");
                }

                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsAdsEnabled.Value;
            }
        }

        public bool IsRemovedAds
        {
            get
            {
                if (Managers.Instance.Storage.RemovedAds)
                {
                    Debug.Log($"{this.name} : {Utils.GetFuncName()}, Player purchased Removed Ads, won't show ads");
                }

                return Managers.Instance.Storage.RemovedAds;
            }
        }

        public bool IsBannerRemotelyEnabled
        {
            get
            {
                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsBannerEnabled.Value || AdsAlwaysAvailableInEditor;
            }
        }

        public bool IsInterstitialLoaded
        {
            get
            {
                if (!IsInit)
                    Debug.LogError($"{this.name} : {Utils.GetFuncName()}, AdsMediation is not Initialized");

                return (IsAdsMediationRemotelyEnabled &&
                        IsInit &&
                        IMediationNetwork.IsInterstitialAvailable()) ||
                        (Application.isEditor && AdsAlwaysAvailableInEditor) ||
                        GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice;
            }
        }

        public bool IsInterstitialRemotelyEnabled
        {
            get
            {
                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsInterstitialEnabled.Value || AdsAlwaysAvailableInEditor;
            }
        }

        public bool IsRewardVideoLoaded
        {
            get
            {
                if (!IsInit)
                    Debug.LogError($"{this.name} : {Utils.GetFuncName()}, AdsMediation is not Initialized");

                return (IsAdsMediationRemotelyEnabled &&
                        IsInit &&
                        IMediationNetwork.IsRewardVideoAvailable()) ||
                        (Application.isEditor && AdsAlwaysAvailableInEditor) ||
                        GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice;
            }
        }

        public bool IsRewardVideoRemotelyEnabled
        {
            get
            {
                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsRewardVideosEnabled.Value || AdsAlwaysAvailableInEditor;
            }
        }

        private bool m_WasInitMediationCalled = false;
        public void InitMediation()
        {
            if (IMediationNetwork != null)
            {
                if (IsAdsMediationRemotelyEnabled)
                {
                    if (!m_WasInitMediationCalled)
                    {
                        m_WasInitMediationCalled = true;

                        Debug.Log($"{this.name} : {Utils.GetFuncName()}() AdsMediation Initialization Started...");

                        if (Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitalSessionStartEnabled.Value)
                        {
                            IMediationNetwork.OnInterstitialAdReadyEvent += ShowInterstitialOnLaunch;
                        }

                        IMediationNetwork.Init(onAdsMediationInitialized);

                        //Invoke(nameof(LateStart), BannerDelayStart);
                    }
                    else
                    {
                        Debug.Log($"{this.name} : {Utils.GetFuncName()}() AdsMediation Already Initialized");
                    }
                }
            }
            else
            {
                Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Can't initialized, IMediationNetwork is null");
            }
        }

        private void onAdsMediationInitialized()
        {
            IsInit = true;

            OnAdsMediationInitialized.InvokeSafe();

            Debug.Log($"{this.name} : {Utils.GetFuncName()}() AdsMediation Initialized Successfully!");

            Invoke(nameof(DelayedStart), BannerDelayStart);
        }

        private void DelayedStart()
        {
            Debug.Log($"{this.name} : {Utils.GetFuncName()}()");

            if (IsBannerRemotelyEnabled && AutoShowBanner)
            {
                ShowBanner();
            }

            if (IsInterstitialRemotelyEnabled)
            {
                // ironSource/MAX: need to load an interstitial
                LoadInterstitial();
            }
        }

        protected bool m_IsLevelCompleted = false;
        private bool m_IsRewardVideoShownOnThisLevel = false;
        private bool m_WasDebugKeyPressed = false;
        public void SetShortcutKeyPress()
        {
#if ENABLE_SHORTCUTS
            m_WasDebugKeyPressed = true;
#endif
        }

        public virtual void LevelLoaded()
        {
            // Making sure this is not the first time level was loaded (after game launch)
            // Meaning it was fired after a level win/lose and when player dismissed the screens
            if (StorageManager.Instance.IsFirstAttempt == false)
            {
                // In case player claimed a reward on win screen he would watch an RV
                // This ensures that an interstitial won't be called immediately,
                // right after watching an RV on the same level round
                if (m_IsRewardVideoShownOnThisLevel == false)
                {

#if UNITY_EDITOR
                    if (m_WasDebugKeyPressed)
                    {
#if ENABLE_SHORTCUTS
                        m_WasDebugKeyPressed = false;
#endif
                        return;
                    }
#endif

                    if (m_IsLevelCompleted)
                    {
                        // For Level Complete, since we are now showing an ad as part of the LevelLoaded flow
                        // We need to compenstate for the CurrentLevel counter bringing it -1 and then back with +1
                        // Since we want the previous levelid to be loggedd
                        StorageManager.Instance.CurrentLevel--;
                        TryShowInterstitial(m_IsLevelCompleted);
                        StorageManager.Instance.CurrentLevel++;
                    }
                    else
                    {
                        // For Level Fail, same id is used so we can call it normally
                        TryShowInterstitial(m_IsLevelCompleted);
                    }
                }
                else
                {
                    Debug.Log($"{nameof(AdsManager)} : {Utils.GetFuncName()}(): Won't call TryShowInterstitial after a RewardVideo");
                }
            }

            // New level was loaded (either by win or lose) let's switch this flag to false
            // Allowing to load interstitial according to cooldown rules
            m_IsRewardVideoShownOnThisLevel = false;
        }

        public virtual void LevelCompleted()
        {
            m_IsLevelCompleted = true;
        }

        public virtual void LevelFailed()
        {
            m_IsLevelCompleted = false;
        }

        public virtual void TryShowInterstitial(bool i_IsLevelCompleted)
        {
#if ENABLE_ADS
            if (Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsInterstitialTimeBased)
            {
                // Timed-Based
                this.InterstitialHandler.TryShowInterstitial(InterstitialHandler.InterstitialType.TimeBased);
            }
            else
            {
                if (i_IsLevelCompleted)
                {
                    this.InterstitialHandler.TryShowInterstitial(InterstitialHandler.InterstitialType.LevelCompleted);
                }
                else
                {
                    this.InterstitialHandler.TryShowInterstitial(InterstitialHandler.InterstitialType.LevelFailed);
                }
            }
#endif
        }

        protected virtual void ShowInterstitialOnLaunch()
        {
            //Debug.Log($"Trying to show interstitial on launch, Time From Startup: {Time.realtimeSinceStartup} seconds  Time Allowed: {RemoteSettingsManager.FirebaseRemoteConfig.InterstitalSessionStartEnabledSecondsAllowed.Value}");

            //if (Time.realtimeSinceStartup <= RemoteSettingsManager.FirebaseRemoteConfig.InterstitalSessionStartEnabledSecondsAllowed.Value)
            //{
            //    ShowInterstitial();
            //}

            if (Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitalSessionStartEnabled.Value)
            {
                ShowInterstitial();
            }

            IMediationNetwork.OnInterstitialAdReadyEvent -= ShowInterstitialOnLaunch;
        }

        public void ShowBanner(BannerShown i_ShownCallback = null,
                               string i_PlacementId = Constants.k_None,
                               BannerHidden i_HiddenCallback = null,
                               BannerFailed i_FailedCallback = null,
                               BannerDestroyed i_DestroyedCallback = null)
        {
            if (!IsInit)
            {
                Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Can't show, AdsMediation is not Initialized.");
                return;
            }

            if (IsAdsMediationRemotelyEnabled)
            {
                if (IsBannerRemotelyEnabled)
                {
                    if (!IsRemovedAds)
                    {
                        m_BannerEventsDummy.ResetCallbacks(
                            BannerEventsGlobal,
                            i_ShownCallback,
                            i_HiddenCallback,
                            i_FailedCallback,
                            i_DestroyedCallback,
                            i_PlacementId);

                        IMediationNetwork.ShowBanner(m_BannerEventsDummy);
                    }
                    else
                    {
                        Debug.Log($"{this.name} : {Utils.GetFuncName()}() Not showing Banner, player bought Removed Ads");
                    }
                }
                else
                {
                    Debug.Log($"{this.name} : {Utils.GetFuncName()}() Banner is disabled by RemoteSettings");
                }
            }
        }

        public void HideBanner()
        {
            if (IsInit)
            {
                IMediationNetwork.HideBanner();
            }
        }

        public void DestroyBanner()
        {
            if (IsInit)
            {
                IMediationNetwork.DestroyBanner();
            }
        }

        public void LoadInterstitial()
        {
            if (!IsInit)
            {
                Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Can't load, AdsMediation is not Initialized.");
                return;
            }

            if (IsAdsMediationRemotelyEnabled && !IsRemovedAds)
            {
                if (IsInterstitialRemotelyEnabled)
                {
                    IMediationNetwork.LoadInterstitial();
                }
                else
                {
                    Debug.Log($"{this.name} : {Utils.GetFuncName()}() Interstitial is disabled by RemoteSettings");
                }
            }
        }

        public void ShowInterstitial(InterstitialShown i_InterstitialShown = null,
                                      string i_PlacementId = Constants.k_None,
                                      InterstitialOpen i_InterstitialOpen = null,
                                      InterstitialClose i_InterstitialClose = null,
                                      InterstitialShownFail i_InterstitialShownFail = null)
        {
            if (!IsInit)
            {
                Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Can't show, AdsMediation is not Initialized.");
                return;
            }

            if (IsAdsMediationRemotelyEnabled)
            {
                if (IsInterstitialRemotelyEnabled)
                {
                    if (!IsRemovedAds)
                    {
                        m_InterstitialEventsDummy.ResetCallbacks(
                            InterstitialEventsGlobal,
                            i_InterstitialShown,
                            i_InterstitialOpen,
                            i_InterstitialClose,
                            i_InterstitialShownFail,
                            i_PlacementId);

                        if (GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice)
                        {
                            Debug.Log($"{this.name} : {Utils.GetFuncName()}() Showing Dummy Ad");

                            IMediationNetwork.ShowInterstitial(m_InterstitialEventsDummy, true);
                        }
                        else
                        {
                            if (IsInterstitialLoaded)
                            {
                                if (ShowInterstitialPreview && AdsInterstitialPreview != null)
                                {
                                    AdsInterstitialPreview.Show(() =>
                                    {
                                        IMediationNetwork.ShowInterstitial(m_InterstitialEventsDummy);
                                    });
                                }
                                else
                                {
                                    IMediationNetwork.ShowInterstitial(m_InterstitialEventsDummy);
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"{this.name} : {Utils.GetFuncName()}() Interstitial is not available, requesting a new one");

                                LoadInterstitial();
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{this.name} : {Utils.GetFuncName()}() Not showing interstitial, player bought Removed Ads");
                    }
                }
                else
                {
                    Debug.LogWarning($"{this.name} : {Utils.GetFuncName()}() Interstitial is disabled by RemoteSettings");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i_CloseWithSuccessCallback">Callback when user closes the video and will receive the reward</param>
        /// <param name="i_CloseWithFailCallback">Callback when user closes the video but will Not receive the reward</param>
        /// <param name="i_PlacementName">ID of the RV video</param>
        /// <param name="i_RewardVideoCallCallback">Returns the Ads status when we ask to open the Reward Video</param>
        public eRewardVideoCallResult ShowRewardVideo(Action i_CloseWithSuccessCallback = null,
                                    Action i_CloseWithFailCallback = null,
                                    string i_PlacementName = Constants.k_None)
        {
            return ShowRewardVideo(i_CloseCallback: (i_Placement, i_IsRVSuccess) =>
            {
                if (i_IsRVSuccess)
                {
                    i_CloseWithSuccessCallback.InvokeSafe();
                }
                else
                {
                    i_CloseWithFailCallback.InvokeSafe();
                }
            },
            i_PlacementName: i_PlacementName);
        }


        public eRewardVideoCallResult ShowRewardVideo(RewardVideoSuccess i_SuccessCallback = null,
                                      string i_PlacementName = Constants.k_None,
                                      RewardVideoOpen i_OpenCallback = null,
                                      RewardVideoClose i_CloseCallback = null,
                                      RewardVideoFail i_FailCallback = null)
        {
            if (!IsInit)
            {
                Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Can't show, AdsMediation is not Initialized.");
                return eRewardVideoCallResult.AdsMediationNotInitialized;
            }

            //Debug.LogError($"TimeFromLastRewardVideo: " + StorageManager.Instance.TimeFromLastRewardVideo);

            if (IsAdsMediationRemotelyEnabled)
            {
                if (IsRewardVideoRemotelyEnabled)
                {
                    //Not checking for removed ads since we want to show it even if player bought it
                    // { IsRemovedAds? }

                    m_RewardVideoCallbacksDummy.ResetCallbacks(
                             RewardVideoEventsGlobal,
                             i_SuccessCallback,
                             i_OpenCallback,
                             i_CloseCallback,
                             i_FailCallback,
                             i_PlacementName);

                    if (GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice)
                    {
                        IMediationNetwork.ShowRewardVideo(m_RewardVideoCallbacksDummy, true);

                        Debug.Log($"{this.name} : {Utils.GetFuncName()}() Showing Dummy Video Ad");
                        return eRewardVideoCallResult.Success;
                    }
                    else
                    {
                        if (IsRewardVideoLoaded)
                        {
                            IMediationNetwork.ShowRewardVideo(m_RewardVideoCallbacksDummy);

                            Debug.Log($"{this.name} : {Utils.GetFuncName()}() Showing a reward video... Placement: {i_PlacementName}");
                            return eRewardVideoCallResult.Success;
                        }
                        else
                        {
                            Debug.LogWarning($"{this.name} : {Utils.GetFuncName()}() RewardVideo is not available");
                            return eRewardVideoCallResult.RewardVideoIsNotAvailable;
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"{this.name} : {Utils.GetFuncName()}() RewardVideo is disabled by RemoteSettings");
                    return eRewardVideoCallResult.RewardVideoIsDisabledByRemoteSettings;
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"{this.name} : {Utils.GetFuncName()}() Ads Mediation is disabled by RemoteSettings");
                return eRewardVideoCallResult.AdsMediationIsDisabledByRemoteSettings;
            }
        }

        #region Ads Callbacks
        private void onRewardVideoClosed(string i_PlacementName, bool i_IsRVSuccess)
        {
            if (i_IsRVSuccess)
            {
                Debug.Log($"{this.name} : {Utils.GetFuncName()}() Global Callback - Placement: {i_PlacementName}");

                Managers.Instance.Storage.TotalVideosWatched++;
                //Managers.Instance.Storage.TotalVideosWatchedInDay++;
                //Managers.Instance.Storage.LastRewardVideoTimeStamp = DateTime.Now;
                Managers.Instance.Storage.InterstitialCoolDownCounter = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.RewardVideoCoolDown.Value;

                m_IsRewardVideoShownOnThisLevel = true;
            }
        }

        private void onInterstitialShown(string i_PlacementName)
        {
            Debug.Log($"{this.name} : {Utils.GetFuncName()}() Global Callback - Placement: {i_PlacementName}");

            Managers.Instance.Storage.TotalInterstitialsWatched++;
            //Managers.Instance.Storage.TotalInterstitialViewedInDay++;
            //Managers.Instance.Storage.LastInterstitialTimeStamp = DateTime.Now;
            Managers.Instance.Storage.InterstitialCoolDownCounter = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialCoolDown.Value;
        }

        private void onBannerShown(string i_PlacementName)
        {
            IsBannerVisible = true;

            Debug.Log($"{this.name} : {Utils.GetFuncName()}() Global Callback - Placement: {i_PlacementName}");
            OnBannerShown.InvokeSafe();
        }

        private void onBannerHidden(string i_PlacementName)
        {
            IsBannerVisible = false;

            Debug.Log($"{this.name} : {Utils.GetFuncName()}() Global Callback - Placement: {i_PlacementName}");
            OnBannerHidden.InvokeSafe();
        }
        #endregion

        void OnApplicationPause(bool i_Pause)
        {
            if (!Application.isEditor)
            {
                IMediationNetwork.OnApplicationPause(i_Pause);
            }
        }

        public void RemoveAds()
        {
            HideBanner();
        }

        public void ShowMediationDebugPanel()
        {
            IMediationNetwork.ShowMediationDebugger();
        }

        #region Prefab Handeling
#if UNITY_EDITOR
        [BoxGroup("Prefabs")]
        [HorizontalGroup("Prefabs/1")]
        [Button, HorizontalGroup("Prefabs/1/Right")]
        public void AddMaxMediationPrefab()
        {
#if ENABLE_MAX
            if (this.GetComponentInChildren<MAXMediation>() == null)
            {
                this.gameObject.AddPrefab("Assets/_KobGamesSDK_Slim_Ads/Prefabs/Ads/Mediation_MAX/MAXMediation.prefab");
            }
#endif
        }

        [Button, HorizontalGroup("Prefabs/1/Left")]
        public void RemoveMaxMediationPrefab()
        {
#if ENABLE_ADS
            if (this.GetComponentInChildren<MAXMediation>() != null)
            {
                this.gameObject.RemovePrefab(this.GetComponentInChildren<MAXMediation>().gameObject);
            }
#endif
        }

        [HorizontalGroup("Prefabs/2")]
        [Button, HorizontalGroup("Prefabs/2/Right")]
        public void AddIronSourceMediationPrefab()
        {
#if UNITY_EDITOR && ENABLE_IRONSOURCE
            if (this.GetComponentInChildren<IronSourceMediation>() == null)
            {
                this.gameObject.AddPrefab("Assets/_KobGamesSDK_Slim_Ads/Prefabs/Ads/Mediation_IronSource/IronSourceMediation.prefab");
            }
#endif
        }

        [Button, HorizontalGroup("Prefabs/2/Left")]
        public void RemoveIronSourceMediationPrefab()
        {
#if ENABLE_ADS
            if (this.GetComponentInChildren<IronSourceMediation>() != null)
            {
                this.gameObject.RemovePrefab(this.GetComponentInChildren<IronSourceMediation>().gameObject);
            }
#endif
        }

        [HorizontalGroup("Prefabs/3")]
        [Button, HorizontalGroup("Prefabs/3/Right")]
        public void AddBepicMediationPrefab()
        {
#if ENABLE_BEPIC

            if (this.GetComponentInChildren<BepicMediation>() == null)
            {
                this.gameObject.AddPrefab("Assets/_KobGamesSDK_Slim_Ads/Prefabs/Ads/Mediation_Bepic/BepicMediation.prefab");
            }
#endif
        }

        [Button, HorizontalGroup("Prefabs/3/Left")]
        public void RemoveBepicMediationPrefab()
        {
#if ENABLE_ADS
            if (this.GetComponentInChildren<BepicMediation>() != null)
            {
                this.gameObject.RemovePrefab(this.GetComponentInChildren<BepicMediation>().gameObject);
            }
#endif
        }


        [Button, HorizontalGroup("Prefabs/4/Left")]
        public void RemoveTestSuiteCanvasPrefab()
        {
            Transform testSuiteCanvas = this.transform.Find("TestSuiteCanvas");

            if (testSuiteCanvas != null)
            {
                this.gameObject.RemovePrefab(testSuiteCanvas.gameObject);
            }
        }

        [HorizontalGroup("Prefabs/5")]
        [Button, HorizontalGroup("Prefabs/5/Left")]
        public void AddInterstitialPreviewPrefab()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (AdsManager.Instance.transform.Find("AdsInterstitialPreview") == null)
                {
                    AdsManager.Instance.gameObject.AddPrefab("Assets/_KobGamesSDK_Slim/Prefabs/Ads/AdsInterstitialPreview.prefab");
                }
            };
#endif
        }

        [Button, HorizontalGroup("Prefabs/5/Right")]
        public void RemoveInterstitialPreviewPrefab()
        {
            Transform adsInterstitialPreview = this.transform.Find("AdsInterstitialPreview");

            if (adsInterstitialPreview != null)
            {
                this.gameObject.RemovePrefab(adsInterstitialPreview.gameObject);
            }
        }

#if ENABLE_BEPIC
        [HorizontalGroup("Prefabs/6")]
        [Button, HorizontalGroup("Prefabs/6/Left")]
        public void AddBepicAdMockupPrefab()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && this.GetComponentInChildren<EditorAdMock>() == null && this.transform.Find("EditorAdMockUnderCanvas") == null)
                {
                    string assetRootPath = Managers.Instance.gameObject.GetPrefabAssetPathOfNearestInstanceRoot();
                    Managers.Instance.gameObject.UnpackPrefabInstanceNearest();

                    this.AdsInterstitialPreview.gameObject.AddPrefab("Assets/SimpleSDK/Prefab/EditorAdMockUnderCanvas.prefab");

                    Managers.Instance.gameObject.ApplyToPrefab(assetRootPath);
                }
            };
#endif
        }

        [Button, HorizontalGroup("Prefabs/6/Right")]
        public void RemoveBepicAdMockupPrefab()
        {
#if UNITY_EDITOR
            EditorAdMock editorAdMock = this.GetComponentInChildren<EditorAdMock>();

            if (editorAdMock != null && this.AdsInterstitialPreview != null)
            {
                string assetRootPath = Managers.Instance.gameObject.GetPrefabAssetPathOfNearestInstanceRoot();
                Managers.Instance.gameObject.UnpackPrefabInstanceNearest();

                this.AdsInterstitialPreview.gameObject.RemovePrefab(editorAdMock.gameObject);

                Managers.Instance.gameObject.ApplyToPrefab(assetRootPath);
            }
#endif
        }
#endif
#endif
        #endregion
    }
#else
    // Dummy Implementation
    [Serializable]
    public class AdsManagerBase : Singleton<AdsManager>
    {
        // Delegates
        private InterstitialEvents m_InterstitialEventsDummy = new InterstitialEvents();
        private RewardVideoEvents m_RewardVideoCallbacksDummy = new RewardVideoEvents();

        public static InterstitialEvents InterstitialEventsGlobal        = new InterstitialEvents();
        public static RewardVideoEvents  RewardVideoEventsGlobal         = new RewardVideoEvents();
        public static Action<bool>       OnRewardVideoAvailabilityChange = (i_State) => { };

        public static BannerEvents BannerEventsGlobal;
        
        public bool IsInterstitialLoaded { get { return true; } }
        public bool IsRewardVideoLoaded { get { return true; } }

        [Title("Mediation Networks"), ShowInInspector, SerializeField]
        public DummyMediation IMediationNetwork = new DummyMediation();

        public void ShowInterstitial() { }

        [NonSerialized, ReadOnly, ShowInInspector]
        public string[] GameObjectsToRemove = { "MAXMediation", "IronSourceMediation", "BepicMediation", "TestSuiteCanvas" };

        [Button]
        public void RemoveAdsGameObjectsLeftOvers()
        {
#if UNITY_EDITOR
            foreach (var name in GameObjectsToRemove)
            {
                Transform transformToRemove = this.transform.GetComponentsInChildren<Transform>()
                                .Where(trans => trans.name.Contains(name)).FirstOrDefault();

                if (transformToRemove != null)
                {
                    string gameobjectName = transformToRemove.name;
                    this.gameObject.RemovePrefab(transformToRemove.gameObject);

                    Debug.LogError($"Removed {gameobjectName} GameObject from {this.name}");
                }
            }

            GameSettings.Instance.AdsMediation.MediationNetworks = eMediationNetworks.None;
#endif
        }

        
        #region RV
        public eRewardVideoCallResult ShowRewardVideo(Action i_CloseWithSuccessCallback = null,
                                    Action                   i_CloseWithFailCallback    = null,
                                    string                   i_PlacementName            = Constants.k_None)
        {
            //If using dummy ads it will show a dummy screen so we use that pipeline
            if (GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice)
            {
                return ShowRewardVideo(i_CloseCallback: (i_Placement, i_IsRVSuccess) =>
                    {
                        if (i_IsRVSuccess)
                        {
                            i_CloseWithSuccessCallback.InvokeSafe();
                        }
                        else
                        {
                            i_CloseWithFailCallback.InvokeSafe();
                        }
                    },
                    i_PlacementName: i_PlacementName);
            }
            
            //If we don't use dummy ads screen it will automatically default to Success
            //Delay exists so it can mimic real Ads where we receive eRewardVideoCallResult first and only then then callback
            DOVirtual.DelayedCall(.2f, i_CloseWithSuccessCallback.InvokeSafe);
            return eRewardVideoCallResult.Success;
        }

        public eRewardVideoCallResult ShowRewardVideo(RewardVideoSuccess i_SuccessCallback = null,
                                    string                               i_PlacementName   = Constants.k_None,
                                    RewardVideoOpen                      i_OpenCallback    = null,
                                    RewardVideoClose                     i_CloseCallback   = null,
                                    RewardVideoFail                      i_FailCallback    = null)
        {

            m_RewardVideoCallbacksDummy.ResetCallbacks(
                     RewardVideoEventsGlobal,
                     i_SuccessCallback,
                     i_OpenCallback,
                     i_CloseCallback,
                     i_FailCallback,
                     i_PlacementName);

            if (GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice)
            {
                Debug.Log($"{this.name} : {Utils.GetFuncName()}() Showing Dummy Video Ad");

                IMediationNetwork.ShowRewardVideo(m_RewardVideoCallbacksDummy, true);
            }
            else
            {
                if (IsRewardVideoLoaded)
                {
                    Debug.Log($"{this.name} : {Utils.GetFuncName()}() Showing a reward video... Placement: {i_PlacementName}");

                    IMediationNetwork.ShowRewardVideo(m_RewardVideoCallbacksDummy);
                }
                else
                {
                    Debug.LogWarning($"{this.name} : {Utils.GetFuncName()}() RewardVideo is not available");
                }
            }

            return eRewardVideoCallResult.Success;
        }
        #endregion
        
        #region Banner
        public void ShowBanner(){}
        public void HideBanner(){}
        #endregion
        
        public void SetShortcutKeyPress(){}
    }
#endif
}

public enum eMediationNetworks
{
    None = 0,
    ironSource = 1,
    MAX = 2,
    SUPERERA = 3
}

public enum ePPNetworks
{
    None = 0,
    BEPIC = 1,
    KOBIC = 2
}
