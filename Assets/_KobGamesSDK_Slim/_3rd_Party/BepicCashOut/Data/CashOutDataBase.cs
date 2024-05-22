#if ENABLE_BEPIC || ENABLE_KOBIC
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace KobGamesSDKSlim.Bepic
{
    [Serializable]
    public class CashOutDataBase
    {
        private const string k_CashOutBoxGroupText = "Editor Only";
        private const string k_CashOutDisabledText = "CashOut is currently Disabled in Editor";
        private const string k_CashOutSuffixText = "-1 will fetch value from back-end";

        [InfoBox(k_CashOutDisabledText, visibleIfMemberName: "@!this.IsCashOutEnabled", InfoMessageType = InfoMessageType.Warning)]
        [BoxGroup(k_CashOutBoxGroupText)]
        public bool IsCashOutEnabled = false;

        [BoxGroup(k_CashOutBoxGroupText)]
        [ShowIf(nameof(IsCashOutEnabled)), SuffixLabel(k_CashOutSuffixText)]
        public int ForceCashCollectValue = -1;

        [BoxGroup(k_CashOutBoxGroupText)]
        [ShowIf(nameof(IsCashOutEnabled)), SuffixLabel(k_CashOutSuffixText)]
        public int ForceFxItemValue = -1;

        [BoxGroup(k_CashOutBoxGroupText), SuffixLabel("[Device Only Var] - Will force Cashout on Debug Builds")]
        public bool ForceCashOutOnDevice = false;

#if ENABLE_KOBIC
        public CashRewardFreePPData CashRewardFreePP;
        public CashRewardData CashReward;
#endif

#if ENABLE_BEPIC
        [LabelText("Debug")]
        public DebugData DebugEditor;
        public DailySignInData DailySignIn;
#endif
        public CountdownData Countdown;

        [ShowInInspector] private bool PPWrapperIsWork { get { return PPWrapper.IsWork; } }
#if ENABLE_BEPIC
        [ShowInInspector] private string Status { get { return PPWrapper.GetFxStatus().ToString(); } }
#endif
#if ENABLE_KOBIC
        [ShowInInspector] private AttributionStatus PPAttribution { get { return PPWrapper.AttributionStatus; } }
#endif

        [System.Serializable]
        public class DebugData
        {
            [PropertyOrder(1), Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
            public void GetCashCollectValue(PPEntries i_PPEntry)
            {
                PPWrapper.GetCashCollectValue(i_PPEntry);
            }

            [PropertyOrder(2), Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
            public void GetCashCollectDailySignValue(PPDailySign i_PPDailySign)
            {
                PPWrapper.GetCashCollectValue(i_PPDailySign);
            }

            private const string k_BoxGroupTargetAmount = "TargetAmount";
            private bool m_IsPlayingAndIsWork => Application.isPlaying && PPWrapper.IsWork;
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string General => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(0) / 100} ({PPWrapper.GetCashOutAmountPerItem(0)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string PP1 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(1) / 100} ({PPWrapper.GetCashOutAmountPerItem(1)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string PP2 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(2) / 100} ({PPWrapper.GetCashOutAmountPerItem(2)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string PP3 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(3) / 100} ({PPWrapper.GetCashOutAmountPerItem(3)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string PP4 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(4) / 100} ({PPWrapper.GetCashOutAmountPerItem(4)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string Amazon1 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(5) / 100} ({PPWrapper.GetCashOutAmountPerItem(5)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string Amazon2 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(6) / 100} ({PPWrapper.GetCashOutAmountPerItem(6)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string Amazon3 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(7) / 100} ({PPWrapper.GetCashOutAmountPerItem(7)})" : "-1";
            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), ShowInInspector]
            public string Amazon4 => m_IsPlayingAndIsWork ? $"${PPWrapper.GetCashOutAmountPerItem(8) / 100} ({PPWrapper.GetCashOutAmountPerItem(8)})" : "-1";

            [PropertyOrder(3), BoxGroup(k_BoxGroupTargetAmount), Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
            public int GetCashOutTargetAmountPerItem(int i_ItemId)
            {
                var resultInCents = PPWrapper.GetCashOutAmountPerItem(i_ItemId);
                Debug.Log($"Fx: GetCashOutAmountPerItem for Item: {i_ItemId} Value: {resultInCents} ${resultInCents/100}");

                return resultInCents;
            }

            [PropertyOrder(4), Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
            public void AddCashToBalance(float i_CashAmountInDollars)
            {
                PPWrapper.AddCashForDebug((int)(i_CashAmountInDollars * 100));
            }
        }

        [System.Serializable]
        public class CountdownData
        {
            private const string k_LastTimePlayerPref = "KG.Game.Data.CashOutData.LastTime01";

            [SerializeField]
            private bool m_IsWork;
            public bool IsWork
            {
                get
                {
                    return m_IsWork && PPWrapper.IsWork;
                }
            }

            public int Time = 300;
            public float NoThanksDelay = 1;

            [ShowInInspector]
            public int TimerSecCount
            {
                get
                {
                    if (!PlayerPrefs.HasKey(k_LastTimePlayerPref))
                    {
                        InitNewTimer();
                    }
                    long temp = Convert.ToInt64(PlayerPrefs.GetString(k_LastTimePlayerPref));
                    return Time - (int)((DateTime.Now - DateTime.FromBinary(temp)).TotalSeconds);
                }
            }

            public void InitNewTimer()
            {
                PlayerPrefs.SetString(k_LastTimePlayerPref, DateTime.Now.ToBinary().ToString());
            }
        }

        [System.Serializable]
        public class CashRewardFreePPData
        {
            public int FreePPAmount = 6;
#if ENABLE_KOBIC
            [ShowInInspector]
            public int FreePPAmountLeft => PPWrapper.FreePPBalance;
#endif
            public List<PPEntries> FreePPEntries = new List<PPEntries>() { PPEntries.FreePP, PPEntries.Gocha, PPEntries.CountTime };
        }

        [System.Serializable]
        public class CashRewardData
        {
            [LabelText("CashoutTargetAmount (*Cents)")]
            public int CashoutTargetAmount = 200 * 100; //20,000 Cents = 200USD as default

            [SerializeField]
            private LevelData[] m_Levels = new LevelData[]
            {
                new LevelData { Level = 0, Percent = 3.5f },
                new LevelData { Level = 5, Percent = 3.3f },
                new LevelData { Level = 12, Percent = 3.2f },
                new LevelData { Level = 20, Percent = 3.1f },
                new LevelData { Level = 30, Percent = 3f }
            };

            [SerializeField]
            private NextStepsData m_NextSteps;

            [SerializeField]
            private float m_MinPercent = 1.6f;

            [Button]
            public int GetSimulatedRewardAmount(float i_CashOutAmount, int i_Level)
            {
                float percent = 1;
                if (i_Level >= m_NextSteps.LevelStart)
                {
                    if (m_Levels.Length > 0)
                    {
                        percent = m_Levels[m_Levels.Length - 1].Percent;
                        i_Level -= m_NextSteps.LevelStart;
                    }
                    i_Level -= m_NextSteps.LevelStep;
                    percent -= m_NextSteps.PercentStep;
                    while (i_Level >= 0)
                    {
                        percent -= m_NextSteps.PercentStep;
                        i_Level -= m_NextSteps.LevelStep;
                    }
                }
                else
                {
                    for (int i = m_Levels.Length - 1; i >= 0; i--)
                    {
                        if (i_Level >= m_Levels[i].Level)
                        {
                            percent = m_Levels[i].Percent;
                            break;
                        }
                    }

                }
                percent = Mathf.Max(percent, m_MinPercent);
                return Mathf.RoundToInt(i_CashOutAmount * percent / 100.0f);
            }

            [Button]
            public float GetSimulatedRewardAmountDollar(float i_CashOutAmount, int i_Level)
            {
                return GetSimulatedRewardAmount(i_CashOutAmount * 100, i_Level) / 100f;
            }

            [System.Serializable]
            private class LevelData
            {
                public int Level;
                public float Percent;
            }

            [System.Serializable]
            private class NextStepsData
            {
                public int LevelStart = 36;
                public int LevelStep = 5;
                public float PercentStep = 0.025f;
            }
        }

#if ENABLE_BEPIC
        [System.Serializable]
        public class DailySignInData
        {
            [ShowInInspector] public int ActiveDaySignIn => PPWrapper.DailySignIn.GetActiveDaySignIn();
            [ShowInInspector] public int LastActiveDaySignIn => PPWrapper.DailySignIn.LastActiveDaySignIn;
            [ShowInInspector] public bool IsNewDaySignIn => PPWrapper.DailySignIn.IsNewDaySignIn();

            [Button]
            public void SimulateNewSignInDay()
            {
#if UNITY_EDITOR
                // Editor Only Function
                PPWrapper.DailySignIn.SimulateNewSignInDay();
#endif
            }
        }
#endif
    }
}
#endif