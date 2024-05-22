using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [RequireComponent(typeof(IronSourceBanner), typeof(IronSourceInterstitial), typeof(IronSourceRewardVideo))]
    public class IronSourceMediation : MonoBehaviour, IMediationNetwork
    {
        public bool IsInit { get; set; }

        public IronSourceBanner IronSourceBanner;
        public IronSourceInterstitial IronSourceInterstitial;
        public IronSourceRewardVideo IronSourceRewardVideo;

        public IBanner Banner => IronSourceBanner;
        public IInterstitial Interstitial => IronSourceInterstitial;
        public IRewardVideo RewardVideo => IronSourceRewardVideo;

        public event Action OnInterstitialAdReadyEvent = () => { };
        public event Action OnInitializedCallback = () => { };

        public void OnValidate()
        {
            SetRefs();
        }

        public void SetRefs()
        {
            if (IronSourceBanner == null) IronSourceBanner = this.GetComponentInChildren<IronSourceBanner>(true);
            if (IronSourceInterstitial == null) IronSourceInterstitial = this.GetComponentInChildren<IronSourceInterstitial>(true);
            if (IronSourceRewardVideo == null) IronSourceRewardVideo = this.GetComponentInChildren<IronSourceRewardVideo>(true);
        }

        public void Init(Action i_InitializedCallback)
        {
#if ENABLE_IRONSOURCE
            OnInitializedCallback += i_InitializedCallback;

            Debug.Log($"Init ironSource with Key: {GameSettings.Instance.AdsMediation.IronSourceAppId}");
            Debug.Log($"AdMob-iOS: {GameSettings.Instance.AdsMediation.AdMobAppIdIos}");
            Debug.Log($"AdMob-Android: {GameSettings.Instance.AdsMediation.AdMobAppIdAndroid}");

            if (!Application.isEditor)
            {
                Debug.Log("IronSource: IronSource.Agent.validateIntegration");
                IronSource.Agent.validateIntegration();

                Debug.Log("IronSource: AdsMediation Initialization...");
                IronSource.Agent.setConsent(true);
                IronSourceConfig.Instance.setClientSideCallbacks(true);
                IronSource.Agent.setAdaptersDebug(false);
                IronSource.Agent.shouldTrackNetworkState(true);
                IronSource.Agent.getAdvertiserId();
                //IronSource.Agent.setUserId(SystemInfo.deviceUniqueIdentifier);

                IronSource.Agent.init(GameSettings.Instance.AdsMediation.IronSourceAppId);
            }

            Banner.Initialize();
            Interstitial.Initialize();
            RewardVideo.Initialize();

            IsInit = true;

            OnInitializedCallback.InvokeSafe();
#endif
        }

        public void ShowMediationDebugger()
        {
#if ENABLE_IRONSOURCE
            // Show Mediation Debugger
            IronSource.Agent.validateIntegration();
#endif
        }

        public bool IsInterstitialAvailable()
        {
            // Make sure to always return true in editor
            return Application.isEditor || Interstitial.IsInterstitialAvailable();
        }

        public bool IsRewardVideoAvailable()
        {
            // Make sure to always return true in editor
            return Application.isEditor || RewardVideo.IsRewardVideoAvailable();
        }

        public void ShowBanner(BannerEvents i_BannerEvents)
        {
            Banner.ShowBanner(i_BannerEvents);
        }

        public void HideBanner()
        {
            Banner.HideBanner();
        }

        public void DestroyBanner()
        {
            Banner.DestroyBanner();
        }

        public void LoadInterstitial()
        {
            Interstitial.LoadInterstitial();
        }

        public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd = false)
        {
            Interstitial.ShowInterstitial(i_InterstitialEvents, i_IsDummyAd);
        }

        public void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd = false)
        {
            RewardVideo.ShowRewardVideo(i_RewardVideoEvents, i_IsDummyAd);
        }

        public void OnApplicationPause(bool i_Pause)
        {
#if ENABLE_IRONSOURCE
            IronSource.Agent.onApplicationPause(i_Pause);
#endif
        }
    }
}