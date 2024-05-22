using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
#if ENABLE_FACEBOOK
using Facebook.Unity.Editor;
#endif

namespace KobGamesSDKSlim
{

    public class ProjectKickStart : OdinEditorWindow
    {
        public ProjectKickStart()
        {
            EditorApplication.update += Update;
        }

        private static ProjectKickStart s_Window;

        [MenuItem("KobGamesSDK/Project Kick Start %k", false, 50)]
        static void Init()
        {
            s_Window = ScriptableObject.CreateInstance(typeof(ProjectKickStart)) as ProjectKickStart;

            s_Window.ShowUtility();
            s_Window.position = new Rect(460, 200, 580, 840);
        }

        public void ShrinkWindow()
        {
            if (s_Window != null)
                s_Window.position = new Rect(20, 20, 1, 1);
        }

        [HideInInspector]
        public static Action OnProjectKickStartDone = () => { };

        public static ProjectKickStart ShowWindow(Action i_DoneCallback = null)
        {
            Init();

            OnProjectKickStartDone = i_DoneCallback;

            return s_Window;
        }

        private const string k_JarResolverSettingsXMLPath = "ProjectSettings/GvhProjectSettings.xml";
        private const string k_ProjectSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";
        private const string k_UnityConnectAssetPath = "ProjectSettings/UnityConnectSettings.asset";

        private SerializedObject m_UnityConnectAssetFile;

        [SuffixLabel("*True - won't show this menu screen")]
        public bool RunSilently = false;

        [HorizontalGroup("Split", width: 450)]
        [ShowInInspector, ReadOnly, VerticalGroup("Split/a"), Tooltip("Can be used to re-run a secific step in case of an error")]
        private int m_Step = 0;
        private int m_StepMin = 0;
        private int m_StepMax = 10;

        [HideLabel]
        [VerticalGroup("Split/b"), Button("-")]
        public void StepMinus() { m_Step = Mathf.Clamp(m_Step - 1, m_StepMin, m_StepMax); if (m_Step == m_StepMin) m_Step = m_StepMax; }

        [HideLabel]
        [VerticalGroup("Split/c"), Button("+")]
        public void StepPlus() { m_Step = Mathf.Clamp(m_Step + 1, m_StepMin, m_StepMax); if (m_Step == m_StepMax) m_Step = m_StepMin; }

        public bool IsAdsEnabled = true;
        [ShowIf(nameof(IsAdsEnabled))]
        public eMediationNetworks MediationNetworks = eMediationNetworks.MAX;
        public string GameName = "[Fill]";
        public string BundleIdentifier = "com.kobgames.[Fill]";
        public string BuildVersion = "0.1";
        public int BuildNumber = 100;
        public string FacebookId;
        public string AppleStoreId;
        public string GoogleAnalayticsTrackerId = "UA-68933874-46";

#if ENABLE_GAMEANALYTICS
        [ShowInInspector] public string GANameIOS { get { return $"{GameName} - iOS"; } }
        [ShowInInspector] public string GANameAndroid { get { return $"{GameName} - Android"; } }
        private string[] m_GAStudioNamesOptions { get { return GAHelper.GAGetStudiosNames(); } }
        [ValueDropdown(nameof(m_GAStudioNamesOptions)), NonSerialized, ShowInInspector] public string GAStudioName = "Please Select a Studio Name...";
#endif

        private bool m_IsGALoggedIn = false;
        //private bool m_GAPendingCreationOfNewGame = false;

        //private Action m_GACreationSuccessCallback;

        protected override void OnGUI()
        {
            if (GUILayout.Button("Close"))
            {
                s_Window?.Close();
            }

            GUILayout.Label("Clicking on Process will execute the following actions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Copying Managers prefab to Project folder", EditorStyles.boldLabel);
            GUILayout.Label("2. Disable Unity Analytics", EditorStyles.boldLabel);
            GUILayout.Label("3. Disable Unity Ads", EditorStyles.boldLabel);
            GUILayout.Label("4. Disconnect from Unity Services", EditorStyles.boldLabel);
            GUILayout.Label("5. Remove ENABLE_ADS Directive", EditorStyles.boldLabel);
            GUILayout.Label("6. Delete Ads & Disable + Delete iAP Files", EditorStyles.boldLabel);
            GUILayout.Label("7. Set all project settings as written below", EditorStyles.boldLabel);
            GUILayout.Label($"8. Creates GameAnalytics Token for iOS / Android (GA Auth: {!m_IsGALoggedIn})", EditorStyles.boldLabel);
            GUILayout.Label("9. All Done", EditorStyles.boldLabel);

            GUI.color = Color.red;
            GUILayout.Label("* Don't forget copying firebase plist & json files to Plugins folder", EditorStyles.boldLabel);
            GUI.color = Color.white;

            printHR();

            var style1 = new GUIStyle();
            style1.alignment = TextAnchor.MiddleCenter;
            style1.fontStyle = FontStyle.Bold;
            style1.fontSize = 12;
            style1.normal.textColor = Color.white;

            GUILayout.Label("Projct Settings", style1);

            // GameAnalytics related
            gaGUIFrameUpdate();

            printHR();

            base.OnGUI();
        }

