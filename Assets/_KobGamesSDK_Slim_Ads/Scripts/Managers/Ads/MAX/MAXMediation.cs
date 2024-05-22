using System;
using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using System.Runtime.InteropServices;

namespace KobGamesSDKSlim
{
    ////////////////////////////////////////////////////////////////////////////////////////
    // Error Codes:
    // #define kALErrorCodeNoFill 204
    // Indicates that no ads are currently eligible for your device & location.
    // https://monetization-support.applovin.com/hc/en-us/articles/115000400607-What-are-the-iOS-error-codes-and-what-do-they-mean-
    // https://github.com/AppLovin/AppLovin-MAX-SDK-iOS/blob/master/README.md
    // -1	            Indicates an unspecified error with one of the mediated network SDKs.
    // 204	            Indicates that no ads are currently eligible for your device.
    // -102	| -1001     Indicates that the ad request timed out (usually due to poor connectivity).
    // -103 | -1009	    Indicates that the device is not connected to the internet(e.g.airplane mode).
    // -5001	        Indicates that the ad failed to load due to various reasons(such as no networks being able to fill).
    // -5201	        Indicates an internal state error with the AppLovin MAX SDK.
    // -5601	        Indicates the provided Activity instance has been garbage collected while the AppLovin MAX SDK attempts to re-load an expired ad.
    ////////////////////////////////////////////////////////////////////////////////////////

    [RequireComponent(typeof(MAXBanner), typeof(MAXInterstitial), typeof(MAXRewardVideo))]
    public class MAXMediation : MonoBehaviour, IMediationNetwork
    {
        public bool IsInit { get; set; }

        public MAXBanner MAXBanner;
        public MAXInterstitial MAXInterstitial;
        public MAXRewardVideo MAXRewardVideo;

        public IBanner Banner => MAXBanner;
        public IInterstitial Interstitial => MAXInterstitial;
        public IRewardVideo RewardVideo => MAXRewardVideo;

        public event Action OnInterstitialAdReadyEvent = () => { };
        public event Action OnInitializedCallback = () => { };

        public void OnValidate()
        {
            SetRefs();
        }

        public void SetRefs()
        {
            if (MAXBanner == null) MAXBanner = this.GetComponentInChildren<MAXBanner>(true);
            if (MAXInterstitial == null) MAXInterstitial = this.GetComponentInChildren<MAXInterstitial>(true);
            if (MAXRewardVideo == null) MAXRewardVideo = this.GetComponentInChildren<MAXRewardVideo>(true);
        }

        public void Init(Action i_InitializedCallback)
        {
#if ENABLE_MAX
            OnInitializedCallback += i_InitializedCallback;

            MaxSdkCallbacks.OnSdkInitializedEvent += onSdkInitializedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (adUnitId, adInfo) => OnInterstitialAdReadyEvent.Invoke();

            Debug.Log($"Init AppLovin with SDK Key: {GameSettings.Instance.AdsMediation.MAXSDKKey}");
            Debug.Log($"AdMob-iOS: {GameSettings.Instance.AdsMediation.AdMobAppIdIos}");
            Debug.Log($"AdMob-Android: {GameSettings.Instance.AdsMediation.AdMobAppIdAndroid}");

            MaxSdk.SetSdkKey(GameSettings.Instance.AdsMediation.MAXSDKKey);
            //MaxSdk.SetTestDeviceAdvertisingIdentifiers(new string[] { "9B6EF53A-41F5-4F9F-9719-21EE4FEBEE4A" });

#if UNITY_IOS && !UNITY_EDITOR
            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
#endif
            MaxSdk.InitializeSdk();
#endif
        }

#if ENABLE_MAX
        private int m_DelayInit = 5;
        private void onSdkInitializedEvent(MaxSdkBase.SdkConfiguration i_SdkConfiguration)
        {
            // AppLovin SDK is initialized, start loading ads
            Debug.Log($"AppLovin onSdkInitializedEvent, Init: {MaxSdk.IsInitialized()}");

            MaxSdk.SetHasUserConsent(true);

            if (GameConfig.Instance.Menus.GDPR.IsGDPRRequired && !StorageManager.Instance.IsGDPRConsentGiven &&
                (i_SdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies ||
                 i_SdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Unknown ||
                 Application.isEditor))
            {
                bool isIOS145 = false;

#if UNITY_IOS || UNITY_IPHONE
                if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser)
                {
                    // On iOS 14.5+ we use MAX consent flow which will show MAX GDPR screen too, so disable in-game GDPR flow.
                    isIOS145 = true;
                }
#endif

                // If iOS 14.5+ no need to show in-game GDPR, MAX is taking care of it
                if (!isIOS145)
                {
                    // Show user consent dialog
                    // Always MaxSdk.SetHasUserConsent(true), since we won't allow to close popup without approval)
                    // TODO - in the future this should call MenuManager func instead of direct call here?
                    MenuManager.Instance.OpenMenuScreen(nameof(Screen_GDPR));
                }
            }

            //Banner.Initialize();
            //Interstitial.Initialize();
            //RewardVideo.Initialize();

            // Let's add some delays before initializing interstitials and RVs to avoid overheads on low-end devices
            Invoke(nameof(DelayedBannerInit), m_DelayInit);
            Invoke(nameof(DelayedInterstitialInit), m_DelayInit + 5);
            Invoke(nameof(DelayedRewardVideoInit), m_DelayInit + 9);

            IsInit = true;

            OnInitializedCallback.InvokeSafe();
        }
#endif

        public void DelayedBannerInit()
        {
            Banner.Initialize();
        }

        public void DelayedInterstitialInit()
        {
            Interstitial.Initialize();
        }

        public void DelayedRewardVideoInit()
        {
            RewardVideo.Initialize();
        }

        public void ShowMediationDebugger()
        {
#if ENABLE_MAX
            // Show Mediation Debugger
            MaxSdk.ShowMediationDebugger();
#endif
        }

        public bool IsInterstitialAvailable()
        {
            // On MAX we mustn't return always true on editor, as it may cause errors, MAX should handle editor case on its own
            return Interstitial.IsInterstitialAvailable();
        }

        public bool IsRewardVideoAvailable()
        {
            // On MAX we mustn't return always true on editor, as it may cause errors, MAX should handle editor case on its own
            return RewardVideo.IsRewardVideoAvailable();
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
        }
    }
}

#if UNITY_IOS
namespace AudienceNetwork
{
    public static class AdSettings
    {
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
        }
    }
}
#endif