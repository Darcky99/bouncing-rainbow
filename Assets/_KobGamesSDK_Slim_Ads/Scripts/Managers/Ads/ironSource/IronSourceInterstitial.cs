using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class IronSourceInterstitial : MonoBehaviour, IInterstitial
    {
        public static string ClassName => nameof(IronSourceInterstitial);

        private InterstitialEvents m_InterstitialEventsDummy;

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

        public void Initialize()
        {
#if ENABLE_IRONSOURCE
            IronSourceEvents.onInterstitialAdReadyEvent += OnInterstitialLoadedEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += OnInterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += OnInterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent += OnInterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent += OnInterstitialAdClosedEvent;
#endif
        }

        public void LoadInterstitial()
        {
#if ENABLE_IRONSOURCE
            IronSource.Agent.loadInterstitial();
#endif
        }

        public bool IsInterstitialAvailable()
        {
#if ENABLE_IRONSOURCE
            return IronSource.Agent.isInterstitialReady();
#else
            return false;
#endif
        }

        public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd = false)
        {
#if ENABLE_IRONSOURCE
            m_InterstitialEventsDummy = i_InterstitialEvents;

            if (i_IsDummyAd)
            {
                OnInterstitialAdOpenedEvent();
            }
            else
            {
                if (Application.isEditor || IsInterstitialAvailable())
                {
                    IronSource.Agent.showInterstitial(m_InterstitialEventsDummy.PlacementId == Constants.k_None ? PlatformPlacementId : m_InterstitialEventsDummy.PlacementId);

                    if (Application.isEditor)
                    {
                        OnInterstitialAdOpenedEvent();
                    }
                }
                else
                {
                    Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Interstitial is not available, requesting a new one");

                    LoadInterstitial();
                }
            }
#endif
        }

        private void OnInterstitialLoadedEvent()
        {
            // Interstitial ad is ready to be shown.
            Debug.Log($"{ClassName}-{Utils.GetFuncName()},  Time From Startup: {Time.realtimeSinceStartup} seconds");
        }

        private void OnInterstitialLoadFailedEvent(IronSourceError i_IronSourceError)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Interstitial failed to load, Code: {i_IronSourceError.getCode()}  Description: {i_IronSourceError.getDescription()}");
        }

        private void OnInterstitialAdShowFailedEvent(IronSourceError i_IronSourceError)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Interstitial failed to show, Code: {i_IronSourceError.getCode()}  Description: {i_IronSourceError.getDescription()}");

            m_InterstitialEventsDummy?.OnShownFailed(new IAdNetworkError(i_IronSourceError.getErrorCode().ToString(), i_IronSourceError.getDescription()));

            // Interstitial ad failed to display. We recommend loading the next ad
            LoadInterstitial();
        }

        private void OnInterstitialAdOpenedEvent()
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}");           
            m_InterstitialEventsDummy?.OnOpened();
        }

        private void OnInterstitialAdShowSucceededEvent()
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_InterstitialEventsDummy?.OnShownSuccess();
                Utils.MuteAllAudioSources();
            });
        }

        private void OnInterstitialAdClosedEvent()
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

            OnInterstitialAdShowSucceededEvent();
            OnInterstitialAdClosedEvent();
        }

        public void SimulateFailedInterstitialLoad()
        {
            if (m_InterstitialEventsDummy == null)
            {
                m_InterstitialEventsDummy = new InterstitialEvents();

                // This is used to inject global events that other scripts listens to
                m_InterstitialEventsDummy.InjectCallBacks(AdsManager.InterstitialEventsGlobal);
            }

            OnInterstitialLoadFailedEvent(new IronSourceError(100, "Simulated Interstitial Error")); //"Simulated Editor Error, this is just a test"
        }
    }
}