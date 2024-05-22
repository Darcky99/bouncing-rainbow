using System;
using KobGamesSDKSlim;
using UnityEngine;

[RequireComponent(typeof(BepicBanner), typeof(BepicInterstitial), typeof(BepicRewardVideo))]
public class BepicMediation : MonoBehaviour, IMediationNetwork
{
    public bool IsInit { get; set; }

    public BepicBanner SupereraBanner;
    public BepicInterstitial SupereraInterstitial;
    public BepicRewardVideo SupereraRewardVideo;

    public IBanner Banner { get => SupereraBanner; set => SupereraBanner = (BepicBanner)value; }
    public IInterstitial Interstitial { get => SupereraInterstitial; set => SupereraInterstitial = (BepicInterstitial)value; }
    public IRewardVideo RewardVideo { get => SupereraRewardVideo; set => SupereraRewardVideo = (BepicRewardVideo)value; }

    public event Action OnInterstitialAdReadyEvent;
    public event Action OnInitializedCallback;

    public void OnValidate()
    {
        SetRefs();
    }

    public void SetRefs()
    {
        if (Banner == null) Banner = this.GetComponentInChildren<BepicBanner>(true);
        if (Interstitial == null) Interstitial = this.GetComponentInChildren<BepicInterstitial>(true);
        if (RewardVideo == null) RewardVideo = this.GetComponentInChildren<BepicRewardVideo>(true);
    }

    public void Init(Action i_InitializedCallback)
    {
        OnInitializedCallback += i_InitializedCallback;

        Banner.Initialize();
        Interstitial.Initialize();
        RewardVideo.Initialize();

        IsInit = true;

        OnInitializedCallback.InvokeSafe();
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

    public bool IsInterstitialAvailable()
    {
        return Interstitial.IsInterstitialAvailable();
    }

    public void LoadInterstitial()
    {
        Interstitial.LoadInterstitial();
    }

    public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd = false)
    {
        Interstitial.ShowInterstitial(i_InterstitialEvents, i_IsDummyAd);
    }

    public bool IsRewardVideoAvailable()
    {
        return RewardVideo.IsRewardVideoAvailable();
    }

    public void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd = false)
    {
        RewardVideo.ShowRewardVideo(i_RewardVideoEvents, i_IsDummyAd);
    }

    public void OnApplicationPause(bool i_Pause)
    {
    }

    public void ShowMediationDebugger()
    {
    }
}
