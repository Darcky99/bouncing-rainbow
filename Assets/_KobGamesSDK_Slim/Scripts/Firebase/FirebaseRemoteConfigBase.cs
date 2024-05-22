using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
#if ENABLE_FIREBASE
using Firebase.RemoteConfig;
using Firebase.Extensions;
#endif

namespace KobGamesSDKSlim
{
    [Serializable]
    public class FirebaseRemoteConfigBase
    {
        [TitleGroup("RemoteConfig Settings")]
        [ShowInInspector, PropertyOrder(-3)] public int FetchTimeSpanHours = 6;
        [ReadOnly, InfoBox("This will force the deletion of playerprefs for all of remoteConfig variables causing the default setting to be reset on device. Should increase this version by 1 everytime you want the remoteConfig variables to be flushed on the device. \n(being set automatically by GameSettings)"), PropertyOrder(-3)]
        public string FlushDeviceConfig = "1";

        [TitleGroup("RemoteConfig Base Variables")]
        [InlineProperty] public FirebaseStringValue ConfigVersion = new FirebaseStringValue(nameof(ConfigVersion), "0.1");
        [InlineProperty] public FirebaseIntValue MaxContinueAmount = new FirebaseIntValue(nameof(MaxContinueAmount), 1);
        [InlineProperty] public FirebaseStringValue ABTestVersion = new FirebaseStringValue(nameof(ABTestVersion), "");
        
        // Ads Variables
        [TitleGroup("Ads")]
        [InlineProperty] public FirebaseBoolValue IsAdsEnabled = new FirebaseBoolValue(nameof(IsAdsEnabled), true);
        [InlineProperty] public FirebaseBoolValue IsBannerEnabled = new FirebaseBoolValue(nameof(IsBannerEnabled), false);
        [InlineProperty] public FirebaseBoolValue IsInterstitialEnabled = new FirebaseBoolValue(nameof(IsInterstitialEnabled), false);
        [InlineProperty] public FirebaseBoolValue IsRewardVideosEnabled = new FirebaseBoolValue(nameof(IsRewardVideosEnabled), true);
        [InlineProperty] public FirebaseBoolValue InterstitalSessionStartEnabled = new FirebaseBoolValue(nameof(InterstitalSessionStartEnabled), false);
        [InlineProperty] public FirebaseIntValue InterstitialLevelCompleteCounter = new FirebaseIntValue(nameof(InterstitialLevelCompleteCounter), 0);
        [InlineProperty] public FirebaseIntValue InterstitialLevelFailedCounter = new FirebaseIntValue(nameof(InterstitialLevelFailedCounter), 0);
        [Tooltip("The Level in which Interstitials will start showing.\nExample: Value==2 will show interstitials on level 2 and beyond.")]
        [InlineProperty] public FirebaseIntValue InterstitialMinLevel = new FirebaseIntValue(nameof(InterstitialMinLevel), 0);
        [InfoBox("If 0 then it means CoolDowns are disabled, will always return true.", visibleIfMemberName: nameof(IsInterstitialOrRewardVideoDisabled))]
        [InlineProperty, PropertyTooltip("Relevant to interstitials only")] public FirebaseIntValue RewardVideoCoolDown = new FirebaseIntValue(nameof(RewardVideoCoolDown), 0);
        [InlineProperty, PropertyTooltip("Relevant to interstitials only")] public FirebaseIntValue InterstitialCoolDown = new FirebaseIntValue(nameof(InterstitialCoolDown), 0);
        [InlineProperty] public FirebaseBoolValue IsTestSuiteEnabled = new FirebaseBoolValue(nameof(IsTestSuiteEnabled), false);

        private bool IsInterstitialOrRewardVideoDisabled { get { return RewardVideoCoolDown.Value <= 0 || InterstitialCoolDown.Value <= 0; } }
        public bool IsInterstitialTimeBased { get { return RewardVideoCoolDown.Value > 0 || InterstitialCoolDown.Value > 0; } }

