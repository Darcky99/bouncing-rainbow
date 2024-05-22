using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using KobGamesSDKSlim.Debugging;

namespace KobGamesSDKSlim
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ManagersBase : Singleton<Managers>
    {
        //#if UNITY_EDITOR
        //        static ManagersBase()
        //        {
        //            EditorApplication.update += () =>
        //            {
        //                if (IsInstanceNull == false)
        //                {
        //                    Instance.Reset();
        //                }
        //            };
        //        }
        //#endif

        public bool IsHapticEnabled = false;

        public SplashScreenManager SplashScreen;
        public AdsManager Ads;
        public AnalyticsManager Analytics;
        public RemoteSettingsManager RemoteSettings;
        public StorageManager Storage;
        public GameManager GameManager;
        public iAPManager iAPManager;
        public HapticManager HapticManager;
        public InteractionManager InteractionManager;

#if ENABLE_WEBTOOLS
        public WebToolsManager WebTools;
#endif
#if ENABLE_FEEDBACK
        public FeedbackManager Feedback;
#endif

        [InfoBox("These dummy references are required so those ScriptableObjects will be initially loaded (since everything in Resources folder goes into the build either way.\n" +
            "if Unity dosen't see a reference to a ScriptableObject it will not load it automatically.\n" +
            "Link: https://baraujo.net/unity3d-making-singletons-from-scriptableobjects-automatically/")]
        public ScriptableObject[] DummyReferences;

        #region Managers Validations
        [Button]
        public virtual void Reset()
        {
            SplashScreen = SplashScreen ?? this.GetComponentInChildren<SplashScreenManager>();

            Ads = Ads ?? this.GetComponentInChildren<AdsManager>();

            Analytics = Analytics ?? this.GetComponentInChildren<AnalyticsManager>();

            RemoteSettings = RemoteSettings ?? this.GetComponentInChildren<RemoteSettingsManager>();

            Storage = Storage ?? this.GetComponentInChildren<StorageManager>();

            GameManager = GameManager ?? this.GetComponentInChildren<GameManager>();

            iAPManager = iAPManager ?? this.GetComponentInChildren<iAPManager>();

            HapticManager = HapticManager ?? this.GetComponentInChildren<HapticManager>();

            InteractionManager = InteractionManager ?? this.GetComponentInChildren<InteractionManager>();

#if ENABLE_WEBTOOLS
            WebTools = WebTools ?? this.GetComponentInChildren<WebToolsManager>();
#endif

#if ENABLE_FEEDBACK
            Feedback = Feedback ?? this.GetComponentInChildren<FeedbackManager>();
#endif
        }

#if UNITY_EDITOR
        [Button]
        public void CreatePrefabVariant()
        {
            string prefabName = "_Managers_V2";

            PrefabUtility.SaveAsPrefabAssetAndConnect(this.gameObject, $"Assets/_Project_Specific_Folder/Prefabs/{prefabName}.prefab", InteractionMode.AutomatedAction);

            Managers.Instance.gameObject.name = prefabName;
        }

        [Button]
        public void ProcessValidateManagers()
        {
            OnValidate();
        }

        public virtual void OnValidate()
        {
            // making sure this is run only on the instance that is on the scene and not on the prefab object
            if (!this.gameObject.scene.name.IsNullOrEmpty())
            {
                //Debug.LogError($"SUP {this.gameObject.name} {this.name} {this.gameObject.scene.name}");
                //return;

                Reset();

                //Debug.LogError("OnValidate() " + this.name + " " + this.gameObject.scene.name);

                //var tmp1 = PluginsVersion.Instance;
                var tmp2 = GameSettings.Instance;
                var tmp3 = GameConfig.Instance;

                DummyReferences = Resources.FindObjectsOfTypeAll<ScriptableObject>()
                   .Where(x => x.name.Contains("GameSettings") ||
                               x.name.Contains("Plugins") ||
                               x.name.Contains("GameConfig")).ToArray();

                if (!Application.isPlaying && !m_ValidateManagersWithDelayLock)
                {
                    m_ValidateManagersWithDelayLock = true;

                    m_ValidateManagersTimeFromStartup = Time.realtimeSinceStartup;
                    EditorApplication.update += ValidateManagersWithDelay;
                }
            }
        }

        [NonSerialized]
        private float m_ValidateManagersDelayCounter = .75f;
        private float m_ValidateManagersTimeFromStartup = 0;
        private static bool m_ValidateManagersWithDelayLock = false;
        protected virtual void ValidateManagersWithDelay()
        {
            if (m_ValidateManagersWithDelayLock && this != null)
            {
                //Debug.LogError((Time.realtimeSinceStartup - m_ValidateManagersTimeFromStartup) + " " + m_ValidateManagersDelayCounter);
                if ((Time.realtimeSinceStartup - m_ValidateManagersTimeFromStartup) >= m_ValidateManagersDelayCounter)
                {
                    EditorApplication.update -= ValidateManagersWithDelay;
                    //Debug.LogError("Calling ValidateManagers() " + this.name + " " + this.gameObject.scene.name);

                    ValidateManagers();

                    //Debug.LogError("Lock Off");
                    m_ValidateManagersWithDelayLock = false;
                }
            }
            else
            {
                EditorApplication.update -= ValidateManagersWithDelay;
            }
        }

        [Button]
        protected virtual void ValidateManagers()
        {
            if (!Application.isPlaying)
            {
                if (this.gameObject.scene.name == SceneManager.GetActiveScene().name)
                {
                    base.DontDestroyLoad = true;

                    var nullFields = this.GetType().GetFields()
                                                            .Where(field => field.GetValue(this) == null && field.Name != nameof(DummyReferences) && field.Name != nameof(Prefab))
                                                            .Select(field => field.Name).ToList();

                    if (GameSettings.Instance == null)
                    {
                        Debug.LogError("## ERROR ## GameSettings couldn't be found, check the ScriptableObject path");
                    }


                    if (nullFields.Count > 0)
                    {
                        Debug.LogError("## Managers null fields found: ");
                        foreach (var nullField in nullFields)
                        {
                            Debug.LogError(nullField);
                        }
                    }
                    else
                    {
                        bool isPrefabNeedsTobeApplied = PrefabUtility.HasPrefabInstanceAnyOverrides(PrefabUtility.GetNearestPrefabInstanceRoot(this.gameObject), false);

                        //Debug.LogError($"Prefab needs to be applied: {isPrefabNeedsTobeApplied}");

                        if (isPrefabNeedsTobeApplied)
                        {
                            bool userChoiceApplyPrefab = EditorUtility.DisplayDialog("Managers Prefab",
                                $"Managers Prefab needs to be applied.\n\n " +
                                $"isPrefabNeedsTobeApplied: {isPrefabNeedsTobeApplied}" +
                                "\n\n" +
                                $"Apply Now?"
                                , "Yes", "No");

                            //Debug.LogError($"Chosen: {userChoiceApplyPrefab}");

                            if (userChoiceApplyPrefab)
                            {
                                Debug.LogError("Automatically applied Prefab: " + this.name);
                                this.gameObject.ApplyToPrefab();
                            }
                        }

                        //if (PrefabUtility.HasPrefabInstanceAnyOverrides(PrefabUtility.GetNearestPrefabInstanceRoot(this.gameObject), false))
                        //{
                        //    if (!m_DummyOverrides.SequenceEqual(getObjectOverrides()))
                        //    {
                        //        setDummyOverrides();

                        //        Debug.LogError("Automatically applied Prefab: " + this.name);
                        //        this.gameObject.ApplyToPrefab();
                        //    }
                        //    //else
                        //    //{
                        //    //    Debug.LogError("Eq seq");
                        //    //}
                        //}
                    }
                }
            }
        }

        //[ShowInInspector]
        private string[] m_DummyOverrides = { "None" };

        private void setDummyOverrides()
        {
            m_DummyOverrides = getObjectOverrides();
        }

        //[ShowInInspector]
        private string[] getObjectOverrides()
        {
            return PrefabUtility.GetObjectOverrides(PrefabUtility.GetNearestPrefabInstanceRoot(this.gameObject), false).Select(x => x.instanceObject.name).ToArray();
        }

        //[Button]
        //public void PrintOverridesPrefab()
        //{
        //    foreach (var x in getObjectOverrides())
        //    {
        //        Debug.LogError(x);
        //    }
        //}

        //[Button]
        //public void PrintOverridesVar()
        //{
        //    foreach (var y in m_DummyOverrides)
        //    {
        //        Debug.LogError(y);
        //    }
        //}

        //[Button]
        //public void SetOverride()
        //{
        //    m_DummyOverrides = PrefabUtility.GetObjectOverrides(PrefabUtility.GetNearestPrefabInstanceRoot(this.gameObject), false).Select(x=>x.instanceObject.name).ToArray();
        //}
#endif
        #endregion

        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            //Debug.LogError("KOBY TESTER: " + GameSettings.Instance);

#if UNITY_EDITOR
            ValidateManagers();
#endif

#if ENABLE_LOGS
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
        }

        [Button]
        public void ReloadScene()
        {
#if UNITY_EDITOR
            EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
#endif
        }

        [Button]
        public void SelectGameSettings()
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeObject = GameSettings.Instance;
#endif
        }


        #region Simulate Loop
        [Title("Simulate Loop"), Button, PropertyOrder(10)]
        public void SimulateLevelCompleted()
        {
#if ENABLE_SHORTCUTS
            ShortcutsManager.Instance.TriggerShortcut(ShortcutsActions.LevelComplete);
#else
            GameManager.Instance.SimulateGameLoop(eGameLoopDebugType.Complete);
#endif
        }

        [Button, PropertyOrder(10)]
        public void SimulateLevelFailedWithRevive()
        {
#if ENABLE_SHORTCUTS
            ShortcutsManager.Instance.TriggerShortcut(ShortcutsActions.LevelFailedWithRevive);
#else
            GameManager.Instance.SimulateGameLoop(eGameLoopDebugType.FailedWithRevive);
#endif
        }

        [Button, PropertyOrder(10)]
        public void SimulateLevelFailed()
        {
#if ENABLE_SHORTCUTS
            ShortcutsManager.Instance.TriggerShortcut(ShortcutsActions.LevelFailed);
#else
            GameManager.Instance.SimulateGameLoop(eGameLoopDebugType.Failed);
#endif
        }
