using UnityEngine;

namespace KobGamesSDKSlim
{
    public class MAXBanner : MonoBehaviour, IBanner
    {
        public static string ClassName => nameof(MAXBanner);

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

        private string m_BannerAdUnitId
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return GameSettings.Instance.AdsMediation.MAXBannerAdUnitIdAndroid;
                }
                else
                {
                    return GameSettings.Instance.AdsMediation.MAXBannerAdUnitIdIOS;
                }
            }
        }

        public void Initialize()
        {
#if ENABLE_MAX
            //MaxSdkCallbacks.OnBannerAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.OnBannerAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
#endif
        }

        public void ShowBanner(BannerEvents i_BannerEvents)
        {
#if ENABLE_MAX
            m_BannerEventsDummy = i_BannerEvents;

            // If banner wasn't requested let's request one first
            if (!m_IsBannerRequested)
            {
                m_IsBannerRequested = true;

                // Setup
                MaxSdk.DestroyBanner(m_BannerAdUnitId);
                MaxSdk.CreateBanner(m_BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
                MaxSdk.SetBannerBackgroundColor(m_BannerAdUnitId, Color.white);
                MaxSdk.SetBannerPlacement(m_BannerAdUnitId, i_BannerEvents.PlacementId == Constants.k_None ? PlatformPlacementIds : i_BannerEvents.PlacementId);
            }

            ShowBanner();
#endif
        }

        public void ShowBanner()
        {
#if ENABLE_MAX
            // Show
            MaxSdk.ShowBanner(m_BannerAdUnitId);
            OnBannerAdScreenPresentedEvent(m_BannerAdUnitId);
#endif
        }

        public void HideBanner()
        {
#if ENABLE_MAX
            MaxSdk.HideBanner(m_BannerAdUnitId);
            BannerAdScreenDismissedEvent();
#endif
        }

        public void DestroyBanner()
        {
#if ENABLE_MAX
            MaxSdk.DestroyBanner(m_BannerAdUnitId);

            m_IsBannerRequested = false;

            BannerAdDestroyedEvent();
#endif
        }

        //private void OnBannerAdLoadedEvent(string i_AdUnitId)
        //{
        //    Debug.Log($"{nameof(MAXBanner)}-{Utils.GetFuncName()} AdUnitId: {i_AdUnitId}");
        //}

        private void OnBannerAdLoadFailedEvent(string i_AdUnitId, int i_ErrorCode)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Failed to load banner, Code: {i_ErrorCode}  AdUnit: {i_AdUnitId}");

            m_BannerEventsDummy?.OnFailed(new IAdNetworkError(i_ErrorCode, i_AdUnitId));

            // Note: note sure if we need to hide banner in case of an error, commenting for now
            // HideBanner();
        }

        private void BannerAdScreenDismissedEvent()
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}");
            m_BannerEventsDummy?.OnHidden();
        }

        private void BannerAdDestroyedEvent()
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}");

            m_BannerEventsDummy?.OnDestroyed();
            BannerAdScreenDismissedEvent();
        }

        private void OnBannerAdScreenPresentedEvent(string i_AdUnitId)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()} AdUnitId: {i_AdUnitId}");
            m_BannerEventsDummy?.OnShown();
        }

        public void SimulateFailedBannerLoad()
        {
            debugPrepareDummy();

            OnBannerAdLoadFailedEvent(m_BannerAdUnitId, 100); //"Simulated Editor Error, this is just a test"
        }

        private void debugPrepareDummy()
        {
            if (m_BannerEventsDummy == null)
            {
                m_BannerEventsDummy = new BannerEvents();

                // This is used to inject global events that other scripts listens to
                m_BannerEventsDummy.InjectCallBacks(AdsManager.BannerEventsGlobal);
            }
        }

        public void OnApplicationPause(bool i_Pause)
        {
        }
    }
}