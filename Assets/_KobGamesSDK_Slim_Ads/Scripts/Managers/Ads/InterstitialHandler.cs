using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class InterstitialHandler
    {
        public enum InterstitialType
        {
            LevelFailed = 0,
            LevelCompleted = 1,
            TimeBased = 2
        }

        private const int k_LabelWidth = 320;

        private int m_InterstitialLevelFailedCounter = -1;
        private int m_InterstitialLevelCompleteCounter = -1;

        [ShowInInspector, ReadOnly]
        public int CurrentLevel
        {
            get
            {
                return AnalyticsManager.Instance.CurrentLevel;
            }
        }

        [ShowInInspector, ReadOnly, LabelWidth(k_LabelWidth)]
        public int InterstitialLevelFailedCounter
        {
            get
            {
                if (m_InterstitialLevelFailedCounter < 0)
                {
                    m_InterstitialLevelFailedCounter = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialLevelFailedCounter.Value;
                }

                return m_InterstitialLevelFailedCounter;
            }
            set
            {
                m_InterstitialLevelFailedCounter = value;
            }
        }

        [ShowInInspector, ReadOnly, LabelWidth(k_LabelWidth)]
        public int InterstitialLevelCompleteCounter
        {
            get
            {
                if (m_InterstitialLevelCompleteCounter < 0)
                {
                    m_InterstitialLevelCompleteCounter = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialLevelCompleteCounter.Value;
                }

                return m_InterstitialLevelCompleteCounter;
            }
            set
            {
                m_InterstitialLevelCompleteCounter = value;
            }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth)]
        public bool IsInterstitialRemotelyEnabled
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsInterstitialEnabled.Value == true; }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth)]
        public bool IsInterstitialRemoteLevelFailedCounterPositive
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialLevelFailedCounter.Value > 0; }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth)]
        public bool IsInterstitialRemoteLevelCompletedCounterPositive
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialLevelCompleteCounter.Value > 0; }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth), PropertyTooltip("Relevant to interstitials only")]
        public int InterstitialCoolDownCounter
        {
            get
            {
                return Managers.Instance.Storage.InterstitialCoolDownCounter;
            }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth), PropertyTooltip("Relevant to interstitials only")]
        public bool IsInterstitialCoolDownSatisfied
        {
            get
            {
                return Managers.Instance.Storage.InterstitialCoolDownCounter <= 0;
            }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth)]
        public int InterstitialCoolDown
        {
            get
            {
                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialCoolDown.Value;
            }
        }

        [ShowInInspector, LabelWidth(k_LabelWidth)]
        public int InterstitialMinLevel
        {
            get
            {
                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialMinLevel.Value;
            }
        }

        /// <summary>
        /// Try Show Interstitial, in case of 'InterstitialType.TimeBased' remember to set the counter under RemoteSettings
        /// </summary>
        /// <param name="i_InterstitialType"></param>
        public void TryShowInterstitial(InterstitialType i_InterstitialType)
        {
            Debug.Log($"{nameof(AdsManager)}-{nameof(InterstitialHandler)}.{Utils.GetFuncName()}() " +
                $"Type- {i_InterstitialType}, IsInterstitialRemotelyEnabled: {IsInterstitialRemotelyEnabled} " +
                $"CurrentLevel: {CurrentLevel} InterstitialMinLevel: {InterstitialMinLevel} " +
                $"IsInterstitialCoolDownSatisfied: {IsInterstitialCoolDownSatisfied}");

            if (IsInterstitialRemotelyEnabled)
            {
                if (CurrentLevel >= InterstitialMinLevel)
                {
                    switch (i_InterstitialType)
                    {
                        case InterstitialType.LevelFailed:
                            tryShowInterstitial(i_InterstitialType);
                            break;
                        case InterstitialType.LevelCompleted:
                            tryShowInterstitial(i_InterstitialType);
                            break;
                        case InterstitialType.TimeBased:
                            tryShowInterstitialTimedBased();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Debug.Log($"{nameof(AdsManager)}-{nameof(InterstitialHandler)}.{Utils.GetFuncName()}(): Not allowed to show interstitial CurrentLevel: {CurrentLevel} < InterstitialMinLevel: {InterstitialMinLevel}");
                }
            }
            else
            {
                Debug.Log($"{nameof(AdsManager)}-{nameof(InterstitialHandler)}.{Utils.GetFuncName()}(): {nameof(IsInterstitialRemotelyEnabled)} is disabled");
            }
        }

        private void tryShowInterstitialTimedBased()
        {
            if (IsInterstitialCoolDownSatisfied)
            {
                Managers.Instance.Ads.ShowInterstitial();
            }
            else
            {
                Debug.Log($"{nameof(AdsManager)}-{nameof(InterstitialHandler)}.{Utils.GetFuncName()}: Not allowed to show an Interstitial due to Interstitial cooldown not met, cooldown left: {Managers.Instance.Storage.InterstitialCoolDownCounter} seconds.");
            }
        }

        private void tryShowInterstitial(InterstitialType i_InterstitialType)
        {
            if (m_IsInterstitialPendingToShow)
            {
                showPendingInterstitial();
                return;
            }

            if (IsInterstitialRemoteCounterPositive(i_InterstitialType))
            {
                DecreaseInterstitialCounter(i_InterstitialType);

                Debug.Log($"{nameof(AdsManager)}-{nameof(InterstitialHandler)}.{Utils.GetFuncName()}(): SessionAttempts: {Managers.Instance.Storage.GameOverSessionAttempts} InterstitialGameOverCounter: {Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialLevelFailedCounter.Value} InterstitialLevelCompleteCounter: {Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialLevelCompleteCounter.Value} CurrentCounter: {GetInterstitialCounter(i_InterstitialType)} PendingInterstitial: {m_IsInterstitialPendingToShow}");

                if (GetInterstitialCounter(i_InterstitialType) == 0)
                {
                    // Checking if cool down counter is larger than the time from last rv, if so we need to wait and not show an Ad
                    if (IsInterstitialCoolDownSatisfied == false)
                    {
                        Debug.Log($"{Utils.GetFuncName()}: Not allowed to show an Ad due to RV cooldown not met");

                        SetInterstitialCounter(i_InterstitialType, -1);

                        m_IsInterstitialPendingToShow = true;
                        return;
                    }

                    if (Managers.Instance.Ads.IsInterstitialLoaded)
                    {
                        Managers.Instance.Ads.ShowInterstitial();

                        SetInterstitialCounter(i_InterstitialType, -1);
                    }
                    else
                    {
                        m_IsInterstitialPendingToShow = true;
                    }
                }
            }
            else
            {
                Debug.Log($"{nameof(AdsManager)}.{Utils.GetFuncName()}(): {nameof(IsInterstitialRemotelyEnabled)} is disabled or {nameof(IsInterstitialRemoteCounterPositive)} is false");
            }
        }

        [ShowInInspector, ReadOnly, LabelWidth(k_LabelWidth)]
        private bool m_IsInterstitialPendingToShow = false;
        private void showPendingInterstitial()
        {
            if (m_IsInterstitialPendingToShow)
            {
                if (IsInterstitialCoolDownSatisfied)
                {
                    if (Managers.Instance.Ads.IsInterstitialLoaded)
                    {
                        Debug.Log($"{nameof(AdsManager)}.{Utils.GetFuncName()}(): SessionAttempts: {Managers.Instance.Storage.GameOverSessionAttempts} Showing Pending Interstitial...");

                        Managers.Instance.Ads.ShowInterstitial();
                        m_IsInterstitialPendingToShow = false;
                    }
                    else
                    {
                        Debug.Log($"{nameof(AdsManager)}.{Utils.GetFuncName()}(): SessionAttempts: {Managers.Instance.Storage.GameOverSessionAttempts} Trying to show 'Pending Interstitial' but interstitial is not available");
                    }
                }
                else
                {
                    Debug.Log($"{nameof(AdsManager)}.{Utils.GetFuncName()}(): SessionAttempts: {Managers.Instance.Storage.GameOverSessionAttempts} Not allowed to show 'Pending Interstitial' since RV cooldown isn't met");
                }
            }
        }

        #region Interstitial Helpers
        private bool IsInterstitialRemoteCounterPositive(InterstitialType i_InterstitialType)
        {
            bool isInterstitialCounterPositive = false;

            if (i_InterstitialType == InterstitialType.LevelCompleted)
            {
                isInterstitialCounterPositive = IsInterstitialRemoteLevelCompletedCounterPositive;
            }
            else if (i_InterstitialType == InterstitialType.LevelFailed)
            {
                isInterstitialCounterPositive = IsInterstitialRemoteLevelFailedCounterPositive;
            }

            return isInterstitialCounterPositive;
        }

        private void SetInterstitialCounter(InterstitialType i_InterstitialType, int i_Counter)
        {
            if (i_InterstitialType == InterstitialType.LevelCompleted)
            {
                InterstitialLevelCompleteCounter = i_Counter;
            }
            else if (i_InterstitialType == InterstitialType.LevelFailed)
            {
                InterstitialLevelFailedCounter = i_Counter;
            }
        }

        private void DecreaseInterstitialCounter(InterstitialType i_InterstitialType)
        {
            if (i_InterstitialType == InterstitialType.LevelCompleted)
            {
                InterstitialLevelCompleteCounter--;
            }
            else if (i_InterstitialType == InterstitialType.LevelFailed)
            {
                InterstitialLevelFailedCounter--;
            }
        }

        private int GetInterstitialCounter(InterstitialType i_InterstitialType)
        {
            if (i_InterstitialType == InterstitialType.LevelCompleted)
            {
                return InterstitialLevelCompleteCounter;
            }
            else if (i_InterstitialType == InterstitialType.LevelFailed)
            {
                return InterstitialLevelFailedCounter;
            }

            return 0;
        }
        #endregion
    }
}