#endregion


        [Button, PropertyOrder(5)]
        public void AddAppMetricaToManagers()
        {
#if UNITY_EDITOR && ENABLE_APPMETRICA
            if (this.GetComponentInChildren<AppMetrica>() == null)
            {
                GameObject newGO = new GameObject("AppMetrica");
                newGO.AddComponent<AppMetrica>();
                newGO.transform.SetParent(this.gameObject.transform);

                EditorUtility.SetDirty(this.gameObject);

                this.gameObject.ApplyToPrefab();
            }
#endif
        }

        [Button, PropertyOrder(5)]
        public void RemoveAppMetricaFromManagers()
        {
#if UNITY_EDITOR
            GameObject appMetricaGameObject = this.gameObject.FindObject<GameObject>("AppMetrica");
            if (appMetricaGameObject != null)
            {
                this.gameObject.RemovePrefab(appMetricaGameObject);
            }
#endif
        }

        [Button, PropertyOrder(5)]
        public void AddBepicToManagers()
        {
#if UNITY_EDITOR && ENABLE_BEPIC
            if (this.GetComponentInChildren<FxNS.FxSdk>() == null)
            {
                GameObject newGO = new GameObject("FxSDK");
                newGO.AddComponent<FxNS.FxSdk>();
                newGO.transform.SetParent(this.gameObject.transform);

                EditorUtility.SetDirty(this.gameObject);

                this.gameObject.ApplyToPrefab();
            }

            if (this.GetComponentInChildren<SimpleSDKNS.SimpleSDK>() == null)
            {
                GameObject newGO = new GameObject("SimpleSDK");
                newGO.AddComponent<SimpleSDKNS.SimpleSDK>();
                newGO.transform.SetParent(this.gameObject.transform);

                EditorUtility.SetDirty(this.gameObject);

                this.gameObject.ApplyToPrefab();

                GameSettings.Instance.General.DeleteBepicUneededFiles();
            }
#endif
        }

        [Button, PropertyOrder(5)]
        public void RemoveBepicFromManagers()
        {
#if UNITY_EDITOR
            GameObject FxSDK = this.gameObject.FindObject<GameObject>("FxSDK");
            GameObject SimpleSDK = this.gameObject.FindObject<GameObject>("SimpleSDK");

            if (FxSDK != null)
            {
                this.gameObject.RemovePrefab(FxSDK);
            }

            if (SimpleSDK != null)
            {
                this.gameObject.RemovePrefab(SimpleSDK);
            }
#endif
        }

        public void ApplyPrefabInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                this.gameObject.ApplyToPrefab();
                //PrefabHelper.ApplyPrefab(this.gameObject);
            }
