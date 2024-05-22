using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class MAXInterstitial : MonoBehaviour, IInterstitial
    {
        public static string ClassName => nameof(MAXInterstitial);

        private InterstitialEvents m_InterstitialEventsDummy;

        private int m_RetryAttempt = 0;

        public string AndroidPlacementId = "interstitial_android";
        public string IOSPlacementId = "interstitial_ios";
        public string PlatformPlacementId
        {
            get
            {
#if UNITY_ANDROID
                return AndroidPlacementId;
#elif UNITY_IOS
                return IOSPlacementId;
#else
                return Constants.k_None;
#endif
            }
        }

        private string m_InterstitialAdUnitId
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return GameSettings.Instance.AdsMediation.MAXInterstitialAdUnitIdAndroid;
                }
                else
                {
                    return GameSettings.Instance.AdsMediation.MAXInterstitialAdUnitIdIOS;
                }
            }
        }

        public void Initialize()
        {
#if ENABLE_MAX
            // Attach callback
            MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += OnInterstitialAdShowFailedEvent;
            MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialAdShowSucceededEvent;
            MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialAdClosedEvent;
#endif
        }

        public void LoadInterstitial()
        {
#if ENABLE_MAX
            MaxSdk.LoadInterstitial(m_InterstitialAdUnitId);
#endif
        }

        public bool IsInterstitialAvailable()
        {
#if ENABLE_MAX
            return MaxSdk.IsInterstitialReady(m_InterstitialAdUnitId);
#else
            return false;
#endif
        }

        public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd = false)
        {
#if ENABLE_MAX
            m_InterstitialEventsDummy = i_InterstitialEvents;

            if (i_IsDummyAd)
            {
                OnInterstitialAdOpenedEvent(m_InterstitialAdUnitId);
            }
            else
            {
                if (Application.isEditor || MaxSdk.IsInterstitialReady(m_InterstitialAdUnitId))
                {
                    MaxSdk.ShowInterstitial(m_InterstitialAdUnitId, m_InterstitialEventsDummy.PlacementId == Constants.k_None ? PlatformPlacementId : m_InterstitialEventsDummy.PlacementId);
                    OnInterstitialAdOpenedEvent(m_InterstitialAdUnitId);
                }
                else
                {
                    Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Interstitial is not available, requesting a new one");

                    LoadInterstitial();
                }
            }
#endif
        }

        private void OnInterstitialLoadedEvent(string adUnitId)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            Debug.Log($"{ClassName}-{Utils.GetFuncName()},  Time From Startup: {Time.realtimeSinceStartup} seconds");

            // Reset retry attempt
            m_RetryAttempt = 0;
        }

        private double m_RetryDelay = 0;
        private void OnInterstitialLoadFailedEvent(string adUnitId, int errorCode)
        {
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 16 seconds)
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Interstitial failed to load, Code: {errorCode}  adUnitId: {adUnitId}");

            m_RetryAttempt++;
            m_RetryDelay = Math.Pow(2, Math.Min(4, m_RetryAttempt)); // delay up to 2^4 = 16 seconds

            Invoke(nameof(LoadInterstitial), (float)m_RetryDelay);
        }

        private void OnInterstitialAdShowFailedEvent(string adUnitId, int errorCode)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Interstitial failed to show, Code: {errorCode}  adUnitId: {adUnitId}");

            m_InterstitialEventsDummy?.OnShownFailed(new IAdNetworkError(errorCode, adUnitId));

            // Interstitial ad failed to display. We recommend loading the next ad
            LoadInterstitial();
        }

        private void OnInterstitialAdOpenedEvent(string adUnitId)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}");
            m_InterstitialEventsDummy?.OnOpened();
        }

        private void OnInterstitialAdShowSucceededEvent(string adUnitId)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_InterstitialEventsDummy?.OnShownSuccess();
                Utils.MuteAllAudioSources();
            });
        }

        private void OnInterstitialAdClosedEvent(string adUnitId)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Time to load a new Interstitial");

            LoadInterstitial();

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_InterstitialEventsDummy?.OnClosed();

                Utils.UnMuteAllAudioSources();
            });
        }

        public void SimulateShownSuccessAndClose()
        {
            if (m_InterstitialEventsDummy == null)
            {
                m_InterstitialEventsDummy = new InterstitialEvents();

                // This is used to inject global events that other scripts listens to
                m_InterstitialEventsDummy.InjectCallBacks(AdsManager.InterstitialEventsGlobal);
            }

            OnInterstitialAdShowSucceededEvent(m_InterstitialAdUnitId);
            OnInterstitialAdClosedEvent(m_InterstitialAdUnitId);
        }

        public void SimulateFailedInterstitialLoad()
        {
            if (m_InterstitialEventsDummy == null)
            {
                m_InterstitialEventsDummy = new InterstitialEvents();

                // This is used to inject global events that other scripts listens to
                m_InterstitialEventsDummy.InjectCallBacks(AdsManager.InterstitialEventsGlobal);
            }

            OnInterstitialLoadFailedEvent(m_InterstitialAdUnitId, 100); //"Simulated Editor Error, this is just a test"
        }
    }
}