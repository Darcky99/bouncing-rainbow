#if ENABLE_KOBIC
using System;
using System.Collections.Generic;
using UnityEngine;
using KobGamesSDKSlim;
using FxNS;
#if ENABLE_ADJUST
using com.adjust.sdk;
#endif
namespace FxNS
{
    public class FxConfig { }
}

namespace KobGamesSDKSlim.Bepic
{
    public enum AttributionStatus
    {
        Organic,
        UA
    }

    public partial class PPWrapper
    {
        public static Action<FxConfig> FxInitSuccess { get; internal set; }

        public static Action OnBalanceChange = () => { };

        public static PPWrapper Instace = new PPWrapper();

        private static PPResult m_PPResult = new PPResult();

        public static AttributionStatus AttributionStatus
        {
            get => PlayerPrefs.GetString(nameof(AttributionStatus)) == nameof(AttributionStatus.UA) ? AttributionStatus.UA : AttributionStatus.Organic;
            set => PlayerPrefs.SetString(nameof(AttributionStatus), value == AttributionStatus.Organic ? nameof(AttributionStatus.Organic) : nameof(AttributionStatus.UA));
        }

        public static int CashBalance
        {
            get => PlayerPrefs.GetInt(nameof(CashBalance), 0);
            set => PlayerPrefs.SetInt(nameof(CashBalance), value);
        }

        public static int FreePPBalance
        {
            get => PlayerPrefs.GetInt(nameof(FreePPBalance), GameConfig.Instance.CashOut.CashRewardFreePP.FreePPAmount);
            set => PlayerPrefs.SetInt(nameof(FreePPBalance), value);
        }

        public static bool IsWorkDebugEnabled
        {
            get => Debug.isDebugBuild && GameConfig.Instance.CashOut.IsPPWorkOnDeviceDebug;
        }

        public static bool IsWork
        {
            get
            {
#if UNITY_EDITOR
                AttributionStatus = GameConfig.Instance.CashOut.IsCashOutEnabled ? AttributionStatus.UA : AttributionStatus.Organic;
#endif

                if (IsWorkDebugEnabled)
                {
                    AttributionStatus = AttributionStatus.UA;
                }

                return AttributionStatus == AttributionStatus.UA;
            }
        }

        public static bool GetFxStatus()
        {
            return IsWork;
        }

        public PPWrapper()
        {
            //Debug.Log("Fx: Ctor");
            Instace = this;
        }

        private static bool m_IsInit = false;
        public static void Init()
        {
            if (!m_IsInit)
            {
                m_IsInit = true;

#if ENABLE_ADJUST
                Debug.Log($"Fx: Init, Attribution: {AttributionStatus} IsWork: {IsWork}");
                attributedInstallInvoker();

                Adjust adjust = GameObject.FindObjectOfType<Adjust>();
                Debug.Log($"Fx: found Adjust, Instance: {adjust}");

                AdjustConfig adjustConfig = new AdjustConfig(adjust.appToken, adjust.environment, (adjust.logLevel == AdjustLogLevel.Suppress));
                adjustConfig.setLogLevel(adjust.logLevel);
                adjustConfig.setSendInBackground(adjust.sendInBackground);
                adjustConfig.setEventBufferingEnabled(adjust.eventBuffering);
                adjustConfig.setLaunchDeferredDeeplink(adjust.launchDeferredDeeplink);
                adjustConfig.setAttributionChangedDelegate(PPWrapper.attributionChangedDelegate);
                adjustConfig.setDeferredDeeplinkDelegate(PPWrapper.deferredDeeplinkCallback);
                Adjust.start(adjustConfig);
                var attribution = Adjust.getAttribution();
                logAdjustAttribution(attribution);
#else
                Debug.LogError($"Fx: Init, Attribution: {AttributionStatus}, Adjust not installed please check!");
#endif
            }
            else
            {
                Debug.Log("Fx: Init, double call to Init, remove one from AnalyticsManager");
            }
        }

#if ENABLE_ADJUST
        private static void deferredDeeplinkCallback(string i_DeeplinkURL)
        {
            Debug.Log($"Fx: Deeplink URL: {i_DeeplinkURL}");
        }

        public static void attributionChangedDelegate(AdjustAttribution i_Attribution)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"Fx: Adjust Attribution changed:");
                logAdjustAttribution(i_Attribution);

                AttributionStatus = i_Attribution.network == "Organic" ? AttributionStatus.Organic : AttributionStatus.UA;

