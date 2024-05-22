#if ENABLE_BEPIC
using System;
using System.Collections.Generic;
using FxNS;
using SimpleSDKNS;
using UnityEngine;
using KobGamesSDKSlim;
using static FxNS.FxSdk;

namespace KobGamesSDKSlim.Bepic
{
    public partial class PPWrapper : FxUIInterface
    {
        public static OpenFxSuccess FxInitSuccess = (i_FxConfig) => { };
        public static OpenFxFail FxInitFail = (i_Message) => { };

        public static Action OnBalanceChange = () => { };

        public static PPWrapper Instace = new PPWrapper();

        private static FxConfig m_FxConfig;
        private static FxCallbackManager m_FxCallbackManager;
        private static FxAchievementManager m_FxAchievementManager;

        public static bool IsCashoutForcedEnabledOnDevice
        {
            get => !Application.isEditor && Debug.isDebugBuild && GameConfig.Instance.CashOut.ForceCashOutOnDevice;
        }

        public static bool IsCashoutEnabledInEditor
        {
            get => Application.isEditor && GameConfig.Instance.CashOut.IsCashOutEnabled;
        }

        public static bool IsWork
        {
            get
            {
                return m_FxConfig != null &&
                       !Application.isEditor && FxSdk.fxStatus == FxStatus.FX_OPEN ||
                       IsCashoutForcedEnabledOnDevice ||
                       IsCashoutEnabledInEditor;
            }
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

                Debug.Log("Fx: Init");

                FxSdk.SetCollectLogDelegate(delegate (string name, Dictionary<string, string> param)
                {
                    SimpleSDK.instance.Log(name, param);
                });

                FxSdk.SetFxUIInterface(Instace);
                SimpleSDK.instance.SetAttributionInfoListener(GetAttribution);
            }
            else
            {
                Debug.Log("Fx: Init, double call to Init, remove one from AnalyticsManager");
            }
        }

        public static void GetAttribution(AttributionInfo info)
        {
            Debug.Log("Fx: Got attribution correct");
            var initInfo = new InitInfo();
            var staticInfo = SimpleSDK.instance.GetStaticInfo();
            initInfo.gameId = staticInfo.gameName;
            initInfo.idfa = staticInfo.idfa;
            initInfo.gameVersion = "gameversion";

            initInfo.deviceId = staticInfo.deviceid;
            initInfo.channel = info.network;

            FxSdk.Init(initInfo, openFxSuccess, openFxFail);
            FxLanguage.SetLanguage(FX_LANGUAGE.en);
        }

        private static void openFxSuccess(FxConfig i_FxConfig)
        {
            Debug.Log($"Fx: is open success with config: {i_FxConfig.configVersion}");

            FxInitSuccess.Invoke(i_FxConfig);
        }

        private static void openFxFail(string i_Message)
        {
            if (Application.isEditor || IsCashoutForcedEnabledOnDevice)
            {
                // On editor force open Fx Success
                m_FxConfig = new FxConfig();
                openFxSuccess(m_FxConfig);
                return;
            }

            Debug.Log($"Fx: is open fail with msg: {i_Message}");

            FxInitFail.Invoke(i_Message);
        }

        // FxUIInterface Implementation
        public void CollectAdFinish(bool I_IsSuccess)
        {
            Debug.Log($"Fx: Collect Ad Finish with {I_IsSuccess}");
        }

        // FxUIInterface Implementation
        public void OpenFx(FxConfig i_Config, FxCallbackManager i_FxCallbackManager, FxAchievementManager i_FxAchievementManager)
        {
            Debug.Log($"Fx: OpenFx, Config: {i_Config != null} CallbackManager: {i_FxCallbackManager != null}");

            m_FxConfig = i_Config;
            m_FxCallbackManager = i_FxCallbackManager;
            m_FxAchievementManager = i_FxAchievementManager;
        }

        public static FxStatus GetFxStatus()
        {
            return FxSdk.fxStatus;
        }

        //TBD Koby
        public static void WidthrawCash(int i_CashAmountInCents)
        {
        }

        private static CanShowResult m_CanShowResultDummy = null;
        public static void AddCash(int i_CashAmountInCents, PPResult i_PPResult)
        {
            if (IsWork)
            {
                m_CanShowResultDummy = i_PPResult.CanShowResult;
                m_CanShowResultDummy.value = i_CashAmountInCents;

                i_PPResult.CashShowResultDummyCounter += m_CanShowResultDummy.value;

                //Debug.LogError($"Fx: CanShowResultStep: {m_CanShowResultDummy.value} AddCash: {i_PPResult.CashShowResultDummyCounter} / {i_PPResult.CashCollectAmount}");

                FxSdk.GetAchievementManager().AddCash(m_CanShowResultDummy);

                if (i_PPResult.CashShowResultDummyCounter == i_PPResult.CashCollectAmount)
                {
                    // Reach target amount - can add logic below
                }

                OnBalanceChange.Invoke();
            }
        }

