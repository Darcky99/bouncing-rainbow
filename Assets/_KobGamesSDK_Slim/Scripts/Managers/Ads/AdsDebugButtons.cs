using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    public class AdsDebugButtons : MonoBehaviour
    {
        public int ClickCounterDefault = 6;
        public float AllowedLastClickDeltaTime = 2;

        [ReadOnly, NonSerialized, ShowInInspector]
        public int ClickCounter = -1;

        private string m_ForceDummyAdsFormat = string.Empty;
        public Text ForceDummyAds;

        private void Awake()
        {
            gameObject.SetActive(false);

            if (ForceDummyAds != null && m_ForceDummyAdsFormat == string.Empty) 
                m_ForceDummyAdsFormat = ForceDummyAds.text;

        }

        private void OnEnable()
        {
            if (ForceDummyAds != null)
            {
                ForceDummyAds.text = string.Format(m_ForceDummyAdsFormat, GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice);
            }
        }

        private float m_TimeFromLastClick = -1;
        [ShowInInspector] private float m_TimeFromLastClickDelta => Time.realtimeSinceStartup - m_TimeFromLastClick;

        public void TryOpenTestSuiteClicked()
        {
            if (Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsTestSuiteEnabled.Value || Application.isEditor)
            {
                if (m_TimeFromLastClick >= 0 && m_TimeFromLastClickDelta <= AllowedLastClickDeltaTime)
                {
                    ClickCounter--;

                    if (ClickCounter <= 0)
                    {
                        open();
                    }
                }
                else
                {
                    m_TimeFromLastClick = Time.realtimeSinceStartup;
                    ClickCounter = ClickCounterDefault - 1; // -1 because we count current click as 1

                    Debug.LogWarning($"{this.name}:{Utils.GetFuncName()}(), Out of AllowedLastClickDeltaTime, Resetting. ({m_TimeFromLastClickDelta} / {AllowedLastClickDeltaTime})");
                }

                m_TimeFromLastClick = Time.realtimeSinceStartup;
            }
            else
            {
                Debug.LogWarning($"{this.name}:{Utils.GetFuncName()}(), Disabled by RemoteSettings");
            }
        }

        private void open()
        {
            this.gameObject.SetActive(true);
            ClickCounter = ClickCounterDefault;

            Managers.Instance.Analytics.LogEvent("TestSuite", "ShowingAdsDebugButtons", "None", 0);
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
            ClickCounter = ClickCounterDefault;
        }

        public void ShowMediationDebugPanel()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.ShowMediationDebugPanel();
#endif
        }

        public void ToggleForceDummyAds(TextMeshProUGUI i_Text)
        {
            GameConfig.Instance.AdsMediation.ToggleForceUseDummyAdsOnDevice();

            i_Text.text = string.Format(m_ForceDummyAdsFormat, GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice);
        }

        public void TestShowBanner()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.ShowBanner();
#endif
        }

        public void TestFailBanner()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.IMediationNetwork.Banner.SimulateFailedBannerLoad();
#endif
        }

        public void TestHideBanner()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.HideBanner();
#endif
        }

        public void TestDestroyBanner()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.DestroyBanner();
#endif
        }

        public void TestLoadInterstitial()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.LoadInterstitial();
#endif
        }

        public void TestShowInterstitial()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.ShowInterstitial();
#endif
        }

        public void TestFailInterstitial()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.IMediationNetwork.Interstitial.SimulateFailedInterstitialLoad();
#endif
        }

        public void TestShowRewardVideo()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.ShowRewardVideo(i_PlacementId =>
            {
                Debug.LogError("DebugButton: RewardVideo finished, Placement: " + i_PlacementId);
            }
            , "TestPlacement");
#endif
        }

        public void TestFailShowRewardVideo()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.IMediationNetwork.RewardVideo.SimulateFailedRewardVideoLoad();
#endif
        }
    }
}