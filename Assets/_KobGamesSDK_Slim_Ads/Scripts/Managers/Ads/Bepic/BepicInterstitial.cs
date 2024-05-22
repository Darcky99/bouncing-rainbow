using System;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using UnityEngine;
#if ENABLE_BEPIC
using AnyThinkAds.Api;
using SimpleSDKNS;

public class BepicInterstitial : MonoBehaviour, IInterstitial, SimpleSDKInterstitialAdListener
{
    private InterstitialEvents m_InterstitialEventsDummy;

    private string m_GameEntry = "Interstitial_Generic";

    public static bool ShowLoadingScreen = false;
    public static Action ShowLoadingScreenStartCallback = () => { };
    public static Action ShowLoadingScreenFinishCallback = () => { };

    public void Initialize()
    {
        SimpleSDKAd.instance.SetSimpleSDKInterstitialAdListener(this);
    }

    public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd)
    {
        m_InterstitialEventsDummy = i_InterstitialEvents;
        m_GameEntry = i_InterstitialEvents.PlacementId == Constants.k_None ? m_GameEntry : i_InterstitialEvents.PlacementId;

        if (i_IsDummyAd)
        {
            onInterstitialAdShow(m_GameEntry, null);
        }
        else
        {
            if (IsInterstitialAvailable())
            {
                SimpleSDKAd.instance.ShowInterstitial(m_GameEntry);
            }
            else
            {
                Debug.Log($"{nameof(BepicInterstitial)}-{Utils.GetFuncName()}: Interstitial is not available");
            }
        }
    }

    public bool IsInterstitialAvailable()
    {
        return Application.isEditor || SimpleSDKAd.instance.HasInterstitial();
    }

    public void LoadInterstitial()
    {
    }

    public void onInterstitialAdShow(string i_GameEntry, ATCallbackInfo callbackInfo)
    {
        Debug.Log($"{nameof(BepicInterstitial)}-{Utils.GetFuncName()} GameEntry: {i_GameEntry}");
        m_InterstitialEventsDummy?.OnOpened();
        m_InterstitialEventsDummy?.OnShownSuccess();
    }

    public void onInterstitialAdClose(string i_GameEntry, ATCallbackInfo callbackInfo)
    {
        Debug.Log($"{nameof(BepicInterstitial)}-{Utils.GetFuncName()} GameEntry: {i_GameEntry}");
        m_InterstitialEventsDummy?.OnClosed();
    }

    public void onInterstitialAdClick(string i_GameEntry, ATCallbackInfo callbackInfo)
    {
        Debug.Log($"{nameof(BepicInterstitial)}-{Utils.GetFuncName()} GameEntry: {i_GameEntry}");
    }

    public void SimulateFailedInterstitialLoad()
    {
        if (m_InterstitialEventsDummy == null)
        {
            m_InterstitialEventsDummy = new InterstitialEvents();

            // This is used to inject global events that other scripts listens to
            m_InterstitialEventsDummy.InjectCallBacks(AdsManager.InterstitialEventsGlobal);
        }

        m_InterstitialEventsDummy?.OnShownFailed(new IAdNetworkError(100, "Simulated Editor Error, this is just a test"));
    }

    public void SimulateShownSuccessAndClose()
    {
        if (m_InterstitialEventsDummy == null)
        {
            m_InterstitialEventsDummy = new InterstitialEvents();

            // This is used to inject global events that other scripts listens to
            m_InterstitialEventsDummy.InjectCallBacks(AdsManager.InterstitialEventsGlobal);
        }

        //onInterstitialShown(m_GameEntry);
        onInterstitialAdClose(m_GameEntry, null);
    }   
}
#else
public class BepicInterstitial : MonoBehaviour, IInterstitial
{
    public void Initialize()
    {
    }

    public bool IsInterstitialAvailable()
    {
        return true;
    }

    public void LoadInterstitial()
    {
    }

    public void ShowInterstitial(InterstitialEvents i_InterstitialEvents, bool i_IsDummyAd)
    {
    }

    public void SimulateFailedInterstitialLoad()
    {
    }

    public void SimulateShownSuccessAndClose()
    {
    }
}
#endif