        public void printHR()
        {
            GUIStyle horizontalLine;
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;

            var c = GUI.color;
            GUI.color = c;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;

            ////I.color = c;
            ////ILayout.Box(GUIContent.none, horizontalLine);
            ////I.color = c;
        }

        public void OnEnableManual()
        {
            Debug.LogError("OnEnableManual() - GAHelper.GALoginToGameAnalytics()");
            GAHelper.GALoginToGameAnalytics();
            OnEnable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_IsGALoggedIn = false;

            Debug.LogError("OnEnable() GAHelper.GALoginToGameAnalytics()");

            GAHelper.GALoginToGameAnalytics();

            m_UnityConnectAssetFile = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(k_UnityConnectAssetPath));

            loadValues();

            //Danagerous - commented for now
            //selectMainScene();
        }

        private void loadValues()
        {
            if (PlayerSettings.productName != "KobGamesSDK Slim")
            {
                GameName = GameSettings.Instance.General.GameName;
                BundleIdentifier = GameSettings.Instance.General.BundleIdentifier;
                BuildVersion = GameSettings.Instance.General.BuildVersion;
                BuildNumber = GameSettings.Instance.General.BuildNumber;
#if ENABLE_FACEBOOK
                FacebookId = GameSettings.Instance.General.FacebookAppId;
#endif
                AppleStoreId = GameSettings.Instance.General.AppStoreId;

#if ENABLE_GAv4
            GoogleAnalayticsTrackerId = GameSettings.Instance.Analytics.iOSTrackerId;
#endif
            }
        }

        private int m_Frames = 0;
        private int m_FramesToWait = 60;
        private bool m_Active = false;
        private bool m_ExecuteOnlyOnce = false;
        private Action m_DoneWaitingCallback = () => { };
        private void Update()
        {
            if (m_Active)// && !EditorApplication.isCompiling)
            {
                if (m_Frames >= m_FramesToWait)
                {
                    var nextStep = m_ExecuteOnlyOnce ? m_Step : m_Step + 1;

                    Debug.LogError($"Step #{m_Step}: Done! Moving to next step: #{nextStep} ....");

                    if (!m_ExecuteOnlyOnce)
                        m_Step++;

                    m_Active = false;
                    m_Frames = 0;

                    m_DoneWaitingCallback.InvokeSafe();
                    m_DoneWaitingCallback = null;

                    if (!m_ExecuteOnlyOnce)
                        ProcessSteps();
                }

                m_Frames++;
            }
        }

        private void SetWaitFramesBeforeProceeding(int i_FramesToWait, Action i_DoneCallback = null)
        {
            Debug.LogError($"Step #{m_Step}: Waiting {i_FramesToWait} Frames & Compiling to be done ....");

            if (i_DoneCallback != null)
            {
                m_DoneWaitingCallback = i_DoneCallback;
            }

            m_FramesToWait = i_FramesToWait;
            m_Frames = 0;
            m_Active = true;
        }

        [Button(ButtonSizes.Gigantic)]
        public void Process()
        {
            EditorApplication.LockReloadAssemblies();

            m_ExecuteOnlyOnce = false;

            OnProjectKickStartDone += () =>
            {
                Debug.LogError("DONE");

                EditorApplication.UnlockReloadAssemblies();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };

            ProcessSteps();
        }

        [Button(ButtonSizes.Large)]
        public void ProcessOneStep()
        {
            EditorApplication.LockReloadAssemblies();

            m_ExecuteOnlyOnce = true;

            m_DoneWaitingCallback += () =>
            {
                Debug.LogError("DONE");

                EditorApplication.UnlockReloadAssemblies();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };

            ProcessSteps();
        }

