using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class IronSourceBanner : MonoBehaviour, IBanner
    {
        public static string ClassName => nameof(IronSourceBanner);

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
                    return GameSettings.Instance.AdsMediation.IronSourceAndroidID;
                }
                else
                {
                    return GameSettings.Instance.AdsMediation.IronSourceAppleID;
                }
            }
        }

        public void Initialize()
        {
#if ENABLE_IRONSOURCE
            IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
#endif
        }      

        public void ShowBanner(BannerEvents i_BannerEvents)
        {
#if ENABLE_IRONSOURCE
            m_BannerEventsDummy = i_BannerEvents;

            // If banner wasn't requested let's request one first
            if (!m_IsBannerRequested)
            {
                m_IsBannerRequested = true;

                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
            }

            ShowBanner();
#endif
        }

        public void ShowBanner()
        {
#if ENABLE_IRONSOURCE
            IronSource.Agent.displayBanner();
            OnBannerAdScreenPresentedEvent(m_BannerAdUnitId);
#endif
        }

        public void HideBanner()
        {
#if ENABLE_IRONSOURCE            
            IronSource.Agent.hideBanner();
            BannerAdScreenDismissedEvent();
#endif
        }

        public void DestroyBanner()
        {
#if ENABLE_IRONSOURCE
            IronSource.Agent.destroyBanner();

            m_IsBannerRequested = false;

            BannerAdDestroyedEvent();
#endif
        }

        private void BannerAdLoadFailedEvent(IronSourceError i_IronSourceError)
        {
            Debug.Log($"{ClassName}-{Utils.GetFuncName()}: Failed to load banner, Code: {i_IronSourceError.getErrorCode()}  Description: {i_IronSourceError.getDescription()}");

            m_BannerEventsDummy?.OnFailed(new IAdNetworkError(i_IronSourceError.getErrorCode().ToString(), i_IronSourceError.getDescription()));

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

            BannerAdLoadFailedEvent(new IronSourceError(100, "Simulated Banner Error")); //"Simulated Editor Error, this is just a test"
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

#if !ENABLE_IRONSOURCE
    public class IronSourceError
    {
        private int v1;
        private string v2;

        public IronSourceError(int v1, string v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public string getErrorCode()
        {
            return "100";
        }

        public string getDescription()
        {
            return "Dummy Description";
        }

        public object getCode()
        {
            return "100";
        }
    }
#endif
}