#endif
        }

        [PropertyOrder(6)]
        public GameObject Prefab;
        [Button, PropertyOrder(6)]
        public void AddPrefabToManagers()
        {
#if UNITY_EDITOR
            this.gameObject.AddPrefab(Prefab);
#endif
        }

        [Button]
        public void FixAdsManagerV2()
        {
#if UNITY_EDITOR
            //Debug.LogError($"Scene: {Managers.Instance.gameObject.scene}");

            GameObject AdsManagerGO = Managers.Instance.gameObject.FindObject<GameObject>("AdsManager");

            if (AdsManagerGO != null)
            {
                Debug.LogError($"AdsManager Legacy Found, AdsManager GameObject: {AdsManagerGO}, Fixing...");

                UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(AdsManagerGO);

                var rootPrefabPath = Managers.Instance.gameObject.UnpackPrefabInstanceNearestAndReturnPath();

                if (AdsManagerGO.GetComponent<AdsManager>() == null)
                    UnityEditorInternal.ComponentUtility.MoveComponentUp(AdsManagerGO.AddComponent<AdsManager>());

                Managers.Instance.gameObject.ApplyToPrefab(rootPrefabPath);

                GameSettings.Instance.Sync.CopyAdsManagerProjectLayerFile();
            }
#endif
        }

