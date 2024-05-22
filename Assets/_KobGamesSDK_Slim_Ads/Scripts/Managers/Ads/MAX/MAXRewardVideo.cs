using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class MAXRewardVideo : MonoBehaviour, IRewardVideo
    {
        public static string ClassName => nameof(MAXRewardVideo);

        private RewardVideoEvents m_RewardVideoEventsDummy;
        private bool m_IsRVSuccess = false;

        private int m_RetryAttempt = 0;

        private string m_RewardVideoAdUnitId
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return GameSettings.Instance.AdsMediation.MAXRewardVideoAdUnitIdAndroid;
                }
                else
                {
                    return GameSettings.Instance.AdsMediation.MAXRewardVideoAdUnitIdIOS;
                }
            }
        }

        public void Initialize()
        {
#if ENABLE_MAX
            // Attach callback
            MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first RewardedAd
            LoadRewardVideo();
#endif
        }

        private void LoadRewardVideo()
        {
#if ENABLE_MAX
            MaxSdk.LoadRewardedAd(m_RewardVideoAdUnitId);
#endif
        }

        public bool IsRewardVideoAvailable()
        {
#if ENABLE_MAX
            return MaxSdk.IsRewardedAdReady(m_RewardVideoAdUnitId);
#else
            return false;
#endif
        }

        public void ShowRewardVideo(RewardVideoEvents i_RewardVideoCallbacks, bool i_IsDummyAd = false)
        {
#if ENABLE_MAX
            m_RewardVideoEventsDummy = i_RewardVideoCallbacks;

            if (i_IsDummyAd)
            {
                OnRewardedAdDisplayedEvent(m_RewardVideoEventsDummy.PlacementId);
            }
            else
            {
                if (Application.isEditor || IsRewardVideoAvailable())
                {
                    MaxSdk.ShowRewardedAd(m_RewardVideoAdUnitId, m_RewardVideoEventsDummy.PlacementId);
                }
                else
                {
                    Debug.Log($"{ClassName}-{Utils.GetFuncName()}: RewardVideo is not available");
                }
            }
#endif
        }

        private void OnRewardedAdLoadedEvent(string adUnitId)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Availability - True, AdUnitId: {adUnitId}");

            // Reset retry attempt
            m_RetryAttempt = 0;
        }

        private double m_RetryDelay = 0;
        private void OnRewardedAdLoadFailedEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to load. We recommend retrying with exponentially higher delays.
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Availability - False, AdUnitId: {adUnitId} ErrorCode: {errorCode}");

            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 16 seconds).
            m_RetryAttempt++;
            m_RetryDelay = Math.Pow(2, Math.Min(4, m_RetryAttempt)); // delay up to 2^4 = 16 seconds

            Invoke(nameof(LoadRewardVideo), (float)m_RetryDelay);
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video Show Failed, Code: {errorCode}  adUnitId: {adUnitId}");

            // Making the reward videos call back thread safe
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_RewardVideoEventsDummy?.OnFailed(new IAdNetworkError(errorCode, adUnitId));

                Utils.UnMuteAllAudioSources();
            });

            // Rewarded ad failed to display. We recommend loading the next ad
            LoadRewardVideo();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video Opened");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Utils.MuteAllAudioSources();

                m_RewardVideoEventsDummy?.OnOpened();
            });
        }

        private void OnRewardedAdClickedEvent(string adUnitId) { }

#if ENABLE_MAX
        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward)
        {
            // Rewarded ad was displayed and user should receive the reward
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video Finished with reward, Placement = {m_RewardVideoEventsDummy.PlacementId} AdUnitId = {adUnitId}");

            // Making the reward videos call back thread safe
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_IsRVSuccess = true;

                m_RewardVideoEventsDummy?.OnSuccess();
            });
        }
#endif

        private void OnRewardedAdDismissedEvent(string adUnitId)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Reward Video is closing... Success: {m_IsRVSuccess}");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"{ClassName}-{Utils.GetFuncName()}-MainThreadDispatcher: Reward Video Closed. Success: " + m_IsRVSuccess);

                //Debug.LogError("CLOSED: " + Time.unscaledTime);
                m_RewardVideoEventsDummy?.OnClosed(m_IsRVSuccess);
                m_IsRVSuccess = false;

                Utils.UnMuteAllAudioSources();
            }, 0.05f);

            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardVideo();
        }

        public void SimulateShownSuccessAndClose()
        {
#if ENABLE_MAX
            if (m_RewardVideoEventsDummy == null)
            {
                m_RewardVideoEventsDummy = new RewardVideoEvents();

                // This is used to inject global events that other scripts listens to
                m_RewardVideoEventsDummy.InjectCallBacks(AdsManager.RewardVideoEventsGlobal);
            }

            OnRewardedAdReceivedRewardEvent(m_RewardVideoAdUnitId, new MaxSdkBase.Reward());
            OnRewardedAdDismissedEvent(m_RewardVideoAdUnitId);
#endif
        }

        public void SimulateFailedRewardVideoLoad()
        {
            if (m_RewardVideoEventsDummy == null)
            {
                m_RewardVideoEventsDummy = new RewardVideoEvents();

                // This is used to inject global events that other scripts listens to
                m_RewardVideoEventsDummy.InjectCallBacks(AdsManager.RewardVideoEventsGlobal);
            }

            OnRewardedAdFailedToDisplayEvent(m_RewardVideoAdUnitId, 100);
            OnRewardedAdDismissedEvent(m_RewardVideoAdUnitId);
        }
    }
}