        [TitleGroup("Rate")]
        [InlineProperty] public FirebaseBoolValue IsRateUsEnabled = new FirebaseBoolValue(nameof(IsRateUsEnabled), true);


        [NonSerialized]
        protected Dictionary<string, object> ConfigDefaults = new Dictionary<string, object>() { };


        // This is called on Awake from RemoteSettingsManager.cs, for things that are not init on this stage but needs to be
        public virtual void Awake()
        {
            SetDefaults();
            flushDeviceConfigIfNeeded();
            PrintRemoteConfigValues();

#if ENABLE_FIREBASE
            
#endif
        }  

        public virtual void SetDefaults()
        {
            //// These are the values that are used if we haven't fetched data from the
            //// service yet, or if we ask for values that the service doesn't have:        
            ConfigDefaults.Add(ConfigVersion.Key, ConfigVersion.Value);
            ConfigDefaults.Add(MaxContinueAmount.Key, MaxContinueAmount.Value);
            ConfigDefaults.Add(ABTestVersion.Key, ABTestVersion.Value);

            ConfigDefaults.Add(IsAdsEnabled.Key, IsAdsEnabled.Value);
            ConfigDefaults.Add(IsBannerEnabled.Key, IsBannerEnabled.Value);
            ConfigDefaults.Add(IsInterstitialEnabled.Key, IsInterstitialEnabled.Value);
            ConfigDefaults.Add(InterstitalSessionStartEnabled.Key, InterstitalSessionStartEnabled.Value);
            ConfigDefaults.Add(InterstitialLevelCompleteCounter.Key, InterstitialLevelCompleteCounter.Value);
            ConfigDefaults.Add(InterstitialLevelFailedCounter.Key, InterstitialLevelFailedCounter.Value);
            ConfigDefaults.Add(InterstitialMinLevel.Key, InterstitialMinLevel.Value);
            ConfigDefaults.Add(IsRewardVideosEnabled.Key, IsRewardVideosEnabled.Value);
            ConfigDefaults.Add(RewardVideoCoolDown.Key, RewardVideoCoolDown.Value);
            ConfigDefaults.Add(InterstitialCoolDown.Key, InterstitialCoolDown.Value);
            ConfigDefaults.Add(IsTestSuiteEnabled.Key, IsTestSuiteEnabled.Value);

            ConfigDefaults.Add(IsRateUsEnabled.Key, IsRateUsEnabled.Value);
        }

        public virtual void OnFirebaseInitialized()
        {
#if ENABLE_FIREBASE
            Debug.Log("FirebaseRemoteConfig: Setting Firebase ConfigDefaults");

            Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(ConfigDefaults);
#endif
        }

        protected virtual void RefrectPropertiesSafe()
        {
#if ENABLE_FIREBASE
            try
            {
                ConfigVersion.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(ConfigVersion.Key).StringValue;
                MaxContinueAmount.Value = GetIntSafe(Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(MaxContinueAmount.Key), MaxContinueAmount.Value);
                ABTestVersion.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(ABTestVersion.Key).StringValue;
                
                IsAdsEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(IsAdsEnabled.Key).BooleanValue;
                IsBannerEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(IsBannerEnabled.Key).BooleanValue;
                IsInterstitialEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(IsInterstitialEnabled.Key).BooleanValue;
                InterstitalSessionStartEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(InterstitalSessionStartEnabled.Key).BooleanValue;
                InterstitialLevelCompleteCounter.Value = GetIntSafe(Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(InterstitialLevelCompleteCounter.Key), InterstitialLevelCompleteCounter.Value);
                InterstitialLevelFailedCounter.Value = GetIntSafe(Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(InterstitialLevelFailedCounter.Key), InterstitialLevelFailedCounter.Value);
                InterstitialMinLevel.Value = GetIntSafe(Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(InterstitialMinLevel.Key), InterstitialMinLevel.Value);
                IsRewardVideosEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(IsRewardVideosEnabled.Key).BooleanValue;
                RewardVideoCoolDown.Value = GetIntSafe(Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(RewardVideoCoolDown.Key), RewardVideoCoolDown.Value);
                InterstitialCoolDown.Value = GetIntSafe(Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(InterstitialCoolDown.Key), InterstitialCoolDown.Value);
                IsTestSuiteEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(IsTestSuiteEnabled.Key).BooleanValue;
                IsRateUsEnabled.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(IsRateUsEnabled.Key).BooleanValue;

                PrintRemoteConfigValues();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(FirebaseRemoteConfigBase)}-{Utils.GetFuncName()}(), Exception: {ex.Message} | Stack: {ex.StackTrace}");
            }
#endif
        }

