using System;
using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    public class AdsScreensDummy : MonoBehaviour
    {
        public GameObject InterstitialScreen;
        public GameObject RewardVideoScreen;

        public Button InterstitialScreenCloseButton;
        public Button RewardVideoScreenCloseButton;
        public Button RewardVideoScreenFailButton;

        private bool m_IsAdsScreensDummyEnabledEditor = false;
        private bool m_IsAdsScreensDummyEnabled
        {
            get
            {
#if UNITY_EDITOR && (ENABLE_IRONSOURCE || ENABLE_SUPERERA || !ENABLE_ADS)
                m_IsAdsScreensDummyEnabledEditor = true;
#endif

                return m_IsAdsScreensDummyEnabledEditor || GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice;
            }
        }

        private bool m_OriginalUseDummyAdsOnDeviceValue = false;
        private void Awake()
        {
            m_OriginalUseDummyAdsOnDeviceValue = GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice;

            AdsMediation.OnUseDummyAdsOnDeviceFromTestSuiteChangeEvent += onUseDummyAdsOnDeviceFromTestSuiteChangeEvent;

            registerEvents();
        }

        private void OnDestroy()
        {
            AdsMediation.OnUseDummyAdsOnDeviceFromTestSuiteChangeEvent -= onUseDummyAdsOnDeviceFromTestSuiteChangeEvent;

            GameConfig.Instance.AdsMediation.SetForceUseDummyAdsOnDevice(m_OriginalUseDummyAdsOnDeviceValue);
        }

        private void onUseDummyAdsOnDeviceFromTestSuiteChangeEvent(bool i_IsDummyAds)
        {
            Debug.Log($"onUseDummyAdsOnDeviceFromTestSuiteChangeEvent: {i_IsDummyAds} , m_IsAdsScreensDummyEnabled: {m_IsAdsScreensDummyEnabled}");

            unRegisterEvents();

            if (i_IsDummyAds)
            {
                registerEvents();
            }
        }

        private void registerEvents()
        {
            if (m_IsAdsScreensDummyEnabled)
            {
                AdsManager.InterstitialEventsGlobal.OpenEvent += OnInterstitialOpened;
                AdsManager.RewardVideoEventsGlobal.OpenEvent += OnRewardVideoOpened;

                InterstitialScreenCloseButton?.onClick.AddListener(interstitialClosedButtonClick);
                RewardVideoScreenCloseButton?.onClick.AddListener(rewardVideoClosedButtonClick);
                RewardVideoScreenFailButton?.onClick.AddListener(rewardVideoClosedFailClick);
            }
        }

        private void unRegisterEvents()
        {
            AdsManager.InterstitialEventsGlobal.OpenEvent -= OnInterstitialOpened;
            AdsManager.RewardVideoEventsGlobal.OpenEvent -= OnRewardVideoOpened;

            InterstitialScreenCloseButton?.onClick.RemoveListener(interstitialClosedButtonClick);
            RewardVideoScreenCloseButton?.onClick.RemoveListener(rewardVideoClosedButtonClick);
            RewardVideoScreenFailButton?.onClick.RemoveListener(rewardVideoClosedFailClick);
        }


        private void OnInterstitialOpened(string i_Placement)
        {
            //if (Application.isEditor)
            {
                if (InterstitialScreen != null)
                {
                    InterstitialScreen.SetActive(true);
                }
                else
                {
                    Debug.LogError("AdsScreensDummy - InterstitialScreen is null, please check refs");
                }
            }
        }

        private void interstitialClosedButtonClick()
        {
            //if (Application.isEditor)
            {
                AdsManager.Instance.IMediationNetwork.Interstitial.SimulateShownSuccessAndClose();
            }
        }

        private void OnRewardVideoOpened(string i_Placement)
        {
            //if (Application.isEditor)
            {
                if (RewardVideoScreen != null)
                {
                    RewardVideoScreen.SetActive(true);
                }
                else
                {
                    Debug.LogError("AdsScreensDummy - RewardVideoScreen is null, please check refs");
                }
            }
        }

        private void rewardVideoClosedButtonClick()
        {
            //if (Application.isEditor)
            {
                AdsManager.Instance.IMediationNetwork.RewardVideo.SimulateShownSuccessAndClose();
            }
        }

        private void rewardVideoClosedFailClick()
        {
            //if (Application.isEditor)
            {
                AdsManager.Instance.IMediationNetwork.RewardVideo.SimulateFailedRewardVideoLoad();
            }
        }
    }
}