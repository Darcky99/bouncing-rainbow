using System;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace KobGamesSDKSlim.GameManagerV1
{
    [ExecutionOrder(eExecutionOrder.GameManager)]
    public class GameManagerBase : Singleton<GameManager>
    {
        [TitleGroup("Game Configuration - Base")]
        [ReadOnly, PropertyOrder(-5)] public eGameState GameState;
        [ReadOnly, ShowInInspector, PropertyOrder(-5)]
        public int MaxContinueAmount
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.MaxContinueAmount.Value; }
            set { Managers.Instance.RemoteSettings.FirebaseRemoteConfig.MaxContinueAmount.Value = value; }
        }
        [ReadOnly, PropertyOrder(-1)] public int CurrContinueAmount = 0;

        private Vector2Int m_OriginalRes;

        [Title("Events")]
        public delegate void GameEvent();
        public static event GameEvent OnGameReset = delegate { }; //Used when we want a clean slate on the entire project
        public static event GameEvent OnGamePause = delegate { };
        public static event GameEvent OnGameUnPause = delegate { };
        public static event GameEvent OnLevelReset = delegate { }; //Used when we just want to reset the current level

        public delegate void LevelEvent();
        public static event LevelEvent OnLevelContinue = delegate { };
        public static event LevelEvent OnLevelStarted = delegate { };
        public static event LevelEvent OnLevelCompleted = delegate { };
        public static event LevelEvent OnLevelLoaded = delegate { };
        public static event LevelEvent OnLevelFailed = delegate { };
        public static event LevelEvent OnLevelFailedNoContinue = delegate { };

        public delegate void OnScreenChangedEvent(int i_Width, int i_Height);
        public static event OnScreenChangedEvent OnScreenChanged = ((width, height) => { });

        [Obsolete]
        public void RaiseOnLevelCompleted() => OnLevelCompleted.Invoke();
        [Obsolete]
        public void RaiseOnLevelContinue() => OnLevelContinue.Invoke();

        [Title("Debug")]
        public bool ShowLogs = false;


        #region Unity Functions
        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            m_OriginalRes = new Vector2Int(Screen.width, Screen.height);
            
            SetDefaultFPS();

            //Making sure Debug stuff is correctly init
            GameConfig.Instance.Debug.InitOnGameManagerAwakeEvent();

            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.GameOverSessionAttempts = 0;
                StorageManager.Instance.GameWonSessionAttempts = 0;
            }

            Reset();

#if UNITY_EDITOR
            if (RenderSettings.fog)
            {
                if (RenderSettings.fogMode == FogMode.Linear)
                {
                    if (RenderSettings.fogEndDistance != Camera.main.farClipPlane)
                        Debug.LogWarning("MainCamera far distance should be the same as fog end for performance reasons");
                }
                else
                {
                    Debug.LogWarning("Consider using Linear Fog instead");
                }
            }
#endif
        }

        public void SetDefaultFPS()
        {
#if UNITY_WEBGL
            //Unity recommends to uncap fps for better performance
            //https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
            Application.targetFrameRate = -1;
#else
            Application.targetFrameRate = 60;
            #endif
        }

        public override void Start()
        {
            base.Start();

            LevelLoaded();
        }

        public virtual void OnEnable()
        {
            RemoteSettingsManager.OnFirebaseRemotConfigUpdated += OnFirebaseRemotConfigUpdated;

#if ENABLE_SHORTCUTS
            ShortcutsManager.Instance.SetGameManagerShortcutsEvents(SimulateLevelCompleted, SimulateLevelFailedWithRevive,
                SimulateLevelFailed, SimulatePrevLevel, SimulateNextLevel, SimulateResetLevel, SimulateResetGame);
#endif
        }

        public override void OnDisable()
        {
            base.OnDisable();

            RemoteSettingsManager.OnFirebaseRemotConfigUpdated -= OnFirebaseRemotConfigUpdated;
        }

        protected virtual void Update()
        {
            var currRes = new Vector2Int(Screen.width, Screen.height);

            if (currRes != m_OriginalRes)
            {
                m_OriginalRes = currRes;
                OnScreenChanged.Invoke(Screen.width, Screen.height);
            }

            
#if UNITY_EDITOR
            if (GameConfig.Instance.Debug.EmulateLowFPS)
            {
                Application.targetFrameRate = Random.Range(GameConfig.Instance.Debug.LowFPSValues.x, GameConfig.Instance.Debug.LowFPSValues.y);
            }
#endif
        }

        protected virtual void OnFirebaseRemotConfigUpdated(bool i_IsUpdated)
        {
        }
        #endregion

        public virtual void GameOver(bool i_ForceNoContinue = false)
        {
            if (CurrContinueAmount < MaxContinueAmount && !i_ForceNoContinue)
                LevelFailed();
            else
                LevelFailedNoContinue();
        }


