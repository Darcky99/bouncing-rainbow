using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [ExecutionOrder(eExecutionOrder.StorageManager)]
    public class StorageManagerBase : Singleton<StorageManager>
    {
        [PropertyOrder(-1), ShowInInspector] public int DefaultStartlevel = 1;

        [TitleGroup("Persistent Values", "General")]
        [ShowInInspector, PropertyOrder(0)] public int GameLaunchCount { get { return PlayerPrefs.GetInt(nameof(GameLaunchCount), 0); } set { PlayerPrefs.SetInt(nameof(GameLaunchCount), value); } }
        [ShowInInspector, PropertyOrder(0)] public bool IsGDPRConsentGiven { get { return PlayerPrefs.GetInt(nameof(IsGDPRConsentGiven), 0) == 1; } set { PlayerPrefs.SetInt(nameof(IsGDPRConsentGiven), value == true ? 1 : 0); } }
        [ShowInInspector, PropertyOrder(0), ReadOnly] public string InstallationID { get { return PlayerPrefs.GetString(nameof(InstallationID), ""); } set { PlayerPrefs.SetString(nameof(InstallationID), value); } }

        [Title("Gameplay")]
        [ShowInInspector, PropertyOrder(0)] public int CurrentLevelAttempts { get { return PlayerPrefs.GetInt(nameof(CurrentLevelAttempts), 0); } protected set { PlayerPrefs.SetInt(nameof(CurrentLevelAttempts), value); } }
        [ShowInInspector, PropertyOrder(0)] public int TotalAttempts { get { return PlayerPrefs.GetInt(nameof(TotalAttempts), 0); } private set { PlayerPrefs.SetInt(nameof(TotalAttempts), value); } }
        [ShowInInspector, PropertyOrder(0)] public int TotalGameWins { get { return PlayerPrefs.GetInt(nameof(TotalGameWins), 0); } private set { PlayerPrefs.SetInt(nameof(TotalGameWins), value); } }
        [ShowInInspector, PropertyOrder(0)] public int HighScore { get { return PlayerPrefs.GetInt(nameof(HighScore), 0); } private set { PlayerPrefs.SetInt(nameof(HighScore), value); } }
        [ShowInInspector, PropertyOrder(0)] public int HighScoreLevel { get { return PlayerPrefs.GetInt(nameof(HighScoreLevel), DefaultStartlevel); } protected set { PlayerPrefs.SetInt(nameof(HighScoreLevel), value); } }

        [Title("Currency")]
        [ShowInInspector, PropertyOrder(0)]
        public int CoinsAmount
        {
            get
            {
                return GameConfig.Instance.HUD.IsUseCollectables
                    ? GetCollectable(Collectable.eCollectableType.Coin)
                    : PlayerPrefs.GetInt(nameof(CoinsAmount), 0);
            }

            set
            {
                if (GameConfig.Instance.HUD.IsUseCollectables)
                {
                    SetCollectable(Collectable.eCollectableType.Coin, value);
                }
                else
                {
                    PlayerPrefs.SetInt(nameof(CoinsAmount), value);

                    OnCoinsAmountChanged?.Invoke(value);
                }
            }
        }

        public delegate void CoinsAmountChangedEvent(int i_CoinsAmount);
        public static event CoinsAmountChangedEvent OnCoinsAmountChanged = delegate { };

        [Title("Collectable")]
        public delegate void CollectableAmountChangedEvent(Collectable.eCollectableType i_CollectableType, int i_EarnAmount, int i_ResultAmount);
        public static event CollectableAmountChangedEvent OnCollectableAmountChanged = delegate { };
        public virtual void SetCollectable(Collectable.eCollectableType i_CollectableType, int i_Amount)
        {
            var earnAmount = i_Amount - GetCollectable(i_CollectableType);

            PlayerPrefs.SetInt(i_CollectableType.ToString(), i_Amount);

            OnCollectableAmountChanged?.Invoke(i_CollectableType, earnAmount, i_Amount);
        }
        public void AddCollectable(Collectable.eCollectableType i_CollectableType, int i_AddAmount)
        {
            SetCollectable(i_CollectableType, GetCollectable(i_CollectableType) + i_AddAmount);
        }
        public void SubtractCollectable(Collectable.eCollectableType i_CollectableType, int i_SubtractAmount)
        {
            SetCollectable(i_CollectableType, GetCollectable(i_CollectableType) - i_SubtractAmount);
        }

        public int GetCollectable(Collectable.eCollectableType i_CollectableType)
        {
            return PlayerPrefs.GetInt(i_CollectableType.ToString(),
                GameConfig.Instance.HUD.CollectableData.ContainsKey(i_CollectableType)
                ? GameConfig.Instance.HUD.CollectableData[i_CollectableType].InitialValue
                : -1);
        }

        // Sounds
        [Title("Sound")]
        [ShowInInspector, PropertyOrder(0)] public bool IsSFXMuted { get { return PlayerPrefs.GetInt(nameof(IsSFXMuted), 0) == 1; } set { PlayerPrefs.SetInt(nameof(IsSFXMuted), value == true ? 1 : 0); } }
        [ShowInInspector, PropertyOrder(0)] public bool IsMusicMuted { get { return PlayerPrefs.GetInt(nameof(IsMusicMuted), 0) == 1; } set { PlayerPrefs.SetInt(nameof(IsMusicMuted), value == true ? 1 : 0); } }
        [ShowInInspector, PropertyOrder(0)] public bool IsVibrationOn { get { return PlayerPrefs.GetInt(nameof(IsVibrationOn), 1) == 1; } set { PlayerPrefs.SetInt(nameof(IsVibrationOn), value == true ? 1 : 0); } }

        /// Ads               
        [Title("Ads")]
        [ShowInInspector, PropertyOrder(2)] public bool RemovedAds { get { return PlayerPrefsBool.GetBool(nameof(RemovedAds), false); } set { PlayerPrefsBool.SetBool(nameof(RemovedAds), value); } }
        [ShowInInspector, ReadOnly, PropertyOrder(3)] public int InterstitialCoolDownCounter = 0;
        [Title("- Videos:", bold: false, horizontalLine: false)]
        [ShowInInspector, PropertyOrder(3), Indent] public int TotalVideosWatched { get { return PlayerPrefs.GetInt(nameof(TotalVideosWatched), 0); } set { PlayerPrefs.SetInt(nameof(TotalVideosWatched), value); } }
        //[ShowInInspector, PropertyOrder(0), Indent] public int TotalVideosWatchedInDay { get { return PlayerPrefs.GetInt(nameof(TotalVideosWatchedInDay), 0); } set { PlayerPrefs.SetInt(nameof(TotalVideosWatchedInDay), value); } }
        //[ShowInInspector, PropertyOrder(0), Indent] public string LastRewardVideoTimeStampString { get { return PlayerPrefs.GetString(nameof(LastRewardVideoTimeStampString), k_UnSetEpochTimeString); } set { PlayerPrefs.SetString(nameof(LastRewardVideoTimeStampString), value); } }
        [Title("- Interstitials:", bold: false, horizontalLine: false)]
        [ShowInInspector, PropertyOrder(3), Indent] public int TotalInterstitialsWatched { get { return PlayerPrefs.GetInt(nameof(TotalInterstitialsWatched), 0); } set { PlayerPrefs.SetInt(nameof(TotalInterstitialsWatched), value); } }
        //[ShowInInspector, PropertyOrder(0), Indent] public int TotalInterstitialViewedInDay { get { return PlayerPrefs.GetInt(nameof(TotalInterstitialViewedInDay), 0); } set { PlayerPrefs.SetInt(nameof(TotalInterstitialViewedInDay), value); } }
        //[ShowInInspector, PropertyOrder(0), Indent] public string LastInterstitialTimeStampString { get { return PlayerPrefs.GetString(nameof(LastInterstitialTimeStampString), k_UnSetEpochTimeString); } set { PlayerPrefs.SetString(nameof(LastInterstitialTimeStampString), value); } }

        private int m_CurrentLevel = 0;
        [Title("Non-Persistent Values"), PropertyOrder(20), ShowInInspector]
        public virtual int CurrentLevel
        {
            get { return m_CurrentLevel; }
            set { if (value != m_CurrentLevel) this.CurrentLevelAttempts = 0; m_CurrentLevel = value; if (m_CurrentLevel > HighScoreLevel) HighScoreLevel = m_CurrentLevel; }
        }

        private int m_CurrentScore = 0;
        [PropertyOrder(20)]
        public int CurrentScore
        {
            get { return m_CurrentScore; }
            set { m_CurrentScore = value; if (m_CurrentScore > HighScore) HighScore = m_CurrentScore; }
        }

        private int m_GameOverSessionAttempts = 0;
        [PropertyOrder(20), ShowInInspector]
        public int GameOverSessionAttempts
        {
            get { return m_GameOverSessionAttempts; }
            set { if (value > m_GameOverSessionAttempts) { CurrentLevelAttempts++; TotalAttempts++; m_IsFirstAttempt = false; } m_GameOverSessionAttempts = value; }
        }

        private int m_GameWonSessionAttempts = 0;
        [PropertyOrder(20), ShowInInspector]
        public int GameWonSessionAttempts
        {
            get { return m_GameWonSessionAttempts; }
            set { if (value > m_GameWonSessionAttempts) { m_IsFirstAttempt = false; TotalGameWins++; } m_GameWonSessionAttempts = value; }
        }

        private bool m_IsFirstAttempt = true;
        [PropertyOrder(-1), ShowInInspector]
        public bool IsFirstAttempt
        {
            get { return m_IsFirstAttempt; }
        }

        [PropertyOrder(-1), ShowInInspector]
        public bool IsFirstLaunch
        {
            get { return GameLaunchCount <= 1; }
        }

        [PropertyOrder(-1), ShowInInspector]
        public string DeviceID
        {
            get { return SystemInfo.deviceUniqueIdentifier; }
        }

        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            GameLaunchCount++;

            if (InstallationID == "")
            {
                //Generate installation id on the first launch
                InstallationID = Guid.NewGuid().ToString();
            }

            //if (DateInstalled == k_UnSet)
            //{
            //    DateInstalled = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //}


            //if ((DateTime.Now - LastRewardVideoTimeStamp).TotalDays >= 1)
            //{
            //    Debug.Log($"TotalVideosWatchedInDay: Different Day! Total Videos watched in previous day: {TotalVideosWatchedInDay} resetting to: 0");

            //    LastRewardVideoTimeStamp = k_EpochDateTime;
            //    TotalVideosWatchedInDay = 0;
            //}


            //if ((DateTime.Now - LastInterstitialTimeStamp).TotalDays >= 1)
            //{
            //    Debug.Log($"TotalInterstitialViewedInDay: Different Day! Total Interstitial viewed in previous day: {TotalInterstitialViewedInDay} resetting to: 0");

            //    LastInterstitialTimeStamp = k_EpochDateTime;
            //    TotalInterstitialViewedInDay = 0;
            //}
        }

        private float m_Timer = 0.0f;
        private void Update()
        {
            if (InterstitialCoolDownCounter > 0)
            {
                m_Timer += Time.deltaTime;
                if (m_Timer > 1)
                {
                    m_Timer = 0;
                    InterstitialCoolDownCounter--;
                }
            }
        }

        [Button, PropertyOrder(-20)]
        public void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [Button, PropertyOrder(-20)]
        public void OpenPersistentPath()
        {
            Application.OpenURL("file://" + Application.persistentDataPath);
        }
    }
}