        private void flushDeviceConfigIfNeeded()
        {
            // This is used to remove firebase remote variables from playerprefs on device
            // in case we wish to flush the variables to their default state
            if (PlayerPrefs.GetString(nameof(FlushDeviceConfig), "-1") != FlushDeviceConfig)
            {
                Debug.Log($"FirebaseRemoteConfig: Flushing DeviceConfig with version: {FlushDeviceConfig}");

                PlayerPrefs.SetString(nameof(FlushDeviceConfig), FlushDeviceConfig);

                foreach (var conf in ConfigDefaults)
                {
                    PlayerPrefs.DeleteKey(conf.Key);
                }

                // Clearing the dic
                ConfigDefaults.Clear();

                // Calling again SetDefaults on both derived and base in order to set default values into the ConfigDefaults dic
                SetDefaults();

                return;
            }
            else
            {
                Debug.Log($"FirebaseRemoteConfig: No need to Flush DeviceConfig with version: {FlushDeviceConfig}");
            }
        }

        private void PrintRemoteConfigValues()
        {
            string msg = $"Firebase-RemoteConfig-DefaultValues (Vars: {ConfigDefaults.Count}):\n";
            foreach (var config in ConfigDefaults)
            {
                msg += $"{config.Key} = {config.Value}\n";
            }

            Debug.Log(msg);
        }

#if ENABLE_FIREBASE
        private int m_DummyInt;
        protected int GetIntSafe(ConfigValue i_ConfigValue, int i_DefaultValue)
        {
            m_DummyInt = i_DefaultValue;

            try
            {
                if (Int32.TryParse(i_ConfigValue.StringValue, out m_DummyInt) == false)
                {
                    m_DummyInt = i_DefaultValue;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(FirebaseRemoteConfigBase)}-{Utils.GetFuncName()}(), Exception: {ex.Message} | Stack: {ex.StackTrace}");
            }

            return m_DummyInt;
        }
#endif

        public void Fetch(FirebaseRemoteConfigUpdatedEvent i_CompletionHandler = null)
        {
#if ENABLE_FIREBASE
            Debug.Log("Firebase-Fetch: Fetching data...");

            Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(new TimeSpan(FetchTimeSpanHours));

            fetchTask.ContinueWithOnMainThread(i_Task =>
            {
                if (i_Task.IsCanceled || i_Task.IsFaulted)
                {
                    // No need for this line, we got an invoke at the bottom, either way
                    //i_CompletionHandler?.Invoke(false);
                    Debug.LogError("Firebase-Fetch: Faild.");
                }
                else
                {
                    Debug.Log("Firebase-Fetch: Completed.");
                }

                var info = Firebase.RemoteConfig.FirebaseRemoteConfig.Info;
                Debug.Log(info.LastFetchStatus);
                switch (info.LastFetchStatus)
                {
                    case LastFetchStatus.Success:
                        Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();
                        Debug.Log("Firebase-Fetch: Success, Remote data loaded and ready.");
                        RefrectPropertiesSafe();
                        i_CompletionHandler?.Invoke(true);
                        Debug.LogFormat($"Firebase-Fetch: Last fetch time {info.FetchTime}).");
                        return;

                    case LastFetchStatus.Failure:
                        switch (info.LastFetchFailureReason)
                        {
                            case FetchFailureReason.Error:
                                Debug.LogError("Firebase-Fetch: Fetch failed for unknown reason");
                                break;
                            case FetchFailureReason.Throttled:
                                RefrectPropertiesSafe();
                                Debug.LogError("Firebase-Fetch: Fetch throttled until " + info.ThrottledEndTime);
                                break;
                        }
                        break;

                    case LastFetchStatus.Pending:
                        Debug.LogError("Firebase-Fetch: Latest Fetch call still pending.");
                        break;
                }

                i_CompletionHandler?.Invoke(false);
            });
#endif
        }