        [HorizontalGroup("1")]
        [Button(ButtonSizes.Large), VerticalGroup("1/1")]
        public void SelectGameAnalytics()
        {
            Selection.activeObject = GameAnalyticsSDK.GameAnalytics.SettingsGA;
        }

        [Button(ButtonSizes.Large), VerticalGroup("1/2")]
        public void SelectGameSettings()
        {
            Selection.activeObject = GameSettings.Instance;
        }

        [Button(ButtonSizes.Large)]
        public void FindAndSetGameAnalyticsData()
        {
#if ENABLE_GAMEANALYTICS
            if (m_IsGALoggedIn)
            {
                GAHelper.GASelectGame(GAStudioName, GANameIOS, RuntimePlatform.IPhonePlayer);
                GAHelper.GASelectGame(GAStudioName, GANameAndroid, RuntimePlatform.Android);
            }
            else
            {
                Debug.LogError("GA: Not Logged In");
            }
#endif
        }

        [Button(ButtonSizes.Large)]
        public void SelectOrCreateMissingGAGames()
        {
#if ENABLE_GAMEANALYTICS
            if (m_IsGALoggedIn)
            {
                GAHelper.GAClearPlatformsSettings();

                bool successIOS = GAHelper.GASelectGame(GAStudioName, GANameIOS, RuntimePlatform.IPhonePlayer);
                bool successAndroid = GAHelper.GASelectGame(GAStudioName, GANameAndroid, RuntimePlatform.Android);

                //m_GACreationSuccessCallback = () => SelectOrCreateMissingGAGames();

                if (!successIOS)
                {
                    GAHelper.GACreateGame(GAStudioName, GANameIOS, RuntimePlatform.IPhonePlayer);
                }

                if (!successAndroid)
                {
                    GAHelper.GACreateGame(GAStudioName, GANameAndroid, RuntimePlatform.Android);
                }

                //EditorApplication.delayCall += () =>
                //{
                //    GAHelper.GASelectGame(GAStudioName, GANameIOS, RuntimePlatform.IPhonePlayer);
                //    GAHelper.GASelectGame(GAStudioName, GANameAndroid, RuntimePlatform.Android);
                //};
            }
            else
            {
                Debug.LogError("GA: Not Logged In");
            }
#endif
        }

        [Button(ButtonSizes.Large)]
        public void OpenJarSettingsAndroid()
        {
            GooglePlayServices.PlayServicesResolver.SettingsDialog();
        }

        [Button]
        public static void SelectMainScene()
        {
            var scenes = EditorBuildSettings.scenes.Select(s => s.path)
                                      .Where(s => s.Contains("Main") || s.Contains("Master"));

            if (scenes.Count() > 0)
            {
                var scene = EditorSceneManager.OpenScene(scenes.First());
                EditorSceneManager.SaveScene(scene);
            }
            else
            {
                Debug.LogError($"{nameof(ProjectKickStart)}-SelectMainScene(): Couldn't find Main or Master scenes");
            }
        }

