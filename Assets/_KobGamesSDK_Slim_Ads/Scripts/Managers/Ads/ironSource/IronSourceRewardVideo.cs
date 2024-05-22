using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class IronSourceRewardVideo : MonoBehaviour, IRewardVideo
    {
        public static string ClassName => nameof(IronSourceRewardVideo);

        private RewardVideoEvents m_RewardVideoEventsDummy;
        private bool m_IsRVSuccess = false;

        public void Initialize()
        {
#if ENABLE_IRONSOURCE
            IronSourceEvents.onRewardedVideoAdOpenedEvent += OnRewardedAdDisplayedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += OnRewardedAdDismissedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += OnRewardedAdLoadedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedAdReceivedRewardEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += OnRewardedAdFailedToDisplayEvent;
#endif
        }

        public bool IsRewardVideoAvailable()
        {
#if ENABLE_IRONSOURCE
            return IronSource.Agent.isRewardedVideoAvailable();
#else
            return false;
#endif
        }

        public void ShowRewardVideo(RewardVideoEvents i_RewardVideoCallbacks, bool i_IsDummyAd)
        {
#if ENABLE_IRONSOURCE
            m_RewardVideoEventsDummy = i_RewardVideoCallbacks;

            if (i_IsDummyAd)
            {
                OnRewardedAdDisplayedEvent();
            }
            else
            {
                if (Application.isEditor || IsRewardVideoAvailable())
                {
                    IronSource.Agent.showRewardedVideo(m_RewardVideoEventsDummy.PlacementId);

                    if (Application.isEditor)
                    {
                        OnRewardedAdDisplayedEvent();
                    }
                }
                else
                {
                    Debug.Log($"{nameof(MAXRewardVideo)}-{Utils.GetFuncName()}: RewardVideo is not available");
                }
            }
#endif
        }

        private void OnRewardedAdLoadedEvent(bool i_IsAvailable)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Availability - {i_IsAvailable}");
        }

        private void OnRewardedAdFailedToDisplayEvent(IronSourceError i_IronSourceError)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video Show Failed, Code: {i_IronSourceError.getErrorCode()}  Description: {i_IronSourceError.getDescription()}");

            // Making the reward videos call back thread safe
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_RewardVideoEventsDummy?.OnFailed(new IAdNetworkError(i_IronSourceError.getErrorCode(), i_IronSourceError.getDescription()));

                Utils.UnMuteAllAudioSources();
            });
        }

        private void OnRewardedAdDisplayedEvent()
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video Opened");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Utils.MuteAllAudioSources();

                m_RewardVideoEventsDummy?.OnOpened();
            });
        }


        private void OnRewardedAdReceivedRewardEvent(IronSourcePlacement i_Placement)
        {
            // Rewarded ad was displayed and user should receive the reward
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video Finished with reward, Placement = {i_Placement.getPlacementName()} RewardName = {i_Placement.getRewardName()}");

            // Making the reward videos call back thread safe
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_IsRVSuccess = true;

                m_RewardVideoEventsDummy?.OnSuccess();
            });
        }

        private void OnRewardedAdDismissedEvent()
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video is closing... Success: {m_IsRVSuccess}");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"{ClassName}-{Utils.GetFuncName()}-MainThreadDispatcher: Reward Video Closed. Success: {m_IsRVSuccess}");

                //Debug.LogError("CLOSED: " + Time.unscaledTime);
                m_RewardVideoEventsDummy?.OnClosed(m_IsRVSuccess);
                m_IsRVSuccess = false;

                Utils.UnMuteAllAudioSources();
            }, 0.05f);
        }

        public void SimulateShownSuccessAndClose()
        {
            if (m_RewardVideoEventsDummy == null)
            {
                m_RewardVideoEventsDummy = new RewardVideoEvents();

                // This is used to inject global events that other scripts listens to
                m_RewardVideoEventsDummy.InjectCallBacks(AdsManager.RewardVideoEventsGlobal);
            }

            OnRewardedAdReceivedRewardEvent(new IronSourcePlacement("Simulated", "Simulated", 100));
            OnRewardedAdDismissedEvent();
        }

        public void SimulateFailedRewardVideoLoad()
        {
            if (m_RewardVideoEventsDummy == null)
            {
                m_RewardVideoEventsDummy = new RewardVideoEvents();

                // This is used to inject global events that other scripts listens to
                m_RewardVideoEventsDummy.InjectCallBacks(AdsManager.RewardVideoEventsGlobal);
            }

            OnRewardedAdFailedToDisplayEvent(new IronSourceError(100, "Simulated RewardVideo Error"));
            OnRewardedAdDismissedEvent();
        }
    }
#if !ENABLE_IRONSOURCE
    public class IronSourcePlacement
    {
        private string v1;
        private string v2;
        private int v3;

        public IronSourcePlacement(string v1, string v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public string getPlacementName()
        {
            return "Dummy Placement";
        }

        public string getRewardName()
        {
            return "Dummy Reward Name";
        }
    }
#endif
}