                Managers.Instance.Analytics.LogGameAnalyticsEvent("Attribution", AttributionStatus.ToString());

                Debug.Log($"Fx: Changed Attribution Status To: {AttributionStatus}");

                attributedInstallInvoker();
            });
        }

        private static void logAdjustAttribution(AdjustAttribution i_Attribution)
        {
            if (i_Attribution != null)
            {
                Debug.Log($"Fx: Adid: '{i_Attribution.adid}' Network: '{i_Attribution.network}' AdGroup: '{i_Attribution.adgroup}' Campaign: '{i_Attribution.campaign}' Creative: '{i_Attribution.creative}' ClickLabel: '{i_Attribution.clickLabel}' TrackerName: '{i_Attribution.trackerName}' TrackerToken: '{i_Attribution.trackerToken}'\n");
            }
            else
            {
                Debug.Log($"Fx: {Utils.GetFuncName()} - i_Attribution is null");
            }
        }

        public static bool m_AttributedInstallInvoked = false;
        public static void attributedInstallInvoker()
        {
            if (!m_AttributedInstallInvoked)
            {
                if (AttributionStatus == AttributionStatus.UA)
                {
                    m_AttributedInstallInvoked = true;

                    Debug.Log($"Fx: Attributed Install Invoked");

                    FxInitSuccess.InvokeSafe(new FxConfig());
                }
            }
        }
#endif

        public static void WidthrawCash(int i_CashAmount)
        {
        }

        public static void AddCash(int i_CashAmount)
        {
            if (IsWork)
            {
                if (m_PPResult != null && m_PPResult.IsFree)
                {
                    FreePPBalance--;
                    m_PPResult.IsFree = false;
                }

                CashBalance += i_CashAmount;

                OnBalanceChange.Invoke();
            }
        }

        public static void SubCash(int i_CashAmount)
        {
            if (IsWork)
            {
                CashBalance -= i_CashAmount;

                OnBalanceChange.Invoke();
            }
        }

        public static int GetCashBalance()
        { 
            return CashBalance;
        }

        
        public static int GetCashOutAmount()
        {
            return GameConfig.Instance.CashOut.CashReward.CashoutTargetAmount;
        }

        private static int m_CashCollectReward
        {
            get
            {
                return GameConfig.Instance.CashOut.CashReward.GetSimulatedRewardAmount(GetCashOutAmount() - GetCashBalance(),
                    StorageManager.Instance.CurrentLevel);
            }
        }

        private static bool IsCashCollectFree(PPEntries i_PPEntry)
        {
            if (FreePPBalance > 0 && GameConfig.Instance.CashOut.CashRewardFreePP.FreePPEntries.Contains(i_PPEntry))
            {
                return true;
            }

            return false;
        }

        public static PPResult GetCashCollectValue(PPEntries i_PPEntry)
        {
            m_PPResult.CashCollectAmount = m_CashCollectReward;
            m_PPResult.IsFree = IsCashCollectFree(i_PPEntry);

            Debug.Log($"Fx: GetCashCollectValue with Entry: {i_PPEntry} Value: {m_PPResult.CashCollectAmount} IsFree: {m_PPResult.IsFree}");

            return m_PPResult;
        }
    }

    public static class PPExtension
    {
        /// <summary>
        /// If >10 will format as "00.00"
        /// If <10 will format as "0.00"
        /// </summary>
        /// <param name="i_CashAmount"></param>
        /// <returns></returns>
        public static string CashFormattedString(this int i_CashAmount)
        {
            int dollar = i_CashAmount / 100;
            int cent = i_CashAmount % 100;
            string result = "$" + dollar + "." + cent.ToString("00");
            return result;
        }

        public static string CashFormattedStringWithoutDollar(this int i_CashAmount)
        {
            int dollar = i_CashAmount / 100;
            int cent = i_CashAmount % 100;
            string result = dollar + "." + cent.ToString("00");
            return result;
        }

        public static string CashNoCentsFormattedString(this int i_CashAmount)
        {
            int dollar = i_CashAmount / 100;
            string result = "$" + dollar;
            return result;
        }
    }

    public class PPResult
    {
        public int CashCollectAmount = -1;
        public bool IsFree = false;

        public PPResult(int i_CashCollectAmount = -1, bool i_IsFree = false)
        {
            this.CashCollectAmount = i_CashCollectAmount;
            this.IsFree = i_IsFree;
        }
    }
}
#endif
