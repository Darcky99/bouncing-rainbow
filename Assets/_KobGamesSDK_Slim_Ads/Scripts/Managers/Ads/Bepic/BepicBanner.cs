using KobGamesSDKSlim;
#if ENABLE_BEPIC
using SimpleSDKNS;
#endif
using UnityEngine;

public class BepicBanner : MonoBehaviour, IBanner
{
    private BannerEvents m_BannerEventsDummy;
    private bool m_IsBannerRequested = false;

    public string AndroidPlacementId = "banner_android";
    public string IOSPlacementId = "banner_ios";
    public string PlatformPlacementIds
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
    }

    public void ShowBanner(BannerEvents i_BannerEvents)
    {
        m_BannerEventsDummy = i_BannerEvents;

        // If banner wasn't requested let's request one first
        if (!m_IsBannerRequested)
        {
            m_IsBannerRequested = true;

#if ENABLE_BEPIC
            SimpleSDKAd.instance.ShowBanner(BannerPos.BOTTOM);
#endif
        }

        ShowBanner();

    }

    public void ShowBanner()
    {
        // Show
        OnBannerAdScreenPresentedEvent();
    }

    public void DestroyBanner()
    {
    }

    public void HideBanner()
    {
#if ENABLE_BEPIC
        SimpleSDKAd.instance.HideBanner();
#endif

        BannerAdDestroyedEvent();
    }

    private void OnBannerAdScreenPresentedEvent()
    {
        Debug.Log($"{nameof(BepicBanner)}-{Utils.GetFuncName()}");
        m_BannerEventsDummy?.OnShown();
    }

    private void BannerAdScreenDismissedEvent()
    {
        Debug.Log($"{nameof(BepicBanner)}-{Utils.GetFuncName()}");
        m_BannerEventsDummy?.OnHidden();
    }

    private void BannerAdDestroyedEvent()
    {
        Debug.Log($"{nameof(BepicBanner)}-{Utils.GetFuncName()}");

        m_BannerEventsDummy?.OnDestroyed();
        BannerAdScreenDismissedEvent();
    }

    public void OnApplicationPause(bool i_Pause)
    {
    }

    public void SimulateFailedBannerLoad()
    {
    }
}
