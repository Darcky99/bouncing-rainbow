using System;
using KobGamesSDKSlim;

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

    IBanner Banner { get; }
    IInterstitial Interstitial { get; }
    IRewardVideo RewardVideo { get; }
}

public interface IBanner
{
    void Initialize();
    void ShowBanner(BannerEvents i_BannerEvents);
    void HideBanner();
    void DestroyBanner();
    void SimulateFailedBannerLoad();
    void OnApplicationPause(bool i_Pause);
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