#region Resets
        public virtual void ResetGame()
        {
            Reset();
            OnGameReset?.Invoke();

            if (ShowLogs)
                Debug.Log("GameManager_OnGame_Reset()");

            LevelLoaded();
        }

        public virtual void ResetLevel()
        {
            Reset();
            OnLevelReset?.Invoke();

            if (ShowLogs)
                Debug.Log($"GameManager_OnLevel_Reset() {StorageManager.Instance.CurrentLevel}");

            LevelLoaded();
        }

        private void Reset()
        {
            GameState = eGameState.Idle;
            CurrContinueAmount = 0;

            KillAllTweensWithExclusionsList();
            Time.timeScale = 1;
        }

        [Button]
        protected virtual void KillAllTweensWithExclusionsList()
        {
            DOTween.KillAll(false, DOTweenIDs.IdsToExcludeFromGameReset); 
        }
        #endregion

        #region Pause/UnPause
        public virtual void PauseGame()
        {
            if (GameState == eGameState.Playing)
                GameState = eGameState.Paused;

            OnGamePause?.Invoke();

            if (ShowLogs)
                Debug.Log("GameManager_PauseGame()");
        }

        public virtual void UnPauseGame()
        {
            if (GameState == eGameState.Paused)
                GameState = eGameState.Playing;

            OnGameUnPause?.Invoke();

            if (ShowLogs)
                Debug.Log("GameManager_UnPauseGame()");
        }
        #endregion

        #region LevelLoop
        protected virtual void LevelLoaded()
        {
#if ENABLE_ADS
            Managers.Instance.Ads.LevelLoaded();
#endif
            Managers.Instance.Analytics.LevelLoaded();

            if (OnLevelLoaded != null)
            {
                OnLevelLoaded.Invoke();
            }

            if (ShowLogs)
                Debug.Log($"GameManager_LevelLoaded() {StorageManager.Instance.CurrentLevel}");
        }

        public virtual void StartGame()
        {
            LevelStarted();
        }

        public virtual void LevelStarted()
        {
            GameState = eGameState.Playing;
            CurrContinueAmount = 0;

            Managers.Instance.Analytics.LevelStarted();

            if (OnLevelStarted != null)
            {
                OnLevelStarted.Invoke();
            }

            if (ShowLogs)
                Debug.Log($"GameManager_LevelStarted() {StorageManager.Instance.CurrentLevel}");
        }

        public virtual void LevelCompleted()
        {
            GameState = eGameState.Finished;

            Managers.Instance.Storage.GameWonSessionAttempts++;
            Managers.Instance.Analytics.LevelCompleted();
#if ENABLE_ADS
            Managers.Instance.Ads.LevelCompleted();
#endif

            if (OnLevelCompleted != null)
            {
                OnLevelCompleted.Invoke();
            }

            if (ShowLogs)
                Debug.Log($"GameManager_LevelCompleted() {StorageManager.Instance.CurrentLevel}");
        }

        public virtual void LevelContinue()
        {
            GameState = eGameState.Playing;

            CurrContinueAmount++;
            Managers.Instance.Analytics.LevelContinued();

            if (OnLevelContinue != null)
            {
                OnLevelContinue.Invoke();
            }

            if (ShowLogs)
                Debug.Log($"GameManager_OnContinueLevel() {StorageManager.Instance.CurrentLevel}");
        }

        protected virtual void LevelFailed() //This should be called by GameOver() only
        {
            GameState = eGameState.GamePreOver;

            if (OnLevelFailed != null)
            {
                OnLevelFailed.Invoke();
            }

            if (ShowLogs)
                Debug.Log($"GameManager_LevelFailed() {StorageManager.Instance.CurrentLevel}");
        }

        protected virtual void LevelFailedNoContinue() //This should be called by GameOver() only
        {
            GameState = eGameState.GameOver;

            Managers.Instance.Storage.GameOverSessionAttempts++;
            Managers.Instance.Analytics.LevelFailed();
#if ENABLE_ADS
            Managers.Instance.Ads.LevelFailed();
#endif

            if (OnLevelFailedNoContinue != null)
            {
                OnLevelFailedNoContinue.Invoke();
            }

            if (ShowLogs)
                Debug.Log($"GameManager_LevelFailedNoContinue() {StorageManager.Instance.CurrentLevel}");
        }
        #endregion

#region Simulate GameLoop