        public void ProcessSteps()
        {
            if (m_Step == 0) // Pre Stuff
            {
                SelectMainScene();

                Selection.activeObject = null;
                SetJarResolverStatus(false);

                m_Step = 1;
            }

            if (m_Step == 1) // Copying Managers prefab to Project folder + Reset Remote Settings
            {
                Debug.LogError($"Step #{m_Step}: Copying Managers prefab to Project folder + Reset Remote Settings");

                DuplicateAndCreatePrefabVariant();

                if (!IsAdsEnabled)
                {
                    disableAdsOnRemoteConfigSettings();
                }

                SetWaitFramesBeforeProceeding(60);
            }

            if (m_Step == 2) // Disable Unity Analytics
            {
                Debug.LogError($"Step #{m_Step}: Disable Unity Analytics");

                m_UnityConnectAssetFile.FindProperty("UnityAnalyticsSettings.m_Enabled").boolValue = false;
                m_UnityConnectAssetFile.ApplyModifiedProperties();

                m_UnityConnectAssetFile.FindProperty("CrashReportingSettings.m_Enabled").boolValue = false;
                m_UnityConnectAssetFile.ApplyModifiedProperties();

                SetWaitFramesBeforeProceeding(60);
            }


            if (m_Step == 3) // Disable Unity Ads
            {
                Debug.LogError($"Step #{m_Step}: Disable Unity Ads");

                m_UnityConnectAssetFile.FindProperty("UnityAdsSettings.m_Enabled").boolValue = false;
                m_UnityConnectAssetFile.FindProperty("UnityAdsSettings.m_GameId").stringValue = string.Empty;

                for (int i = 0; i < m_UnityConnectAssetFile.FindProperty("UnityAdsSettings.m_GameIds").arraySize; i++)
                {
                    var gameIdSerObj = m_UnityConnectAssetFile.FindProperty("UnityAdsSettings.m_GameIds").GetArrayElementAtIndex(i);

                    //Debug.LogError($"Name: {gameIdSerObj.displayName} Value: {gameIdSerObj.FindPropertyRelative("second").stringValue}");
                    gameIdSerObj.FindPropertyRelative("second").stringValue = string.Empty;
                }

                m_UnityConnectAssetFile.ApplyModifiedProperties();

                SetWaitFramesBeforeProceeding(100);
            }


            if (m_Step == 4) // Disconnect From UnityConnect
            {
                Debug.LogError($"Step #{m_Step}: Disconnect From UnityConnect");

                disconnectUnityConnectViaReflection();

                EditorApplication.UnlockReloadAssemblies();
                EditorApplication.LockReloadAssemblies();

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //kob
                //EditorApplication.UnlockReloadAssemblies();

                SetWaitFramesBeforeProceeding(60);
            }

            if (m_Step == 5) // Remove ENABLE_ADS Directive or Set correct mediation network
            {
                if (!IsAdsEnabled)
                {
                    Debug.LogError($"Step #{m_Step}: Remove ENABLE_ADS Directive");

                    UtilsEditor.RemoveDefineDirective(GameSettings.DirectiveConstants.k_ENABLE_ADS_DIRECTIVE);
                }
                else
                {
                    GameSettings.Instance.AdsMediation.MediationNetworks = MediationNetworks;
                }

                SetWaitFramesBeforeProceeding(200);
            }


            if (m_Step == 6) // Delete Ads & iAP Files
            {
                Debug.LogError($"Step #{m_Step}: Delete Ads & iAP Files");

                //kob
                //EditorApplication.LockReloadAssemblies();

                //kob
                //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

                GameSettings.Instance.General.DeleteAdsPlugins();

                //// 11.3.2021: iAP is now removed by default
                //// 9.3.2021: not removing iAP anymore, due to Unity bug not remove UNITY_PURCHASING define (turns out wasn't a bug, but a custom defined that was added by mistake)
                //GameSettings.Instance.General.DeleteIAPPlugins(true);
                //m_UnityConnectAssetFile.FindProperty("UnityPurchasingSettings.m_Enabled").boolValue = false;
                //m_UnityConnectAssetFile.ApplyModifiedProperties();

                //kob
                //EditorApplication.UnlockReloadAssemblies();
                //AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();

                SetWaitFramesBeforeProceeding(300);
            }


            if (m_Step == 7) // Project Configurations (Name, Bundle, Version, FacebookId, GoogleAnalytics Id)
            {
                Debug.LogError($"Step #{m_Step}: Project Configurations (Name, Bundle, Version, FacebookId, GoogleAnalytics Id)");

                setProjectConfigSettings();

                EditorUtility.SetDirty(GameSettings.Instance);
                EditorUtility.SetDirty(this);

                //kob
                //AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();

                SetWaitFramesBeforeProceeding(60);
            }


            if (m_Step == 8) // GameAnalytics Configurations
            {
                Debug.LogError($"Step #{m_Step}: GameAnalytics Configurations...");

                Action GAConfig = () =>
                {
                    SelectOrCreateMissingGAGames();

                    SetWaitFramesBeforeProceeding(2500, () =>
                    {
                        FindAndSetGameAnalyticsData();
                        Debug.LogError($"Step #{m_Step}: Set GameAnalytics Configurations - Done!");
                    });
                };

                if (m_IsGALoggedIn)
                {
                    GAConfig();
                }
                else
                {
                    OnGALoginSuccess -= GAConfig;
                    OnGALoginSuccess += GAConfig;
                }
            }

            if (m_Step == 9) // Project Configurations (Name, Bundle, Version, FacebookId, GoogleAnalytics Id)
            {
                Debug.LogError($"Step #{m_Step}: GameSettings Focus + Inspector");

#if ENABLE_FACEBOOK
                ManifestMod.GenerateManifest();
#endif
                GameSettings.Instance.OnGameSettingFocus();
                GameSettings.Instance.OnGameSettingsGUI();

                GooglePlayServices.PlayServicesResolver.MenuForceResolve();

                SetWaitFramesBeforeProceeding(600);
            }

            if (m_Step == 10) // DONE
            {
                Debug.LogError($"Step #{m_Step}: Project KickStart DONE");

                Selection.activeObject = GameSettings.Instance;
                GameSettings.Instance.General.Load();

                EditorUtility.SetDirty(GameSettings.Instance);
                EditorUtility.SetDirty(this);

                EditorSceneManager.SaveOpenScenes();

                SetJarResolverStatus(true);

                GooglePlayServices.PlayServicesResolver.MenuForceResolve();

                OnProjectKickStartDone.InvokeSafe();
                OnProjectKickStartDone = null;
            }
        }