        public static void AddCashForDebug(int i_CashAmountInCents)
        {
#if UNITY_EDITOR
            if (IsWork)
            {
                //Debug.LogError($"{m_CanShowResult.value} / {m_CanShowResultStep.value}");
                FxSdk.GetAchievementManager().AddAchievement(FxAchievementManager.CASH_AMOUNT, i_CashAmountInCents);
                FxSdk.GetAchievementManager().AddAchievement(FxAchievementManager.RAND_CASH_AMOUNT, i_CashAmountInCents);
                OnBalanceChange.Invoke();
            }
#endif
        }

        public static void SubCash(int i_CashAmountInCents)
        {
            if (IsWork)
            {
                FxSdk.GetAchievementManager().SubCash(i_CashAmountInCents);

                OnBalanceChange.Invoke();
            }
        }

        private static int m_CashBalance = 0;
        public static int GetCashBalance() // Results in cents
        {
            m_CashBalance = 0;
            if (IsWork)
            {
                FxAchievementManager fxAchievementManager = FxSdk.GetAchievementManager();
                if (fxAchievementManager != null)
                {
                    m_CashBalance = fxAchievementManager.GetCash();
                }

            }

            return m_CashBalance;
        }

        // Fx Items
        public const int k_FxCashOutTargetItem = 0;

        // Target cashout amount for player
        private const int k_CashOutTragetAmountDefault = 200 * 100; //In Cents, ex. 20,000 Cents = 200USD as default

        private static int m_CashOutTargetAmount = k_CashOutTragetAmountDefault;
        public static int GetCashOutAmount() // Results in cents
        {
            m_CashOutTargetAmount = GetCashOutAmountPerItem(k_FxCashOutTargetItem);

            return m_CashOutTargetAmount == 0 ? k_CashOutTragetAmountDefault : m_CashOutTargetAmount;
        }

        private static Dictionary<string, int> m_PrevFxItem = new Dictionary<string, int>();
        private static FxItem m_FxItem;