#if !ENABLE_SHORTCUTS

        public void SimulateGameLoop(eGameLoopDebugType i_Type)
        {
            //Debug Mode is only available with the new GameConfig. Old project might not have this var
            //If Unity Editor it should always go through. On mobile it will depend on DebugMode
#if !UNITY_EDITOR
            if (!GameConfig.Instance.Debug.DebugMode)
                return;
#endif

            switch (i_Type)
            {
                case eGameLoopDebugType.Complete:
                    LevelCompleted();
                    break;
                case eGameLoopDebugType.FailedWithRevive:
                    SimulateLevelFailedWithRevive();
                    break;
                case eGameLoopDebugType.Failed:
                    GameOver(true);
                    break;
                case eGameLoopDebugType.PrevLevel:
                    SimulatePrevLevel();
                    break;
                case eGameLoopDebugType.NextLevel:
                    SimulateNextLevel();
                    break;
                case eGameLoopDebugType.ResetLevel:
                    ResetLevel();
                    break;
                case eGameLoopDebugType.ResetGame:
                    ResetGame();
                    break;
                default:
                    break;
            }
        }
#endif
        protected virtual void SimulateLevelCompleted()
        {
            if (GameState != eGameState.Playing)
            {
                ResetGame();
                StartGame();
            }

            LevelCompleted();
        }

        protected virtual void SimulateLevelFailedWithRevive()
        {
            if(GameState != eGameState.Playing)
            {
                ResetGame();
                StartGame();
            }

            if (MaxContinueAmount == 0)
                MaxContinueAmount = 1;

            CurrContinueAmount = 0;

            GameOver();
        }

        protected virtual void SimulateLevelFailed()
        {
            if (GameState != eGameState.Playing)
            {
                ResetGame();
                StartGame();
            }

            GameOver(true);
        }

        protected virtual void SimulatePrevLevel()
        {
            AdsManager.Instance.SetShortcutKeyPress();
            StorageManager.Instance.CurrentLevel = Mathf.Max(StorageManager.Instance.CurrentLevel - 1, StorageManager.Instance.DefaultStartlevel);
            GameManager.Instance.ResetGame();
        }

        protected virtual void SimulateNextLevel()
        {
            AdsManager.Instance.SetShortcutKeyPress();
            StorageManager.Instance.CurrentLevel++;
            GameManager.Instance.ResetGame();
        }

        protected virtual void SimulateResetLevel()
        {
            AdsManager.Instance.SetShortcutKeyPress();
            if (GameState != eGameState.Playing)
            {
                ResetGame();
                StartGame();
            }

            ResetLevel();
        }
        protected virtual void SimulateResetGame()
        {
            AdsManager.Instance.SetShortcutKeyPress();
            ResetGame();
        }
#endregion


        //NOTE RUBEN - this should probably be implemented per project.
        //public delegate void LevelProgress(int i_Progress);
        //public LevelProgress OnLevelProgress;

        //public virtual void ReportProgress(int i_Progress, string i_EventTitle = "LevelProgress")
        //{
        //    Managers.Instance.Analytics.LogLevelProgress(
        //    Managers.Instance.Analytics.kv(nameof(Managers.Instance.Storage.CurrentLevel), Managers.Instance.Storage.CurrentLevel),
        //    Managers.Instance.Analytics.kv(i_EventTitle, i_Progress //hide point would go here
        //    ));

        //    if (OnLevelProgress != null)
        //    {
        //        OnLevelProgress.Invoke(i_Progress);
        //    }

        //    Debug.Log($"{nameof(ReportProgress)} Level: {Managers.Instance.Storage.CurrentLevel.ToString()} Progress: {i_Progress}");
        //}

        //NOTE RUBEN - This should be on iAP Manager
        //public delegate void InGameShopEvent(string EventName, int ItemName);
        //public InGameShopEvent OnGameShopEvent;

        //public virtual void ShopEvent(int i_ItemName, string i_ShopEvent = "ItemPurchased")
        //{
        //    Managers.Instance.Analytics.LogLevelProgress(
        //    Managers.Instance.Analytics.kv(nameof(Managers.Instance.Storage.CurrentLevel), Managers.Instance.Storage.CurrentLevel),
        //    Managers.Instance.Analytics.kv(i_ShopEvent, i_ItemName //hide point would go here
        //    ));

        //    if (OnGameShopEvent != null)
        //    {
        //        OnGameShopEvent.Invoke(i_ShopEvent, i_ItemName);
        //    }

        //    Debug.Log($"{nameof(ShopEvent)} {i_ShopEvent}: {i_ItemName}");
        //}
    }

}