        [Serializable]
        public class FirebaseGenericValueType<T>
        {
            public virtual T Value { get; set; }

            [HideInInspector]
            public string Key;
            
            [HideLabel, LabelText("Default:"), LabelWidth(50), ShowInInspector, HorizontalGroup("1"), PropertyOrder(1), SerializeField]
            public T m_DefaultValue;

            public FirebaseGenericValueType(string i_Key, T i_DefaultValue)
            {
                this.Key = i_Key;
                this.m_DefaultValue = i_DefaultValue;
            }

            public void SetToDefaultValue(T i_DefaultValue, bool i_UpdateValue = false)
            {
                this.m_DefaultValue = i_DefaultValue;

                if (i_UpdateValue)
                {
                    this.Value = this.m_DefaultValue;
                }
            }
        }

        [Serializable]
        public class FirebaseStringValue : FirebaseGenericValueType<string>
        {
            [HideLabel, LabelText("Value:"), LabelWidth(50), ShowInInspector, HorizontalGroup("1", Width = 100), PropertyOrder(0)]
            public override string Value { get { return PlayerPrefs.GetString(Key, m_DefaultValue); } set { PlayerPrefs.SetString(Key, value); } }

            public FirebaseStringValue(string i_Key, string i_DefaultValue) : base(i_Key, i_DefaultValue) { }
        }

        [Serializable]
        public class FirebaseIntValue : FirebaseGenericValueType<int>
        {
            [HideLabel, LabelText("Value:"), LabelWidth(50), ShowInInspector, HorizontalGroup("1", Width = 100), PropertyOrder(0)]
            public override int Value { get { return PlayerPrefs.GetInt(Key, m_DefaultValue); } set { PlayerPrefs.SetInt(Key, value); } }// ValueFloat = (float)value; } }
            //[ShowInInspector] public float ValueFloat { get { return PlayerPrefs.GetFloat(Key, m_DefaultValue); } set { PlayerPrefs.SetFloat(Key, value); } }

            public FirebaseIntValue(string i_Key, int i_DefaultValue) : base(i_Key, i_DefaultValue) { }
        }

        [Serializable]
        public class FirebaseFloatValue : FirebaseGenericValueType<float>
        {
            [HideLabel, LabelText("Value:"), LabelWidth(50), ShowInInspector, HorizontalGroup("1", Width = 100), PropertyOrder(0)]
            public override float Value { get { return PlayerPrefs.GetFloat(Key, m_DefaultValue); } set { PlayerPrefs.SetFloat(Key, value); } }//ValueInt = (int)value; } }
            //[ShowInInspector] public int ValueInt { get { return PlayerPrefs.GetInt(Key, (int)m_DefaultValue); } set { PlayerPrefs.SetInt(Key, value); } }

            public FirebaseFloatValue(string i_Key, float i_DefaultValue) : base(i_Key, i_DefaultValue) { }
        }

        [Serializable]
        public class FirebaseBoolValue : FirebaseGenericValueType<bool>
        {
            [HideLabel, LabelText("Value:"), LabelWidth(50), ShowInInspector, HorizontalGroup("1", Width = 100), PropertyOrder(0)]
            public override bool Value { get { return PlayerPrefs.GetInt(Key, m_DefaultValue == true ? 1 : 0) == 1; } set { PlayerPrefs.SetInt(Key, value == true ? 1 : 0); } }

            public FirebaseBoolValue(string i_Key, bool i_DefaultValue) : base(i_Key, i_DefaultValue) { }
        }
    }
}