        public void SetGAStep()
        {
            m_Step = 8;
            ProcessSteps();
        }

        public static void DuplicateAndCreatePrefabVariant()
        {
            string prefabName = "_Managers_V2";

            //PrefabUtility.UnpackPrefabInstance(Managers.Instance.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            //PrefabUtility.SaveAsPrefabAssetAndConnect(Managers.Instance.gameObject, $"Assets/_Project_Specific_Folder/Prefabs/{prefabName}.prefab", InteractionMode.AutomatedAction);
            Managers.Instance.gameObject.ApplyToPrefab($"Assets/_Project_Specific_Folder/Prefabs/{prefabName}.prefab");
            Managers.Instance.gameObject.name = prefabName;
        }

        private void disableAdsOnRemoteConfigSettings()
        {
            Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsAdsEnabled.SetToDefaultValue(false, true);
            Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsRewardVideosEnabled.SetToDefaultValue(false, true);
            Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsTestSuiteEnabled.SetToDefaultValue(false, true);

            Managers.Instance.RemoteSettings.SetProductionMode();

            EditorUtility.SetDirty(Managers.Instance);
            Managers.Instance.ApplyPrefabInstance();
        }

        private void setProjectConfigSettings()
        {
            GameSettings.Instance.General.BundleIdentifier = BundleIdentifier;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BundleIdentifier);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, BundleIdentifier);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, BundleIdentifier);
            PlayerSettings.companyName = "KobGames";
            GameSettings.Instance.General.GameName = PlayerSettings.productName = GameName;
            GameSettings.Instance.General.BuildVersion = PlayerSettings.bundleVersion = BuildVersion;
            GameSettings.Instance.General.BuildNumber = PlayerSettings.Android.bundleVersionCode = BuildNumber;
            PlayerSettings.iOS.buildNumber = BuildNumber.ToString();

#if ENABLE_FACEBOOK
            GameSettings.Instance.General.FacebookAppId = FacebookId;
            GameSettings.Instance.General.SetFacebookAppId();
#endif

            GameSettings.Instance.General.GooglePlayId = BundleIdentifier;
            GameSettings.Instance.General.AppStoreId = AppleStoreId;

#if ENABLE_GAv4
            GameSettings.Instance.Analytics.AndroidTrackerId = GoogleAnalayticsTrackerId;
            GameSettings.Instance.Analytics.iOSTrackerId = GoogleAnalayticsTrackerId;
            GameSettings.Instance.Analytics.SetGAv4Params();
