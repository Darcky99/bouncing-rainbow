using KobGamesSDKSlim;
using UnityEngine;
#if ENABLE_BEPIC
using AnyThinkAds.Api;
using SimpleSDKNS;

public class BepicRewardVideo : MonoBehaviour, IRewardVideo, SimpleSDKRewardedVideoListener
{
    private RewardVideoEvents m_RewardVideoEventsDummy;

    private bool m_IsRVSuccess = false;
    private string m_GameEntry = "RV_Generic";

    public void Initialize()
    {
        SimpleSDKAd.instance.SetSimpleSDKRewardedVideoListener(this);
    }

    public void ShowRewardVideo(RewardVideoEvents i_RewardVideoCallbacks, bool i_IsDummyAd)
    {
        m_RewardVideoEventsDummy = i_RewardVideoCallbacks;
        m_GameEntry = i_RewardVideoCallbacks.PlacementId == Constants.k_None ? m_GameEntry : i_RewardVideoCallbacks.PlacementId;

        if (i_IsDummyAd)
        {
            onRewardedVideoAdPlayStart(m_GameEntry, null);
        }
        else
        {
            if (IsRewardVideoAvailable())
            {
                Debug.Log($"{nameof(BepicRewardVideo)}-{Utils.GetFuncName()}: ShowRewardVideo, GameEntry: {m_GameEntry}");

                SimpleSDKAd.instance.ShowReward(m_GameEntry);
            }
            else
            {
                Debug.Log($"{nameof(BepicRewardVideo)}-{Utils.GetFuncName()}: RewardVideo is not available");
            }
        }
    }

    public bool IsRewardVideoAvailable()
    {
        return Application.isEditor || SimpleSDKAd.instance.HasReward();
    }

    public void onRewardedVideoAdPlayStart(string i_GameEntry, ATCallbackInfo callbackInfo)
    {
        Debug.Log($"{nameof(BepicRewardVideo)}-{Utils.GetFuncName()}: Reward Video Opened, GameEntry: {i_GameEntry}");

        Utils.MuteAllAudioSources();

        m_RewardVideoEventsDummy?.OnOpened();
    }

    public void onRewardedVideoAdPlayFail(string i_GameEntry, string i_ErrorCode, string i_Msg)
    {
        Debug.Log($"{nameof(BepicRewardVideo)}-{Utils.GetFuncName()}: Reward Video Show Failed, GameEntry: {i_GameEntry}  Code: {i_ErrorCode}  Message: {i_Msg}");

        m_RewardVideoEventsDummy?.OnFailed(new IAdNetworkError(i_ErrorCode, i_Msg));

        Utils.UnMuteAllAudioSources();
    }

    public void onRewardedVideoAdPlayClosed(string i_GameEntry, bool i_IsReward, ATCallbackInfo callbackInfo)
    {
        Debug.Log($"{nameof(BepicRewardVideo)}-{Utils.GetFuncName()}: Reward Video Finished with reward, GameEntry: {i_GameEntry}");

        Utils.UnMuteAllAudioSources();

        m_IsRVSuccess = i_IsReward;

        if (i_IsReward)
        {
            m_RewardVideoEventsDummy?.OnSuccess();
        }

        Debug.Log($"{nameof(BepicRewardVideo)}-{Utils.GetFuncName()}-MainThreadDispatcher: Reward Video Closed. Success: " + m_IsRVSuccess);

        m_RewardVideoEventsDummy?.OnClosed(m_IsRVSuccess);
    }

    public void onRewardedVideoAdPlayClicked(string i_GameEntry, ATCallbackInfo callbackInfo)
    {
    }

    public void SimulateFailedRewardVideoLoad()
    {
        if (m_RewardVideoEventsDummy == null)
        {
            m_RewardVideoEventsDummy = new RewardVideoEvents();

            // This is used to inject global events that other scripts listens to
            m_RewardVideoEventsDummy.InjectCallBacks(AdsManager.RewardVideoEventsGlobal);
        }

        onRewardedVideoAdPlayFail(m_GameEntry, "100", "Simulated Editor Error, this is just a test");
    }

    public void SimulateShownSuccessAndClose()
    {
        if (m_RewardVideoEventsDummy == null)
        {
            m_RewardVideoEventsDummy = new RewardVideoEvents();

            // This is used to inject global events that other scripts listens to
            m_RewardVideoEventsDummy.InjectCallBacks(AdsManager.RewardVideoEventsGlobal);
        }

        onRewardedVideoAdPlayClosed(m_GameEntry, true, null);
    }    
}
#else
public class BepicRewardVideo : MonoBehaviour, IRewardVideo
{
    public void Initialize()
    {
    }

    public bool IsRewardVideoAvailable()
    {
        return true;
    }

    public void ShowRewardVideo(RewardVideoEvents i_RewardVideoEvents, bool i_IsDummyAd)
    {
    }

    public void SimulateFailedRewardVideoLoad()
    {
    }

    public void SimulateShownSuccessAndClose()
    {
    }
}
#endif