        private static int m_CashOutAmountTargetForFxItem = 0;
        public static int GetCashOutAmountPerItem(int i_Item = 0) // Results in cents
        {
            m_CashOutAmountTargetForFxItem = k_CashOutTragetAmountDefault;

            try
            {
                if (IsWork)
                {
                    if (m_FxConfig != null && m_FxConfig.GetFxItemsCount() > 0)
                    {
                        m_FxItem = m_FxConfig.GetFxItemByIndex(i_Item);
                        m_CashOutAmountTargetForFxItem = m_FxItem.value;

                        // Editor only - will allow to force FxItem value from GameConfig
                        if (Application.isEditor && GameConfig.Instance.CashOut.ForceFxItemValue != -1)
                        {
                            m_CashOutAmountTargetForFxItem = GameConfig.Instance.CashOut.ForceFxItemValue;
                        }
                        // --------------------------------------------------------------

                        // To avoid duplicate calls, we store values in dic and compare
                        if (m_PrevFxItem.AddIfNotExists(m_FxItem.name, m_CashOutAmountTargetForFxItem) ||
                            m_PrevFxItem[m_FxItem.name] != m_CashOutAmountTargetForFxItem)
                        {
                            Debug.Log($"Fx: Item: {m_FxItem.name} Value: {m_CashOutAmountTargetForFxItem} Fx Value: {m_FxItem.value} Fx PrevValue: {m_PrevFxItem[m_FxItem.name]}");
                            m_PrevFxItem[m_FxItem.name] = m_CashOutAmountTargetForFxItem;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Fx: GetCashOutAmount() Error: {ex.Message} Stack: {ex.StackTrace}");
            }

            return m_CashOutAmountTargetForFxItem;
        }
        
        //private static PPResult m_PPResult = new PPResult();

        public static PPResult GetCashCollectValue(PPEntries i_PPEntry)
        {
            return getCashCollectValue(i_PPEntry.ToString());
        }

        public static PPResult GetCashCollectValue(PPDailySign i_PPEntry)
        {
            return getCashCollectValue(i_PPEntry.ToString());
        }

        private static CanShowResult m_CanShowResult = null;
        private static PPResult getCashCollectValue(string i_PPEntry)
        {
            m_CanShowResult = null;

            FxSdk.ShowCollectWithEvent($"{i_PPEntry}");          

            //EDITOR ONLY or Device Debug Build ON - BEGIN
            if (Application.isEditor || IsCashoutForcedEnabledOnDevice) // editor or forced to work on device when debug build
            {
                if (GameConfig.Instance.CashOut.IsCashOutEnabled)
                {
                    if (GameConfig.Instance.CashOut.ForceCashCollectValue != -1)
                    {
                        Debug.Log($"Fx: Forcing call ShowCollectPanel");

                        var isDirect = Managers.Instance.Storage.CurrentLevel <= 2 ? PickCondition.direct : PickCondition.ad;
                        Instace.ShowCollectPanel(i_PPEntry, CanShowResult.Success(isDirect, CoinRuleType.Fixed, GameConfig.Instance.CashOut.ForceCashCollectValue));
                    }
                }
            }
            //EDITOR ONLY or Device Debug Build ON - END

            PPResult m_PPResult = new PPResult(i_PPEntry: i_PPEntry);

            if (m_CanShowResult != null)
            {
                m_PPResult.CanShowResult = m_CanShowResult;
                m_PPResult.CashCollectAmount = m_CanShowResult.value;
                m_PPResult.IsFree = m_CanShowResult.pickCondition == PickCondition.direct;
            }

            Debug.Log($"Fx: GetCashCollectValue with Entry: {i_PPEntry} Value: {m_PPResult.CashCollectAmount} IsFree: {m_PPResult.IsFree}");

            return m_PPResult;
        }

        // FxUIInterface Implementation
        public void ShowCollectPanel(string i_Name, CanShowResult i_Result)
        {
            Debug.Log($"Fx: ShowCollectPanel " + i_Name + " " + "canShow=" + i_Result.canShow + " reason=" + i_Result.reason + " pickCondition=" + i_Result.pickCondition + " value=" + i_Result.value + " coinRuleType=" + i_Result.coinRuleType);
            m_CanShowResult = i_Result;
        }

        public static void CollectCash(int i_CashAmount)
        {
            //FxSdk.CollectCash(i_CashAmount);
        }

        public static void LogLevelCompleted(int i_LevelID)
        {
            if (m_FxAchievementManager != null)
            {
                m_FxAchievementManager.SetLevel(i_LevelID);
            }
        }

        public static class TimeHelper
        {
            public static bool IsServerTimeAvailable => FxAchievementManager.ServerTime != -1;

            public static DateTime? GetServerTime()
            {
                if (IsServerTimeAvailable)
                {
                    return DateTime.FromBinary(FxAchievementManager.ServerTime);
                }

                return null;
            }
        }

        public static class DailySignIn
        {
            public static int LastActiveDaySignIn
            {
                get { return PlayerPrefs.GetInt(nameof(LastActiveDaySignIn), 0); }
                set { PlayerPrefs.SetInt(nameof(LastActiveDaySignIn), value); }
            }

            public static int GetActiveDaySignIn()
            {
                return PlayerPrefs.GetInt(FxAchievementManager.ACTIVE_DAY, 1);
            }

            public static void UpdateLastActiveDaySignIn()
            {
                LastActiveDaySignIn = GetActiveDaySignIn();
            }

            public static bool IsNewDaySignIn()
            {
                return LastActiveDaySignIn != GetActiveDaySignIn();
            }

#if UNITY_EDITOR
            public static void SimulateNewSignInDay()
            {
                PlayerPrefs.SetInt(FxAchievementManager.ACTIVE_DAY, GetActiveDaySignIn() + 1);
            }
#endif
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
        public string PPEntry = "None";
        public int CashShowResultDummyCounter = 0;
        public CanShowResult CanShowResult = CanShowResult.Fail("None");

        public PPResult(int i_CashCollectAmount = -1, bool i_IsFree = false, string i_PPEntry = "None")
        {
            this.CashCollectAmount = i_CashCollectAmount;
            this.IsFree = i_IsFree;
            this.PPEntry = i_PPEntry;
            this.CashShowResultDummyCounter = 0;
            this.CanShowResult = CanShowResult.Fail("None");
        }

        public void Reset(string i_PPEntry = "None")
        {
            this.CashCollectAmount = -1;
            this.IsFree = false;
            this.PPEntry = i_PPEntry;
            this.CashShowResultDummyCounter = 0;
            this.CanShowResult = CanShowResult.Fail("None");
        }
    }
}
#endif
