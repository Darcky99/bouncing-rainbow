#if !ENABLE_ADS
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class DummyMediation : IMediationNetwork
    {
        public bool IsInit { get; set; }

        [SerializeField, HideInInspector]
        private IInterstitial m_Interstitial = new DummyInterstitial();
        public IInterstitial Interstitial => m_Interstitial;

        [SerializeField, HideInInspector]
        private IRewardVideo m_RewardVideo = new DummyRewardVideo();
        public IRewardVideo RewardVideo => m_RewardVideo;

        public event Action OnInterstitialAdReadyEvent;
        public event Action OnInitializedCallback;

        public void DestroyBanner() { }

        public void HideBanner() { }

        public void Init(Action i_InitializedCallback) { }

        public bool IsInterstitialAvailable() { return true; }

        public bool IsRewardVideoAvailable() { return true; }

        public void LoadInterstitial() { }

        public void OnApplicationPause(bool i_Pause) { }

        public void OnValidate() { }

        public void SetRefs() { }

        public void ShowBanner(BannerEvents i_BannerEvents) { }

        public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd = false)
        {
            Interstitial.ShowInterstitial(i_InterstitialEvents, i_IsDummyAd);
        }

        public void ShowMediationDebugger() { }

        public void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd = false)
        {
            RewardVideo.ShowRewardVideo(i_RewardVideoEvents, i_IsDummyAd);
        }
    }

    [Serializable]
    public class DummyInterstitial : IInterstitial
    {
        public static string ClassName => nameof(DummyInterstitial);

        private InterstitialEvents m_InterstitialEventsDummy;

        public void Initialize() { }

        public bool IsInterstitialAvailable() { return true; }

        public void LoadInterstitial() { }

        public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd)
        {
            m_InterstitialEventsDummy = i_InterstitialEvents;

            m_InterstitialEventsDummy?.OnOpened();
        }

        public void SimulateFailedInterstitialLoad()
        {
            m_InterstitialEventsDummy?.OnShownFailed(new IAdNetworkError(-1, "None"));
        }

        public void SimulateShownSuccessAndClose()
        {
            m_InterstitialEventsDummy?.OnShownSuccess();
            m_InterstitialEventsDummy?.OnClosed();
        }
    }

    [Serializable]
    public class DummyRewardVideo : IRewardVideo
    {
        public static string ClassName => nameof(DummyRewardVideo);

        private RewardVideoEvents m_RewardVideoEventsDummy;

        public void Initialize() { }

        public bool IsRewardVideoAvailable() { return true; }

        public void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd = false)
        {
            m_RewardVideoEventsDummy = i_RewardVideoEvents;

            m_RewardVideoEventsDummy?.OnOpened();
        }

        public void SimulateFailedRewardVideoLoad()
        {
            m_RewardVideoEventsDummy?.OnFailed(new IAdNetworkError(-1, "None"));
            m_RewardVideoEventsDummy?.OnClosed(false);
        }

        public void SimulateShownSuccessAndClose()
        {
            m_RewardVideoEventsDummy?.OnSuccess();
            m_RewardVideoEventsDummy?.OnClosed(true);
        }
    }

    public interface IMediationNetwork
    {
        bool IsInit { get; set; }

        void Init(Action i_InitializedCallback);
        void OnValidate();
        void SetRefs();
        bool IsInterstitialAvailable();
        bool IsRewardVideoAvailable();
        void ShowBanner(BannerEvents i_BannerEvents);
        void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd = false);
        void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd = false);
        void HideBanner();
        void DestroyBanner();
        void LoadInterstitial();
        void ShowMediationDebugger();
        void OnApplicationPause(bool i_Pause);

        event Action OnInterstitialAdReadyEvent;
        event Action OnInitializedCallback;

        IInterstitial Interstitial { get; }
        IRewardVideo RewardVideo { get; }
    }

    public interface IInterstitial
    {
        void Initialize();
        bool IsInterstitialAvailable();
        void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd);
        void LoadInterstitial();
        void SimulateShownSuccessAndClose();
        void SimulateFailedInterstitialLoad();
    }

    public interface IRewardVideo
    {
        void Initialize();
        bool IsRewardVideoAvailable();
        void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd = false);
        void SimulateShownSuccessAndClose();
        void SimulateFailedRewardVideoLoad();
    }
}
#endif