#endif

            //GameSettings.Instance.General.BuildIOSPath = GameSettings.Instance.General.BuildIOSPath.Replace("KobGamesSDKSlim", GameName);

            EditorUtility.SetDirty(Managers.Instance);
            Managers.Instance.ApplyPrefabInstance();
        }

        private void onGALoginSuccess()
        {
#if ENABLE_GAMEANALYTICS
            GAStudioName = m_GAStudioNamesOptions.FirstOrDefault(x => x.Contains("KobGames"));

            if (GAStudioName != null)
                GAStudioName.Trim();

            FindAndSetGameAnalyticsData();
#endif
        }

        //private int m_gaGamesCountOnPrevFrame = -1;
        private static Action OnGALoginSuccess = () => { };
        private void gaGUIFrameUpdate()
        {
#if ENABLE_GAMEANALYTICS
            if (GameAnalyticsSDK.GameAnalytics.SettingsGA.LoginStatus.Contains("Received data"))
            {
                if (!m_IsGALoggedIn)
                {
                    Debug.LogError("GA Logged In");

                    m_IsGALoggedIn = true;
                    onGALoginSuccess();

                    OnGALoginSuccess();
                }
            }

            if (GameAnalyticsSDK.GameAnalytics.SettingsGA.LoginStatus.Contains("Not logged"))
            {
                if (m_IsGALoggedIn)
                {
                    Debug.LogError("GA LOGOFF");

                    m_IsGALoggedIn = false;
                }
            }
#endif
        }

        private void disconnectUnityConnectViaReflection()
        {
            //Debug.LogError("HI");
            UnlinkUnityServices();
        }

        [Button]
        private void UnlinkUnityServices()
        {
            Type myType = null;
            object myObject = null;

            // Reflection
            {
                Type[] types = Assembly.Load("UnityEditor").GetTypes();

                foreach (Type type in types)
                {
                    MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic);

                    foreach (MemberInfo member in members)
                    {
                        if (member.Name.ToLower().Contains("UnbindCloudProject".ToLower()))
                        {
                            //Debug.LogError(type.Name + " " + member.Name);

                            myType = type;

                            break;
                        }
                    }

                    //Debug.LogError("DONE Getting Type");

                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

                    foreach (FieldInfo field in fields)
                    {
                        //Debug.LogError(type.Name + "." + field.Name);

                        if (type.Name == "UnityConnect" && field.Name.ToLower().Contains("s_instance"))
                        {
                            myObject = field.GetValue(null);

                            break;
                        }
                    }

                    //Debug.LogError("DONE Getting instance Object");
                }
            }

            //Debug.LogError(myType);
            //Debug.LogError(myType.Name);
            //Debug.LogError(myObject);

            // Invoke via reflection
            {
                if (myType != null && myObject != null)
                {
                    //Debug.LogError("_");
                    //foreach (var x in myType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
                    //{
                    //    Debug.LogError(x.Name);
                    //}

                    myType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                              .Where(methodInfo => methodInfo.Name.Contains("UnbindCloudProject"))
                              .First()
                              .Invoke(myObject, null);
                }
            }
        }

        private void SetJarResolverStatus(bool i_State)
        {
            typeof(GooglePlayServices.SettingsDialog)
                        .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(methodInfo => methodInfo.Name.Contains("EnableAutoResolution"))
                        .First()
                        .SetValue(this, i_State);
        }

        private void disconnectUnitySettingsViaAssetFile()
        {
            var projectSettingsAssetFile = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(k_ProjectSettingsAssetPath));
            var unityConnectAssetFile = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(k_UnityConnectAssetPath));

            // Project Settings
            {
                //Debug.LogError(projectSettingsAssetFile.FindProperty("cloudServicesEnabled").arraySize);

                for (int i = 0; i < projectSettingsAssetFile.FindProperty("cloudServicesEnabled").arraySize; i++)
                {
                    var cloudServiceSerObj = projectSettingsAssetFile.FindProperty("cloudServicesEnabled").GetArrayElementAtIndex(i);

                    //Debug.LogError($"Name: {cloudServiceSerObj.displayName} Value: {cloudServiceSerObj.FindPropertyRelative("second").boolValue}");
                    cloudServiceSerObj.FindPropertyRelative("second").boolValue = false;
                }

                projectSettingsAssetFile.FindProperty("cloudProjectId").stringValue = string.Empty;
                projectSettingsAssetFile.FindProperty("projectName").stringValue = string.Empty;
                projectSettingsAssetFile.FindProperty("organizationId").stringValue = string.Empty;
                projectSettingsAssetFile.ApplyModifiedProperties();
            }

            // UnityConnect Settings
            {
                unityConnectAssetFile.FindProperty("m_Enabled").boolValue = false;
                unityConnectAssetFile.ApplyModifiedProperties();
            }
        }

        [Button]
        public void UnlockReloadAssemblies()
        {
            EditorApplication.UnlockReloadAssemblies();
        }

        private void reloadGameSettings()
        {
            //AssetDatabase.ForceReserializeAssets();
            //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(GameSettings.Instance));

            var guids = AssetDatabase.FindAssets("t:Script gamesettings");

            foreach (string guid in guids)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        //private void reloadAllScripts()
        //{
        //    AssetDatabase.StartAssetEditing();
        //    string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        //    foreach (string assetPath in allAssetPaths)
        //    {
        //        MonoScript script = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript)) as MonoScript;
        //        if (script != null)
        //        {
        //            Debug.LogError(script.name);
        //            AssetDatabase.ImportAsset(assetPath);
        //        }
        //    }
        //    AssetDatabase.StopAssetEditing();
        //}
    }
}