#region DebugKeysInEditor_S_Key_Pause/UnPause

#if UNITY_EDITOR
        private int m_PauseCounter = 10;
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.S) && !UnityEditor.EditorApplication.isPaused && m_PauseCounter <= 0)
            {
                //Time.timeScale = 0;
                //DOTween.timeScale = 0;

                UnityEditor.EditorApplication.isPaused = true;
                UnityEditor.EditorApplication.modifierKeysChanged += waitForUnPausedByKey;
                UnityEditor.EditorApplication.update += waitForUnPausedEditor;

                m_PauseCounter = 10;
            }

            m_PauseCounter--;
        }

        private void waitForUnPausedByKey()
        {
            //Debug.Log(Input.inputString.ToLower() + " : " + Input.GetKey(KeyCode.S));
            //Debug.Log("KEY: " + Input.inputString);
            if (UnityEditor.EditorApplication.isPaused)
            {
                if (Input.inputString.ToLower().Contains("s") || Input.inputString.ToLower().Contains("ד") || Input.GetKeyDown(KeyCode.S))
                {
                    //Time.timeScale = 1;
                    //DOTween.timeScale = 1;

                    UnityEditor.EditorApplication.isPaused = false;

                    UnityEditor.EditorApplication.modifierKeysChanged -= waitForUnPausedByKey;
                    UnityEditor.EditorApplication.update -= waitForUnPausedEditor;
                }
            }
        }

        private void waitForUnPausedEditor()
        {
            if (UnityEditor.EditorApplication.isPaused)
            {
                if (Input.inputString.ToLower().Contains("s") || Input.inputString.ToLower().Contains("ד") || Input.GetKeyDown(KeyCode.S))
                {
                    //Time.timeScale = 1;
                    //DOTween.timeScale = 1;

                    UnityEditor.EditorApplication.isPaused = false;

                    UnityEditor.EditorApplication.modifierKeysChanged -= waitForUnPausedByKey;
                    UnityEditor.EditorApplication.update -= waitForUnPausedEditor;
                }
            }
            else
            {
                //Debug.LogError("UNPAUSED MANUALLY");

                //Time.timeScale = 1;
                //DOTween.timeScale = 1;

                UnityEditor.EditorApplication.modifierKeysChanged -= waitForUnPausedByKey;
                UnityEditor.EditorApplication.update -= waitForUnPausedEditor;
            }
        }
#endif
#endregion
    }
}