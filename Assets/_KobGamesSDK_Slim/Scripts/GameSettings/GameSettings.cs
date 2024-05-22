using Sirenix.OdinInspector;
using KobGamesSDKSlim;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
#endif
#if ENABLE_ADJUST
using com.adjust.sdk;
#endif


namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "GameSettings")]
    public class GameSettings : SingletonScriptableObject<GameSettings>
    {
#if UNITY_EDITOR
        #region SDKLogo
        private Texture m_KobGamesInspectorTexture;
        public Texture KobGamesInspectorTexture
        {
            get
            {
                if (m_KobGamesInspectorTexture == null)
                {
                    m_KobGamesInspectorTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_KobGamesSDK_Slim/Graphics/KobGames Inspector Logo.psd");
                }

                return m_KobGamesInspectorTexture;
            }
        }

        [OnInspectorGUI, PropertyOrder(-10)]
        private void ShowImage()
        {
            try
            {
                GUILayout.Label(KobGamesInspectorTexture, EditorStyles.centeredGreyMiniLabel, GUILayout.MinHeight(90));
            }
            catch { }
        }
        #endregion
#endif


        [TabGroup("General"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public GeneralEditor General = new GeneralEditor();

        //[TabGroup("WebTools"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public WebToolsEditor WebTools = new WebToolsEditor();

        [TabGroup("Screenshots"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public ScreenshotsEditor Screenshots = new ScreenshotsEditor();

        [TabGroup("Ads Mediation"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public AdsMediationEditor AdsMediation = new AdsMediationEditor();

        [TabGroup("Remote Settings"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public RemoteSettingsEditor RemoteSettings = new RemoteSettingsEditor();

        [TabGroup("Analytics"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public AnalyticsEditor Analytics = new AnalyticsEditor();

        [TabGroup("RateUs"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public RateUsEditor RateUs = new RateUsEditor();

        [TabGroup("Shortcuts"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public GameSettings_ShortcutsEditor Shortcuts = new GameSettings_ShortcutsEditor();

        [TabGroup("Sync"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public SyncEditor Sync = new SyncEditor();

        [TabGroup("Diff"), ShowInInspector, HideReferenceObjectPicker, HideLabel] public VersionsDiffEditor VersionsDiff = new VersionsDiffEditor();


        #region Selection Changes

        public virtual void OnFocus()
        {
#if UNITY_EDITOR
            //Debug.LogError("OnFocus");

            General.Load();

            if (GameObject.FindObjectOfType<Managers>() == null)
            {
                Debug.LogError("Managers object doesnt exists in the scene, make sure to add it manually");
            }

            Analytics.SetCustomDimension01();
            Analytics.SetCustomDimension02();
            Analytics.SetCustomDimension03();

            Sync.SetPathToThisProject();
#endif
        }

#if UNITY_EDITOR
        public void SceneLoaded()
        {
            var autoSelectedGameObject = Resources.FindObjectsOfTypeAll<GameObjectAutoSelect>().Where(x => !EditorUtility.IsPersistent(x.gameObject)).FirstOrDefault();

            if (autoSelectedGameObject != null)
            {
                Debug.LogError($"Auto GameObject Select: {autoSelectedGameObject.name} Scene: {autoSelectedGameObject.gameObject.scene.name}");

                Selection.activeGameObject = autoSelectedGameObject.gameObject;
                EditorGUIUtility.PingObject(Selection.activeObject);
                //}
                //else
                //{
                //    Debug.LogError($"Auto GameObject Select: Couldn't find Instance Id");
                //}
            }
        }
#endif
        #endregion

#if UNITY_EDITOR
        static UnityEditor.PackageManager.Requests.RemoveRequest s_RemRequest;
        static Queue<string> s_pkgNameQueue;

        // this is called via a UI button
        public static void StartRemovingFirebasePackages()
        {
            s_pkgNameQueue = new Queue<string>();
            s_pkgNameQueue.Enqueue("com.google.firebase.analytics");
            s_pkgNameQueue.Enqueue("com.google.firebase.crashlytics");
            s_pkgNameQueue.Enqueue("com.google.firebase.dynamic-links");
            s_pkgNameQueue.Enqueue("com.google.firebase.instance-id");
            s_pkgNameQueue.Enqueue("com.google.firebase.remote-config");

            // callback for every frame in the editor
            EditorApplication.update += PackageRemovalProgress;
            EditorApplication.LockReloadAssemblies();

            var nextRequestStr = s_pkgNameQueue.Dequeue();
            s_RemRequest = UnityEditor.PackageManager.Client.Remove(nextRequestStr);

            return;
        }


        static void PackageRemovalProgress()
        {
            if (s_RemRequest.IsCompleted)
            {
                switch (s_RemRequest.Status)
                {
                    case UnityEditor.PackageManager.StatusCode.Failure:    // couldn't remove package
                        if (!s_RemRequest.Error.message.Contains("cannot be found"))
                        {
                            Debug.LogError("Couldn't remove package '" + s_RemRequest.PackageIdOrName + "': " + s_RemRequest.Error.message);
                        }
                        break;

                    case UnityEditor.PackageManager.StatusCode.InProgress:
                        break;

                    case UnityEditor.PackageManager.StatusCode.Success:
                        Debug.Log("Removed package: " + s_RemRequest.PackageIdOrName);
                        break;
                }

                if (s_pkgNameQueue.Count > 0)
                {
                    var nextRequestStr = s_pkgNameQueue.Dequeue();
                    Debug.Log("Requesting removal of '" + nextRequestStr + "'.");
                    s_RemRequest = UnityEditor.PackageManager.Client.Remove(nextRequestStr);

                }
                else
                {    // no more packages to remove
                    EditorApplication.update -= PackageRemovalProgress;
                    EditorApplication.UnlockReloadAssemblies();
                }
            }

            return;
        }

        public class DirectiveConstants
        {
            public const string k_ENABLE_LOGS_DIRECTIVE = "ENABLE_LOGS";
            public const string k_ENABLE_ADS_DIRECTIVE = "ENABLE_ADS";
            public const string k_ENABLE_IRONSOURCE_DIRECTIVE = "ENABLE_IRONSOURCE";
            public const string k_ENABLE_MAX_DIRECTIVE = "ENABLE_MAX";
            public const string k_ENABLE_SUPERERA_DIRECTIVE = "ENABLE_SUPERERA";
            public const string k_ENABLE_BEPIC_DIRECTIVE = "ENABLE_BEPIC";
            public const string k_ENABLE_KOBIC_DIRECTIVE = "ENABLE_KOBIC";

            public const string k_ENABLE_ADJUST_DIRECTIVE = "ENABLE_ADJUST";
            public const string k_ENABLE_APPSFLYER_DIRECTIVE = "ENABLE_APPSFLYER";
            public const string k_ENABLE_APPMETRICA_DIRECTIVE = "ENABLE_APPMETRICA";
            public const string k_ENABLE_GAv4_DIRECTIVE = "ENABLE_GAv4";
            public const string k_ENABLE_FIREBASE_DIRECTIVE = "ENABLE_FIREBASE";
            public const string k_ENABLE_GAMEANALYTICS_DIRECTIVE = "ENABLE_GAMEANALYTICS";
            public const string k_ENABLE_FACEBOOK_DIRECTIVE = "ENABLE_FACEBOOK";
            public const string k_ENABLE_WEBTOOLS_DIRECTIVE = "ENABLE_WEBTOOLS";
            public const string k_ENABLE_FEEDBACK_DIRECTIVE = "ENABLE_FEEDBACK";
            public const string k_KOBGAMES_DIRECTIVE = "KobGamesSDKSlim";
        }

        public void OnGameSettingFocus()
        {
            UtilsEditor.SetDefineDirective(DirectiveConstants.k_KOBGAMES_DIRECTIVE);
        }

        public void OnGameSettingsGUI()
        {
            if (General.IsAdsEnabled)
            {
                if (AdsMediation.IsIronSourceEnabled)
                {
                    UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_IRONSOURCE_DIRECTIVE);
                }
                else
                {
                    UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_IRONSOURCE_DIRECTIVE);
                }

                if (AdsMediation.IsMAXEnabled)
                {
                    UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_MAX_DIRECTIVE);
                }
                else
                {
                    UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_MAX_DIRECTIVE);
                }

                if (AdsMediation.IsSupereraEnabled)
                {
                    UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_SUPERERA_DIRECTIVE);
                }
                else
                {
                    UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_SUPERERA_DIRECTIVE);
                }

                if (AdsMediation.IsBepicEnabled)
                {
                    UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_BEPIC_DIRECTIVE);

                    if (Managers.Instance != null)
                    {
                        Managers.Instance.AddBepicToManagers();
                    }
                }
                else
                {
                    UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_BEPIC_DIRECTIVE);

                    if (Managers.Instance != null)
                    {
                        Managers.Instance.RemoveBepicFromManagers();
                    }
                }

                if (AdsMediation.IsKobicEnabled)
                {
                    UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_KOBIC_DIRECTIVE);
                }
                else
                {
                    UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_KOBIC_DIRECTIVE);
                }
            }

            if (!AdsMediation.IsIronSourceEnabled)
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_IRONSOURCE_DIRECTIVE);
            }

            if (!AdsMediation.IsMAXEnabled)
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_MAX_DIRECTIVE);
            }

            if (!AdsMediation.IsSupereraEnabled)
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_SUPERERA_DIRECTIVE);

            if (!AdsMediation.IsBepicEnabled)
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_BEPIC_DIRECTIVE);

            if (!AdsMediation.IsKobicEnabled)
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_KOBIC_DIRECTIVE);
            }

            if (Analytics.AnalyticsManager.IsAppsFlyerEnabled)
            {
                UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_APPSFLYER_DIRECTIVE);
            }
            else
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_APPSFLYER_DIRECTIVE);
            }

            if (Analytics.AnalyticsManager.IsAdjustEnabled)
            {
                UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_ADJUST_DIRECTIVE);
            }
            else
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_ADJUST_DIRECTIVE);
            }

            if (Analytics.AnalyticsManager.IsAppMetricaEnabled)
            {
                if (UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_APPMETRICA_DIRECTIVE))
                {
                    Utils.EditorDelayRun(Managers.Instance.AddAppMetricaToManagers);
                }
            }
            else
            {
                if (UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_APPMETRICA_DIRECTIVE))
                {
                    Utils.EditorDelayRun(Managers.Instance.RemoveAppMetricaFromManagers);
                }
            }

            if (Analytics.AnalyticsManager.IsGoogleAnalyticsEnabled)
            {
                UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_GAv4_DIRECTIVE);
            }
            else
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_GAv4_DIRECTIVE);
            }

            if (Analytics.AnalyticsManager.IsFirebaseAnalyticsEnabled)
            {
                UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_FIREBASE_DIRECTIVE);
            }
            else
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_FIREBASE_DIRECTIVE);
            }

            if (Analytics.AnalyticsManager.IsGameAnalyticsEnabled)
            {
                UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_GAMEANALYTICS_DIRECTIVE);
            }
            else
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_GAMEANALYTICS_DIRECTIVE);
            }

            if (Analytics.AnalyticsManager.IsFacebookAnalyticsEnabled)
            {
                UtilsEditor.SetDefineDirective(DirectiveConstants.k_ENABLE_FACEBOOK_DIRECTIVE);
            }
            else
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_FACEBOOK_DIRECTIVE);
            }

            // Remove Legacy defines 
            {
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_WEBTOOLS_DIRECTIVE);
                UtilsEditor.RemoveDefineDirective(DirectiveConstants.k_ENABLE_FEEDBACK_DIRECTIVE);
            }

            // MAX Mediation
#if ENABLE_ADS && ENABLE_MAX
            if (AdsMediation.IsMAXEnabled)
            {
                EditorApplication.delayCall += () =>
                {
                    if ((AppLovinSettings.Instance.AdMobIosAppId != AdsMediation.AdMobAppIdIos) ||
                        (AppLovinSettings.Instance.AdMobAndroidAppId != AdsMediation.AdMobAppIdAndroid))
                    {
                        AppLovinSettings.Instance.AdMobIosAppId = AdsMediation.AdMobAppIdIos;
                        AppLovinSettings.Instance.AdMobAndroidAppId = AdsMediation.AdMobAppIdAndroid;

                        Debug.LogError("GamesSettings: Saving MAX AdMob Ids Settings");

                        EditorUtility.SetDirty(AppLovinSettings.Instance);
                        AssetDatabase.SaveAssets();
                    }
                };
            }
#endif

            Managers.Instance.Reset();
        }
#endif
    }

    [Serializable]
    public class GeneralEditor
    {
        [PropertyOrder(-102), BoxGroup, ShowInInspector, DisplayAsString, HideLabel, EnableGUI, GUIColor(nameof(GetAdsDetailsStatusColor))]
        private string m_AdsDetailsStatus
        {
            get => $"Ads: {GameConfig.Instance.AdsMediation.IsAdsEnabled} | " +
                                              $"Banner: {GameConfig.Instance.AdsMediation.IsBannerEnabled} | " +
                                              $"Interstitial: {GameConfig.Instance.AdsMediation.IsInterstitialEnabled} | " +
                                              $"RewardVideo: {GameConfig.Instance.AdsMediation.IsRewardVideosEnabled} | " +
                                              $"DummyAds: {GameConfig.Instance.AdsMediation.UseDummyAdsOnDevice}";
        }

        [PropertyOrder(-101), BoxGroup, ShowInInspector, DisplayAsString, HideLabel, EnableGUI, ShowIf(nameof(IsDiffExists)), GUIColor(nameof(GetDiffStatusColor))]
        private string m_VersionsDiffStatus
        {
            get => GameSettings.Instance.VersionsDiff.VersionsDiffStatus;
        }

        public Color GetDiffStatusColor()
        {
            return GameSettings.Instance.VersionsDiff.GetDiffStatusColor();
        }

        public bool IsDiffExists()
        {
            return GameSettings.Instance.VersionsDiff.IsDiffExists();
        }

#if ENABLE_BEPIC && UNITY_EDITOR
        [PropertyOrder(-101), OnInspectorInit(nameof(fetchBepicJson)), BoxGroup, ShowInInspector, DisplayAsString, HideLabel, EnableGUI, GUIColor(0, 1, 0)]
        private string m_BepicInfo = "None";

        private void fetchBepicJson()
        {
            string[] guids = AssetDatabase.FindAssets("SimpleSDKConfig");
            string bepicJson = "None";

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.Contains(".json"))
                {
                    var jsonString = File.ReadAllText(path);
                    SimpleSDKConfigJson Lines = JsonUtility.FromJson<SimpleSDKConfigJson>(jsonString);
                    bepicJson = $"{Lines.gameName}";
                }
            }

            var toponVersion = Type.GetType("SimpleSDKTopon")?.GetField("TOPON_VERSION")?.GetValue(Type.GetType("SimpleSDKTopon"));
            var mediation = Type.GetType("SimpleSDKMediation")?.GetField("VERSION")?.GetValue(Type.GetType("SimpleSDKMediation"));

            toponVersion = toponVersion != null ? toponVersion : (mediation != null ? mediation : "None");

            m_BepicInfo = $"{bepicJson} | " +
                          $"SDK: {SimpleSDKNS.SimpleSDK.SDK_VERSION} | " +
                          $"Topon: {toponVersion} | " +
                          $"Fx: {FxNS.FxSdk.FX_SDK_VERSION} | " +
                          $"AF: {SimpleSDKNS.AppsflyerHelper.afInstance.getAttrVersion()}";
        }

        [Serializable]
        public class SimpleSDKConfigJson
        {
            public string gameName;
        }
#endif

        private static Color GetAdsDetailsStatusColor()
        {
            return GameConfig.Instance.AdsMediation.IsAdsEnabled ? Color.green : Color.red;
        }

        //private bool isPluginsUpToDate()
        //{
        //    return
        //        PluginsVersion.Instance != null &&
        //        GameSettings.Instance != null &&
        //        GameSettings.Instance.AdsMediation != null &&
        //        GameSettings.Instance.AdsMediation.SDKVersionsList != null &&
        //        PluginsVersion.Instance.Version != GameSettings.Instance.AdsMediation.SDKVersionsList.PluginsVersion;
        //}

        //private string str { get { return $"Plugins folder is outdated, make sure to copy from SDK ver {GameSettings.Instance.AdsMediation.SDKVersionsList.PluginsVersion} found: { PluginsVersion.Instance.Version }"; } }

        private void openQualitySettings()
        {
#if UNITY_EDITOR
            Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("QualitySettings");
            //SettingsService.OpenProjectSettings("Project/Quality");
#endif
        }

        
        
        
#if UNITY_EDITOR
        private const int k_BuildTabsOrder = -4;
        private const int k_LabelWidth = 140;
        private const string k_AndroidTabFoldout = "Tab/Build Android/More Options";
        private const string k_IOSTabFoldout = "Tab/Build iOS/More Options";

        [OnInspectorInit("@#(#Tab).State.Set<int>(\"CurrentTabIndex\", TabSelect)")]
        public int TabSelect => EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? 1 : 0;
        //public int TabSelect => TabSelector;
        //public int TabSelector = -1;

        // Android
        [InfoBox("Development Build", InfoMessageType.Warning, nameof(IsConfiguredAsDevelopmentBuildAndroid))]
        [TabGroup("Tab", "Build Android"), LabelWidth(k_LabelWidth), PropertyOrder(k_BuildTabsOrder), Title("Build Android"), InlineButton(nameof(buildAndroid)), InlineButton(nameof(SwitchToMono), "Mono"), InlineButton(nameof(SwitchToIL2CPP), "IL2CPP"), ShowInInspector]
        public string BuildAndroidPath { get => EditorPrefs.GetString(Constants.k_BuildAndroidEditorPrefKey, "/Users/kobyle/GamesProjects/Builds/"); set => EditorPrefs.SetString(Constants.k_BuildAndroidEditorPrefKey, value); }
        [FoldoutGroup(k_AndroidTabFoldout), LabelWidth(k_LabelWidth), DisplayAsString, ShowInInspector]
        public string BuildAndroidType { get { return $"{PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)}_{PlayerSettings.Android.targetArchitectures.ToString().Replace(", ", "_")}"; } }
        [FoldoutGroup(k_AndroidTabFoldout), LabelWidth(k_LabelWidth), ShowInInspector, DisplayAsString, HideLabel]
        public string BuildAndroidProductName { get { return $"{PlayerSettings.productName}_v{BuildVersion}_{BuildNumber}_{PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)}_{PlayerSettings.Android.targetArchitectures.ToString().Replace(", ", "_")}_{(BuildOptionAndroid.HasFlag(BuildOptions.Development) ? "Dev" : "Prod")}.{(IsAppBundle ? "aab" : "apk")}"; } }
        public string BuildAndroidFolderPath { get { return $"{BuildAndroidPath}{PlayerSettings.productName}_v{BuildVersion}_{BuildNumber}_Android/"; } }
        public string BuildAndroidFullPathIncludingFolder { get { return $"{BuildAndroidFolderPath}{BuildAndroidProductName}"; } }

        [FoldoutGroup(k_AndroidTabFoldout), LabelWidth(k_LabelWidth), ShowInInspector]
        public BuildOptions BuildOptionAndroid = BuildOptions.AutoRunPlayer | BuildOptions.ShowBuiltPlayer;
        [FoldoutGroup(k_AndroidTabFoldout), LabelWidth(k_LabelWidth)]
        public bool IsAppBundle = false;
        [FoldoutGroup(k_AndroidTabFoldout), LabelWidth(k_LabelWidth)]
        public bool AddressableAndroid = false;

        // IOS
        [InfoBox("Development Build", InfoMessageType.Warning, nameof(IsConfiguredAsDevelopmentBuildIOS))]
        [TabGroup("Tab", "Build iOS"), LabelWidth(k_LabelWidth), PropertyOrder(k_BuildTabsOrder), Title("Build IOS"), InlineButton(nameof(buildIOS)), InlineButton(nameof(openTerminal)), ShowInInspector]
        public string BuildIOSPath { get => EditorPrefs.GetString(Constants.k_BuildIOSEditorPrefKey, "/Users/kobyle/GamesProjects/Builds/"); set => EditorPrefs.SetString(Constants.k_BuildIOSEditorPrefKey, value); }
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), ShowInInspector, DisplayAsString, HideLabel]
        public string BuildIOSFullPath { get { return $"{BuildIOSPath}{PlayerSettings.productName}_v{BuildVersion}_{BuildNumber}"; } }

        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), ShowInInspector]
        public BuildOptions BuildOptionIOS = BuildOptions.AutoRunPlayer | BuildOptions.ShowBuiltPlayer;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), ShowInInspector]
        public bool LinkerAll = true;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), ShowInInspector]
        public bool RemoveMetal = false;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth)]
        public bool AddressableIOS = false;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), OnInspectorInit(nameof(SetFastlaneGameName))]
        public string FastlaneGameName = string.Empty;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth)]
        public string PolicyURL = string.Empty;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), InlineButton(nameof(SetScreenshotsPath), label: "Set")]
        public string ScreenshotsPath = string.Empty;
        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), ValueDropdown(nameof(BuildSuccessValues))]
        public string BuildSuccessCmd = k_BuildAndUpload;

        private ValueDropdownList<string> BuildSuccessValues = new ValueDropdownList<string>()
        {
            {$"None", k_None },
            {$"Update Meta Data", k_UpdateMetaData },
            {$"Update Policy", k_UpdatePolicy },
            {$"Build And Upload", k_BuildAndUpload },
            {$"Upload Only", k_UploadOnly }
        };

        public void SetFastlaneGameName()
        {
            if (FastlaneGameName == string.Empty)
            {
                FastlaneGameName = GameName;
            }
        }

        public void SetScreenshotsPath()
        {
            ScreenshotsPath = GameSettings.Instance.Screenshots.VMFastlaneScreenshotsPath;
        }

        public const string k_None = "-1";
        public const string k_UpdateMetaData = "2";
        public const string k_UpdatePolicy = "3";
        public const string k_BuildAndUpload = "4";
        public const string k_UploadOnly = "5";
        public const string k_BuildAndroid = "Android";
        public const string k_BuildIOS = "IOS";

        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), Button(ButtonSizes.Medium)]
        public void UpdateMetaDataAndPrivacy()
        {
            DeliverIOS(k_UpdateMetaData);
        }

        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), Button(ButtonSizes.Medium)]
        public void UpdatePolicy()
        {
            DeliverIOS(k_UpdatePolicy);
        }

        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), Button(ButtonSizes.Medium)]
        public void BuildAndUpload()
        {
            DeliverIOS(k_BuildAndUpload);
        }

        [FoldoutGroup(k_IOSTabFoldout), LabelWidth(k_LabelWidth), Button(ButtonSizes.Medium)]
        public void UploadOnly()
        {
            DeliverIOS(k_UploadOnly);
        }

        public void DeliverIOS(string i_MenuAction)
        {
            string scriptPath, scriptPathKob, scriptPathVM;

            scriptPath = string.Empty;
            scriptPathKob = $"/Users/kobyle/Dropbox/Shared-VM/FastlaneFiles/scripts";
            scriptPathVM = $"/Users/kobyle/Dropbox/Shared-VM/FastlaneFiles/scripts";

            if (Directory.Exists(scriptPathKob))
            {
                scriptPath = scriptPathKob;
            }
            else if (Directory.Exists(scriptPathVM))
            {
                scriptPath = scriptPathVM;
            }

            if (scriptPath != string.Empty)
            {
                var script = "runCreateApp.command";
                var appProfile = "1"; // 1 - kobic / guy, 2 - bepic carlos, 3 - bepic maria, 4 - artyom, 5 - koby, 6 - azur

                switch (SignIOSTeamID)
                {
                    case k_KpicGuy:
                        appProfile = "1";
                        break;
                    case k_BepCarlos:
                        appProfile = "2";
                        break;
                    case k_BepMaria:
                        appProfile = "3";
                        break;
                    case k_BepArtyom:
                        appProfile = "4";
                        break;
                    case k_KobyTeamID:
                        appProfile = "5";
                        break;
                    case k_Az:
                        appProfile = "6";
                        break;
                    default:
                        break;
                }

                var menuAction = i_MenuAction; // 1 - Produce , 2 - Update Meta , 3 - Update Policy , 4 - Build & Upload , 5 - Upload Only
                var appBundle = GameSettings.Instance.General.BundleIdentifier;
                var appName = GameSettings.Instance.General.FastlaneGameName;
                var appVersion = GameSettings.Instance.General.BuildVersion;
                var appPolicyURL = GameSettings.Instance.General.PolicyURL;
                var appScreenshotsPath = GameSettings.Instance.General.ScreenshotsPath;
                var appProjectXcodePath = $"{BuildIOSFullPath}/Unity-iPhone.xcodeproj";

                // Writing config arguments to disk
                var arguments = $"appMenuAction=\"{menuAction}\" appProfile=\"{appProfile}\" appBundle=\"{appBundle}\" appName=\"{appName}\" appVersion=\"{appVersion}\" appPolicyURL=\"{appPolicyURL}\" appScreenshotsPath=\"{appScreenshotsPath}\" appProjectXcodePath=\"{appProjectXcodePath}\"";
                string createText = arguments + Environment.NewLine;
                File.WriteAllText($"{scriptPath}/args.txt", createText);

                System.Diagnostics.Process.Start("osascript", $"-e 'tell application \"Terminal\" to do script \"{scriptPath}/{script} args\"'");
                System.Diagnostics.Process.Start("osascript", $"-e 'tell application \"Terminal\" to activate'");
                System.Diagnostics.Process.Start("osascript", "-e 'tell application \"Terminal\" to set the bounds of front window to { 100, 200, 1800, 800}'");
            }
        }

        public bool IsConfiguredAsDevelopmentBuildIOS()
        {
            return (BuildOptionIOS & BuildOptions.Development) == BuildOptions.Development;
        }

        public bool IsConfiguredAsDevelopmentBuildAndroid()
        {
            return (BuildOptionAndroid & BuildOptions.Development) == BuildOptions.Development;
        }

        public void SwitchToMono()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        }

        public void SwitchToIL2CPP()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;// | AndroidArchitecture.X86_64;
            //PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
        }
#endif

        //[InfoBox("$str", nameof(isPluginsUpToDate), InfoMessageType = InfoMessageType.Error)]
        [PropertyOrder(-2), Title("General"), InlineButton(nameof(ReCopyProject))]
        [InfoBox("Note that changing some of the below settings will cause unity to re-compile, in which case the inspector will be reloaded once compiled", InfoMessageType.Info)]
        public bool IsInit;

#if UNITY_EDITOR
        [PropertyOrder(-1), ShowInInspector, InlineButton(nameof(openQualitySettings))]
        public bool IsDebugEnabled { get => EditorPrefs.GetBool(Constants.k_IsDebugLogsEditorPrefKey, true); set => EditorPrefs.SetBool(Constants.k_IsDebugLogsEditorPrefKey, value); }
#endif

#if !UNITY_PURCHASING
        [InlineButton(nameof(DeleteIAPPlugins))]
#endif
#if !ENABLE_ADS
    [InlineButton(nameof(DeleteAdsPlugins))]
#endif
        public bool IsAdsEnabled;
        //[DisplayAsString, InfoBox("Check Quality Settings", nameof(VerifyQualitySettings), InfoMessageType = InfoMessageType.Warning), InlineButton(nameof(openQualitySettings))]
        //public string QualitySetting = "None";
        [OnValueChanged(nameof(Save))]
        public Sprite Icon;
        [OnValueChanged(nameof(Save))]
        public Sprite SplashImage;
        public bool UpdateSplash = true;
        public bool IOSTeamIDTest = false;

        [HorizontalGroup("AutoSign")]
        [VerticalGroup("AutoSign/1"), LabelText("Sign IOS TeamID"), OnValueChanged(nameof(Save))]
        public bool AutoSign = true;
        [VerticalGroup("AutoSign/2"), ValueDropdown(nameof(SignIOSTeamIDValues)), OnValueChanged(nameof(Save)), HideLabel, ShowIf(nameof(AutoSign))]
        public string SignIOSTeamID = k_KobyTeamID;

        public const string k_KobyTeamID = "6T5SK42BUG";
        public const string k_YakovTeamID = "Q2K5DK5986";
        public const string k_VTeamID = "QUZ546KL38";
        public const string k_Wak = "JDUQM39U2T";
        public const string k_Az = "XCVXFM7SV3";
        public const string k_AzAI = "SXQF39B47F";
        public const string k_BepCarlos = "D89KY442NR";
        public const string k_BepMaria = "9W632B3D96";
        public const string k_BepArtyom = "2W94JVYPBL";
        public const string k_KpicGuy = "ZVB6BWCW85";

        private ValueDropdownList<string> SignIOSTeamIDValues = new ValueDropdownList<string>()
        {
            {$"{k_KobyTeamID} - Koby", k_KobyTeamID },
            {$"{k_YakovTeamID} - Yakov", k_YakovTeamID },
            {$"{k_VTeamID} - V", k_VTeamID },
            {$"{k_Wak} - Wak", k_Wak },
            {$"{k_Az} - Az", k_Az },
            {$"{k_AzAI} - AzAI", k_AzAI },
            {$"{k_BepCarlos} - Bp Carlos", k_BepCarlos },
            {$"{k_BepMaria} - Bp Maria", k_BepMaria },
            {$"{k_BepArtyom} - Bp Artyom", k_BepArtyom },
            {$"{k_KpicGuy} - Kpic Guy", k_KpicGuy }
        };

        [HorizontalGroup("SeparateBundles")]
        [VerticalGroup("SeparateBundles/1"), LabelText("Bundle Identified"), Tooltip("Separated Bundles")]
        public bool SeparateBundles = false;
        [VerticalGroup("SeparateBundles/2")]
        [OnValueChanged(nameof(Save)), DelayedProperty, InlineButton(nameof(UpdatePackageInManifest), "Update Manifest"), HideLabel]
        public string BundleIdentifier;

        [OnValueChanged(nameof(Save)), DelayedProperty, ShowIf(nameof(IsBundleIdentifierAndroidAvailable))]
        public string BundleIdentifierAndroid;
        public string GetBundleIdentifierAndroid()
        {
            return IsBundleIdentifierAndroidAvailable ? BundleIdentifierAndroid : BundleIdentifier;
        }

        private bool IsBundleIdentifierAndroidAvailable
        {
            get
            {
                return SeparateBundles;
                //return GameSettings.Instance.AdsMediation.IsBepicEnabled || SignIOSTeamID == k_Az;
            }
        }

        [OnValueChanged(nameof(Save)), DelayedProperty]
        public string GameName;
        [OnValueChanged(nameof(SaveBuild)), DelayedProperty]
        public string BuildVersion;
        [OnValueChanged(nameof(SaveBuild)), DelayedProperty]
        public int BuildNumber;
        public string AppsFlyerId = string.Empty;
        [OnValueChanged(nameof(OnAdjustIdChanged)), DelayedProperty, InlineButton(nameof(SelectAdjustGO), "Select")]
        public string AdjustId = string.Empty;

        [OnValueChanged(nameof(Save)), DelayedProperty, InlineButton(nameof(SelectAppMetricaGO), "Select")]
        public string AppMetricaId = string.Empty;
#if ENABLE_FACEBOOK
        [OnValueChanged(nameof(SetFacebookAppId)), InlineButton(nameof(SelectFacebookSettings), "Select")]
        public string FacebookAppId;
        [Multiline]
        public string FacebookAppIdNotes;
#endif

        [InfoBox("AppsFlyer key exists, make sure to set the GooglePlay Id", nameof(m_IsAppFlyerKeyExistsGoogle), InfoMessageType = InfoMessageType.Warning), ReadOnly]
        public string GooglePlayId = string.Empty;
        [InfoBox("AppsFlyer key exists, make sure to set the AppStore Id", nameof(m_IsAppFlyerKeyExistsApple), InfoMessageType = InfoMessageType.Warning)]
        public string AppStoreId = string.Empty;
        public bool EnforceQualitySettings = true;

#if UNITY_EDITOR
        [BoxGroup("APK"), PropertyOrder(2), InfoBox("Make sure to select a key store", nameof(SignAPK)), OnValueChanged(nameof(SetSignAPK)), ShowInInspector]
        public bool SignAPK { get => EditorPrefs.GetBool(Constants.k_SignAPKEditorPrefKey, false); set => EditorPrefs.SetBool(Constants.k_SignAPKEditorPrefKey, value); }
        [BoxGroup("APK"), PropertyOrder(3), ShowInInspector]
        public string KeyStorePath { get => EditorPrefs.GetString(Constants.k_KeyStorePathEditorPrefKey, "/Users/kobyle/Dropbox/Games/KeyStores"); set => EditorPrefs.SetString(Constants.k_KeyStorePathEditorPrefKey, value); }
        [BoxGroup("APK"), PropertyOrder(4)] public string KeyStoreName = "";
        [BoxGroup("APK"), PropertyOrder(5)] public string KeyStorePass = "Mazit1!";
        [BoxGroup("APK"), PropertyOrder(6)] public string KeyaliasName = "mazitkey";
        [BoxGroup("APK"), PropertyOrder(7)] public string KeyaliasPass = "Mazit1!";
#endif

        //#if UNITY_EDITOR
        //        [BoxGroup("Build Save Path"), ShowInInspector]
        //        [InlineButton(nameof(BrowseSavePath), "Browse")]
        //        public string BuildSavePath { get => EditorPrefs.GetString(Constants.k_BuildSavePathKey, @"D:/Dropbox/Games"); set => EditorPrefs.SetString(Constants.k_BuildSavePathKey, value); }
        //#endif

        private bool m_IsAppFlyerKeyExistsGoogle { get { return AppsFlyerId != string.Empty && GooglePlayId == string.Empty; } }
        private bool m_IsAppFlyerKeyExistsApple { get { return AppsFlyerId != string.Empty && AppStoreId == string.Empty; } }

        private const string k_AppSpecificPassKoby = "ovxr-ihfe-kmlj-njut";
        private const string k_AppSpecificPassYakov = "msfj-vnux-axaa-ozre";

        //private void RemoveAppsFlyerManifest()
        //{
        //    Utils.RemoveAppsFlyerFrmoManifest();
        //}

        //public void TEST()
        //{
        //    string pathToDelete = Application.dataPath + "/_KobGamesSDK_Slim/_3rd_Party/Analytics/AppsFlyer/Editor/AppsFlyerDependencies";
        //    Directory.CreateDirectory(Application.dataPath + $"/_KobGamesSDK_Slim/_3rd_Party/Analytics/AppsFlyer/Editor/Hidden~/");
        //    File.Move($"{pathToDelete}.xml", Application.dataPath + $"/_KobGamesSDK_Slim/_3rd_Party/Analytics/AppsFlyer/Editor/Hidden~/AppsFlyerDependencies.xml");
        //}

#if UNITY_EDITOR
        private void openTerminal()
        {
            string path = string.Empty;
            string folderName = string.Empty;
            string appSpecificPassword = k_AppSpecificPassKoby;

            if (SignIOSTeamID == k_KobyTeamID)
            {
                folderName = GameEditorConstants.Instance.FolderFastlaneKobyProfile;
                appSpecificPassword = k_AppSpecificPassKoby;
            }
            else if (SignIOSTeamID == k_YakovTeamID)
            {
                folderName = GameEditorConstants.Instance.FolderFastlaneYakovProfile;
                appSpecificPassword = k_AppSpecificPassYakov;
            }

            path = $"{GameEditorConstants.Instance.FolderFastlanePath}{folderName}";

            //Debug.LogError(path);

            System.Diagnostics.Process.Start("osascript", $"-e 'tell application \"Terminal\" to do script \"cd '\"'{ BuildIOSFullPath }'\"' " +
                                                          $"&& rm -rf '\"'{"fastlane"}'\"' " +
                                                          $"&& cp -R '\"'{path}'\"' . " +
                                                          $"&& mv '\"'{folderName}'\"' '\"'{"fastlane"}'\"' " +
                                                          $"&& sed 's/CHANGEME/{BundleIdentifier}/g' fastlane/Appfile > fastlane/Appfile1 " +
                                                          $"&& mv fastlane/Appfile1 fastlane/Appfile" +
                                                          $"&& export FASTLANE_APPLE_APPLICATION_SPECIFIC_PASSWORD={appSpecificPassword}" +
                                                          $"&& cat fastlane/Appfile" +
                                                          $"&& fastlane" +
                                                          //$"&& env " +
                                                          $"\"'");
            System.Diagnostics.Process.Start("osascript", $"-e 'tell application \"Terminal\" to activate'");
            //System.Diagnostics.Process.Start("osascript", "-e 'tell application \"System Events\" to keystroke \"=\" using { command down, control down } with delay 2'");
            System.Diagnostics.Process.Start("osascript", "-e 'tell application \"Terminal\" to set the bounds of front window to { 100, 200, 1800, 800}'");

            //System.Diagnostics.Process.Start("osascript", $"-e 'tell application \"Terminal\" to do script \"ls .\"'");
            //System.Diagnostics.Process.Start("osascript", $"-e 'tell application \"Terminal\" to do script \"echo 'hi' \"'");
        }
#endif

        public void OnBuildSuccess()
        {
#if UNITY_EDITOR
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                switch (BuildSuccessCmd)
                {
                    case k_UpdateMetaData:
                        UpdateMetaDataAndPrivacy();
                        break;
                    case k_UpdatePolicy:
                        UpdatePolicy();
                        break;
                    case k_BuildAndUpload:
                        BuildAndUpload();
                        break;
                    case k_UploadOnly:
                        UploadOnly();
                        break;
                }
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // TBD
            }
#endif
        }

        [HideInInspector]
        public bool IsBuildIOS = false;
        private void buildIOS()
        {
#if UNITY_EDITOR
            IsBuildIOS = true;
#endif
        }

        [HideInInspector]
        public bool IsBuildAndroid = false;
        private void buildAndroid()
        {
#if UNITY_EDITOR
            IsBuildAndroid = true;
#endif
        }

        private void SelectAdjustGO()
        {
#if UNITY_EDITOR && ENABLE_ADJUST
            Selection.activeObject = GameObject.FindObjectOfType<Adjust>();
#endif
        }

        public void SelectAppMetricaGO()
        {
#if UNITY_EDITOR && ENABLE_APPMETRICA
            Selection.activeObject = Managers.Instance.GetComponentInChildren<AppMetrica>();
#endif
        }

        public void SelectFacebookSettings()
        {
#if UNITY_EDITOR && ENABLE_FACEBOOK
            Selection.activeObject = Facebook.Unity.Settings.FacebookSettings.Instance;//  GameObject.FindObjectOfType<Facebook.Unity.Settings.FacebookSettings>();
#endif
        }

        public bool VerifyQualitySettings()
        {
            return QualitySettings.GetQualityLevel() < 2;
        }

        private const int k_SelectStorageAndRemoteOrder = -3;
        [HorizontalGroup("1"), Button(ButtonSizes.Large), PropertyOrder(k_SelectStorageAndRemoteOrder)]
        public void selectRemoteConfig()
        {
#if UNITY_EDITOR
            Selection.activeGameObject = Managers.Instance.RemoteSettings.gameObject;
#endif
        }

        [HorizontalGroup("1"), Button(ButtonSizes.Large), PropertyOrder(k_SelectStorageAndRemoteOrder)]
        public void selectStorageManager()
        {
#if UNITY_EDITOR
            Selection.activeGameObject = Managers.Instance.Storage.gameObject;
#endif
        }

        //        public void BrowseSavePath()
        //        {
        //#if UNITY_EDITOR
        //            BuildSavePath = EditorUtility.SaveFolderPanel("Path to Save Build", BuildSavePath, @"D:/Dropbox/Games");
        //#endif
        //        }

        [HideInInspector] public bool IsFacebookAppIdSave = false;
        public void SetSignAPK()
        {
#if UNITY_EDITOR
            if (SignAPK)
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = $"{KeyStorePath}/{KeyStoreName}.keystore";
                PlayerSettings.Android.keystorePass = KeyStorePass;
                PlayerSettings.Android.keyaliasName = KeyaliasName;
                PlayerSettings.Android.keyaliasPass = KeyaliasPass;
            }
            else
            {
                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.Android.keystoreName = string.Empty;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasName = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
            }
#endif
        }

        public void DeleteFilesAndFolders(string[] i_Paths)
        {
#if UNITY_EDITOR
            bool deleted = false;

            foreach (var path in i_Paths)
            {
                string pathToDelete = Application.dataPath + path;
                if (Directory.Exists(pathToDelete))
                {
                    Directory.Delete(pathToDelete, true);

                    Debug.LogError($"Deleted Folder {pathToDelete}");

                    deleted = true;
                }
                else if (File.Exists(pathToDelete))
                {
                    File.Delete(pathToDelete);

                    Debug.LogError($"Deleted File {pathToDelete}");

                    deleted = true;
                }
            }

            if (!deleted)
            {
                Debug.LogWarning("Nothing to delete!");
            }
            else
            {
                AssetDatabase.Refresh();
            }
#endif
        }

        [HideInInspector, NonSerialized]
        public string[] MAXPathsToDelete =
        {
            "/MaxSdk",
            "/MaxSdk.meta",
            "/Plugins/Android/MaxMediationGoogle",
            "/Plugins/Android/res",
            "/Plugins/Android/MaxMediationGoogle",
            "/Plugins/Android/MaxMediationGoogle.meta",
            "/Plugins/Android/MaxMediationGoogle.androidlib",
            "/Plugins/Android/MaxMediationGoogle.androidlib.meta",
            "/Plugins/Android/res",
            "/Plugins/Android/res.meta"
        };

        [HideInInspector, NonSerialized]
        public string[] IronSourcePathsToDelete =
        {
            "/IronSource",
            "/IronSource.meta",
            "/Plugins/Android/IronSource",
            "/Plugins/iOS/IronSource"
        };

        [HideInInspector, NonSerialized]
        public string[] AdsPathsToDelete =
        {
            "/_KobGamesSDK_Slim_Ads",
            "/_KobGamesSDK_Slim_Ads.meta",
            "/MaxSdk",
            "/MaxSdk.meta",
            "/IronSource",
            "/IronSource.meta",
            "/Plugins/Android/IronSource",
            "/Plugins/iOS/IronSource",
            "/Plugins/Android/MaxMediationGoogle",
            "/Plugins/Android/MaxMediationGoogle.meta",
            "/Plugins/Android/MaxMediationGoogle.androidlib",
            "/Plugins/Android/MaxMediationGoogle.androidlib.meta",
            "/Plugins/Android/res",
            "/Plugins/Android/res.meta"
        };

        public void DeleteAdsPlugins()
        {
            if (Application.isEditor)
            {
#if !ENABLE_ADS
                DeleteFilesAndFolders(AdsPathsToDelete);
                DeleteFilesAndFolders(MAXPathsToDelete);
                DeleteFilesAndFolders(IronSourcePathsToDelete);
                Managers.Instance.Ads.RemoveAdsGameObjectsLeftOvers();
#else
                // Ads Enabled but MAX isn't defined
#if !ENABLE_MAX && UNITY_EDITOR
                //DeleteFilesAndFolders(MAXPathsToDelete);
                GameSettings.Instance.Sync.DeleteMAXFiles();
#endif

                // Ads Enabled but IronSource isn't defined
#if !ENABLE_IRONSOURCE && UNITY_EDITOR
                //DeleteFilesAndFolders(IronSourcePathsToDelete);
                GameSettings.Instance.Sync.DeleteIronSourceFiles();
#endif

                Debug.LogError("DeleteAdsPlugins() - Error: Won't delete anything since 'ENABLE_ADS' is still defined");
#endif
            }
        }

        [HideInInspector, NonSerialized]
        public string[] IAPPathsToDelete =
        {
            "/Plugins/UDP",
            "/Plugins/UDP.meta",
            "/Plugins/UnityPurchasing",
            "/Plugins/UnityPurchasing.meta",
            "/Resources/BillingMode.json"
        };

        public void DeleteIAPPlugins(bool i_IsForce = false)
        {
#if UNITY_EDITOR
            {
#if !UNITY_PURCHASING
                i_IsForce = true;
#endif
                if (i_IsForce)
                {
#if UNITY_PURCHASING
                    foreach (var iapManager in GameObject.FindObjectsOfType<iAPManager>(true))
                    {
                        iapManager.RemoveIAPListener();
                    }
                    Managers.Instance.iAPManager.RemoveIAPListener();
                    Managers.Instance.ApplyPrefabInstance();
#endif

                    UnityEditor.PackageManager.Client.Remove("com.unity.purchasing");
                    DeleteFilesAndFolders(IAPPathsToDelete);
                }
                else
                {
                    Debug.LogError("DeleteIAPPlugins() - Error: Won't delete anything since 'UNITY_PURCHASING' is still defined");
                }
            }
#endif
        }

        public void DeleteBepicUneededFiles()
        {
#if UNITY_EDITOR && ENABLE_BEPIC
            GameSettings.Instance.Analytics.DeleteAppsFlyerPlugins();
            GameSettings.Instance.Analytics.DeleteFacebookAnalyticsPlugins();
#endif
        }

        //private string m_PathToProjectUpdateBatchFile = "D:\\GameDev\\KobGameSSDK-Slim\\Assets\\_Project_Specific_Folder\\ReCopyProjectFolders.bat";
        private string m_PathToProjectUpdateBatchFile = "/_KobGamesSDK_Slim/BAT Scripts/ReCopyProjectFolders.bat";
        public void ReCopyProject()
        {
#if UNITY_EDITOR
            Debug.LogError(Application.dataPath + m_PathToProjectUpdateBatchFile);
            System.Diagnostics.Process.Start(Application.dataPath + m_PathToProjectUpdateBatchFile);
            //System.Diagnostics.Process.Start(m_PathToProjectUpdateBatchFile);
#endif
        }

#if UNITY_EDITOR
        public string CurrentSimulateDevice { get { return "Simulate Device: " + Crystal.SafeArea.Sim.ToString(); } }

        [ShowInInspector] public Crystal.SafeArea[] SafeAreas { get { return GameObject.FindObjectsOfType<Crystal.SafeArea>(); } }

        [Button(Name = "$CurrentSimulateDevice")]
        public void SimulateDevice()
        {
            Crystal.SafeArea.SimDevice[] devices = (Crystal.SafeArea.SimDevice[])Enum.GetValues(typeof(Crystal.SafeArea.SimDevice));
            int currentIndex = -1;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i] == Crystal.SafeArea.Sim)
                {
                    currentIndex = i;
                    break;
                }
            }
            currentIndex++;
            if (currentIndex >= devices.Length)
            {
                currentIndex = 0;
            }
            Crystal.SafeArea.Sim = devices[currentIndex];
            Debug.Log("SafeArea: " + Crystal.SafeArea.Sim);
        }
#endif

        public void UpdatePackageInManifest()
        {
            Utils.UpdateAndroidManifestPackageName(GetBundleIdentifierAndroid());

#if !ENABLE_BEPIC
            GameSettings.Instance.AdsMediation.SetMAXSDKKeyOnManifest();
            GameSettings.Instance.AdsMediation.SetAdMobAppIdOnManifest();

            SetFacebookAppId();
#endif
        }

        public void GetPackageInManifest()
        {
            Utils.GetAndroidManifestPackageName();

            SetFacebookAppId();
        }

#if UNITY_EDITOR
        [Button("Open Build Settings (Ctrl+Shift+G)")]
        public void OpenBuildSettings()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow, UnityEditor"));
        }

        [Button("Open Player Settings (Ctrl+W)")]
        public void OpenPlayerSettings()
        {
            Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
        }

        [Button("Open AndroidManifest.xml")]
        public void OpenAndroidManifest()
        {
            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(Constants.k_AndroidManifestPath, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);

            AssetDatabase.OpenAsset(obj);
        }

        private const string k_ProjectSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";
        private const string k_QualitySettingsAssetPath = "ProjectSettings/QualitySettings.asset";
        private const string k_TextMeshSeetingsAssetPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        private SerializedObject m_projectSettingsSO;
        private SerializedObject m_projectSettingsTextMeshSO;

        public void Load()
        {
            SetSplashAndIconSettings();

            var guids = AssetDatabase.FindAssets("t:scriptableobject TMP Settings");

            foreach (string guid in guids)
            {
                m_projectSettingsTextMeshSO = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)));
                //Debug.LogError(m_projectSettingsTextMeshSO);
                m_projectSettingsTextMeshSO.FindProperty("m_warningsDisabled").intValue = 1;
                m_projectSettingsTextMeshSO.ApplyModifiedProperties();
            }

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.iOS.appleDeveloperTeamID = SignIOSTeamID;

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, GetBundleIdentifierAndroid());
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, BundleIdentifier);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, BundleIdentifier);
            PlayerSettings.productName = GameName;
            PlayerSettings.bundleVersion = BuildVersion;
            PlayerSettings.Android.bundleVersionCode = BuildNumber;

            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
            PlayerSettings.gcIncremental = true;
            PlayerSettings.accelerometerFrequency = 0;
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.stripEngineCode = false;

            // Enable dynamic batching automatically for all OSs        
            m_projectSettingsSO = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(k_ProjectSettingsAssetPath)[0]);
            for (int i = 0; i < m_projectSettingsSO.FindProperty("m_BuildTargetBatching").arraySize; i++)
            {
                m_projectSettingsSO.FindProperty($"m_BuildTargetBatching.Array.data[{i}].m_DynamicBatching").boolValue = true;
            }

            m_projectSettingsSO.ApplyModifiedProperties();

#if ENABLE_FACEBOOK
            if (Facebook.Unity.Settings.FacebookSettings.AppIds.Count > 0 && FacebookAppId == string.Empty)
            {
                FacebookAppId = Facebook.Unity.Settings.FacebookSettings.AppIds[0];
            }
#endif

            if (Utils.GetAndroidManifestPackageName() != GetBundleIdentifierAndroid())
            {
                Debug.LogError($"Changing Manifest Package Name from '{Utils.GetAndroidManifestPackageName()}' to '{BundleIdentifier}'");
                UpdatePackageInManifest();
            }

            SetSignAPK();

            SetFacebookAppId();

            SetAppsMetricaAppId();

            SetAdjustAppId();

            SetQualitySettings();

            GameSettings.Instance.Analytics.Load();
            GameSettings.Instance.AdsMediation.Load();

            DOTweenSettings DOTWeenSettings = Resources.Load<DOTweenSettings>("DOTweenSettings");

            if (DOTWeenSettings != null &&
                (DOTWeenSettings.useSafeMode == false ||
                 DOTWeenSettings.storeSettingsLocation != DOTweenSettings.SettingsLocation.DOTweenDirectory ||
                 DOTWeenSettings.debugMode == false ||
                 DOTWeenSettings.debugStoreTargetId == false))
            {
                DOTWeenSettings.useSafeMode = true;
                DOTWeenSettings.safeModeOptions.logBehaviour = DG.Tweening.Core.Enums.SafeModeLogBehaviour.Error;
                DOTWeenSettings.debugMode = true;
                DOTWeenSettings.debugStoreTargetId = true;
                DOTWeenSettings.storeSettingsLocation = DOTweenSettings.SettingsLocation.DOTweenDirectory;

                EditorUtility.SetDirty(DOTWeenSettings);
                AssetDatabase.Refresh();
                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(DOTWeenSettings), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);

                Debug.LogError("DOTWeen: Updated Settings");
            }

#if !ENABLE_ADS
            GameSettings.Instance.General.DeleteAdsPlugins();
#endif

            GameConfig.Instance.OnInspectorInit();
        }

        private void SetQualitySettings()
        {
            if (EnforceQualitySettings)
            {
                QualitySettings.masterTextureLimit = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.antiAliasing = 0;

                QualitySettings.shadowResolution = ShadowResolution.Medium;
            }
        }
#endif

        public bool UpdateFacebookManifest = false;
        public void SetFacebookAppId()
        {
#if UNITY_EDITOR && ENABLE_FACEBOOK
            if (Facebook.Unity.Settings.FacebookSettings.AppLabels.Count > 0)
            {
                Facebook.Unity.Settings.FacebookSettings.AppLabels[0] = GameName;

                if (FacebookAppId != string.Empty)
                {
                    Facebook.Unity.Settings.FacebookSettings.AppIds = new List<string>() { FacebookAppId };
                }

                EditorUtility.SetDirty(Facebook.Unity.Settings.FacebookSettings.Instance);
                UpdateFacebookManifest = true;
            }

            IsFacebookAppIdSave = true;

            //Save();
#endif
        }

        public void SetAppsMetricaAppId()
        {
#if UNITY_EDITOR && ENABLE_APPMETRICA
            if (GameSettings.Instance.General.AppMetricaId != string.Empty)
            {
                AppMetrica appMetrica = Managers.Instance.GetComponentInChildren<AppMetrica>();

                if (appMetrica == null)
                {
                    Managers.Instance.AddAppMetricaToManagers();
                    appMetrica = Managers.Instance.GetComponentInChildren<AppMetrica>();
                }

                if (appMetrica._ApiKey != GameSettings.Instance.General.AppMetricaId)
                {
                    appMetrica._SessionTimeoutSec = 300;
                    appMetrica._ApiKey = GameSettings.Instance.General.AppMetricaId;
                    appMetrica._LocationTracking = false;
                    EditorUtility.SetDirty(appMetrica.gameObject);
                    Managers.Instance.gameObject.ApplyToPrefab();
                }
            }
#endif
        }

#if UNITY_EDITOR && ENABLE_ADJUST
        private Adjust getOrCreateAdjustGO()
        {
            var adjustGO = GameObject.FindObjectOfType<Adjust>();
            if (adjustGO == null)
            {
                var adjustAsset = AssetDatabase.LoadAssetAtPath<Adjust>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("adjust t:Prefab")[0]));
                adjustGO = PrefabUtility.InstantiatePrefab(adjustAsset) as Adjust;

                Debug.LogError("Adjust: added prefab to Scene");
            }

            return adjustGO;
        }
#endif

        private void OnAdjustIdChanged()
        {
#if UNITY_EDITOR && ENABLE_ADJUST
            var adjust = getOrCreateAdjustGO();

            adjust.appToken = AdjustId;
            adjust.gameObject.ApplyToPrefab();

            Debug.Log($"Adjust: changed AdjustId to: {adjust.appToken}");
#endif
        }

        public void SetAdjustAppId()
        {
#if UNITY_EDITOR && ENABLE_ADJUST
            var adjust = getOrCreateAdjustGO();

            if (AdjustId != adjust.appToken)
            {
                AdjustId = adjust.appToken;

                Debug.Log($"Adjust: found a different id on Adjust prefab, setting AdjustId to: {AdjustId}");
            }
#endif
        }

        private void SaveBuild()
        {
#if UNITY_EDITOR
            Save();

#if ENABLE_GAMEANALYTICS
            for (int i = 0; i < GameAnalyticsSDK.GameAnalytics.SettingsGA.Platforms.Count; ++i)
            {
                if (GameAnalyticsSDK.GameAnalytics.SettingsGA.UsePlayerSettingsBuildNumber)
                {
                    if (GameAnalyticsSDK.GameAnalytics.SettingsGA.Platforms[i] == RuntimePlatform.Android ||
                        GameAnalyticsSDK.GameAnalytics.SettingsGA.Platforms[i] == RuntimePlatform.IPhonePlayer)
                    {
                        GameAnalyticsSDK.GameAnalytics.SettingsGA.Build[i] = PlayerSettings.bundleVersion;
                    }
                }
            }

            EditorUtility.SetDirty(GameAnalyticsSDK.GameAnalytics.SettingsGA);
#endif
#endif
        }

        private void Save()
        {
#if UNITY_EDITOR
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, GetBundleIdentifierAndroid());
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, BundleIdentifier);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, BundleIdentifier);
            PlayerSettings.companyName = "KobGames";
            PlayerSettings.productName = GameName;
            PlayerSettings.bundleVersion = BuildVersion;
            PlayerSettings.Android.bundleVersionCode = BuildNumber;
            PlayerSettings.iOS.buildNumber = BuildNumber.ToString();
            PlayerSettings.iOS.appleEnableAutomaticSigning = AutoSign;
            PlayerSettings.iOS.appleDeveloperTeamID = AutoSign ? SignIOSTeamID : string.Empty;

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            SetSplashAndIconSettings();

            Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.Value = BuildVersion;
            Managers.Instance.RemoteSettings.FirebaseRemoteConfig.FlushDeviceConfig = BuildVersion;
            Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ConfigVersion.SetToDefaultValue(BuildVersion);

            GooglePlayId = GetBundleIdentifierAndroid();

#if ENABLE_GAv4
        if (GameSettings.Instance.Analytics.GAv4.bundleIdentifier != GameSettings.Instance.General.BundleIdentifier)
        {
            GameSettings.Instance.Analytics.SetGAv4Params();
        }
#endif

            SetAppsMetricaAppId();

            if (!EditorApplication.isPlaying && PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(Managers.Instance.gameObject))
            {
                EditorUtility.SetDirty(Managers.Instance);
                Managers.Instance.ApplyPrefabInstance();
                //PrefabUtility.ApplyPrefabInstance(Managers.Instance.gameObject, InteractionMode.UserAction);
            }
#endif
        }

        public void OpenAppStoreURL()
        {
            Utils.OpenUrlStore(GooglePlayId, AppStoreId);
        }

        public void SetSplashAndIconSettings()
        {
#if UNITY_EDITOR
            if (UpdateSplash)
            {
                var splashImageTexture = SplashImage != null ? SplashImage.texture : null;
                var iconImageTexture = Icon != null ? Icon.texture : null;

                PlayerSettings.SplashScreen.show = false;
                PlayerSettings.SplashScreen.showUnityLogo = false;
                PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFit;

                // Set Android bg + scale
                m_projectSettingsSO = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(k_ProjectSettingsAssetPath)[0]);
                m_projectSettingsSO.FindProperty("androidSplashScreen").objectReferenceValue = splashImageTexture;
                m_projectSettingsSO.FindProperty("iOSLaunchScreenFillPct").floatValue = 100;
                m_projectSettingsSO.FindProperty("iOSLaunchScreeniPadFillPct").floatValue = 50;
                m_projectSettingsSO.ApplyModifiedProperties();

                {
                    PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { iconImageTexture });
                    PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] { iconImageTexture });
                    PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { iconImageTexture });
                }

                // iOS
                {
                    PlayerSettings.SplashScreen.background = SplashImage;
                    PlayerSettings.SplashScreen.backgroundPortrait = SplashImage;
                    PlayerSettings.iOS.SetLaunchScreenImage(splashImageTexture, iOSLaunchScreenImageType.iPhonePortraitImage);
                    PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
                    PlayerSettings.iOS.SetLaunchScreenImage(splashImageTexture, iOSLaunchScreenImageType.iPadImage);
                    PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
                }
            }
#endif
        }
        
        #region  Culture Info
        [Title("Culture")]
        public bool SetSpecificCulture = true;
        [ShowIf(nameof(SetSpecificCulture)), ValueDropdown(nameof(GetCultureList))]
        public string CultureName = "en-US";

        private ValueDropdownList<string> GetCultureList()
        { 
            ValueDropdownList<string> cultures = new ValueDropdownList<string>();
            CultureInfo.GetCultures(CultureTypes.AllCultures).ForEach2(x => cultures.Add(x.Name));
            return cultures;
        }
        #endregion
    }

    [Serializable]
    public class AdsMediationEditor
    {
        [Title("AdsMediation")]
        public eMediationNetworks MediationNetworks = eMediationNetworks.None;
        public ePPNetworks PPNetworks = ePPNetworks.None;
        [PropertyOrder(0), ShowInInspector]
        public int MaxContinueAmount { get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.MaxContinueAmount.Value; } }
        [PropertyOrder(0), ShowIf(nameof(IsIronSourceEnabled))]
        public string IronSourceAppleID = "None";
        [PropertyOrder(0), ShowIf(nameof(IsIronSourceEnabled))]
        public string IronSourceAndroidID = "None";

        public string IronSourceAppId { get { return Application.platform == RuntimePlatform.Android ? IronSourceAndroidID : IronSourceAppleID; } }

        // k - i8Iz-ON-o5T8cn3iA6AXHWEd5MiybWDIfVFeudcgi5qKsu21-DZGlouq9bnl7-2nUIPNw12d6szzk_-iCTzxKs
        // a - 6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR
        private const string k_KobyMAXSDK = "i8Iz-ON-o5T8cn3iA6AXHWEd5MiybWDIfVFeudcgi5qKsu21-DZGlouq9bnl7-2nUIPNw12d6szzk_-iCTzxKs";
        private const string k_A_MAXSDK = "6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR";
        private const string k_Mo_MAXSDK = "EghIU8MPrS5yShsp12hFZTN2T0sysd9F-mpr9MgBKNplGKZGGPVnpldaKrIrxV3vqXdmcNDl2FyCHT-KIC2hC2";

        private ValueDropdownList<string> MAXSDKKeysValues = new ValueDropdownList<string>()
        {
            {$"Koby - {k_KobyMAXSDK}", k_KobyMAXSDK },
            {$"A- {k_A_MAXSDK}", k_A_MAXSDK },
            {$"Mo- {k_Mo_MAXSDK}", k_Mo_MAXSDK }
        };

        [PropertyOrder(0), ValueDropdown(nameof(MAXSDKKeysValues)), LabelText("MAX SDK Key"), InlineButton(nameof(SetMAXSDKKeyOnManifest), "Set"), InlineButton(nameof(FetchMAXSDKKeyFromManifest), "Fetch"), ShowIf(nameof(IsMAXEnabled))]
        public string MAXSDKKey = k_KobyMAXSDK;
        [PropertyOrder(0), LabelText("AdMob AppId Android"), InfoBox("Must be filled!", nameof(m_IsAdMobAppIdAndroidEmpty), InfoMessageType = InfoMessageType.Error), DelayedProperty, OnValueChanged(nameof(SetAdMobAppIdOnManifest)), InlineButton(nameof(FetchAdMobAppIdOnManifest), "Fetch")]
        public string AdMobAppIdAndroid;
        [PropertyOrder(0), LabelText("AdMob AppId iOS"), InfoBox("Must be filled!", nameof(m_IsAdMobAppIdIosEmpty), InfoMessageType = InfoMessageType.Error)]
        public string AdMobAppIdIos;

        [ShowIf(nameof(IsMAXEnabled)), BoxGroup("IOS")] public string MAXBannerAdUnitIdIOS = "[Please Set]";
        [ShowIf(nameof(IsMAXEnabled)), BoxGroup("IOS")] public string MAXInterstitialAdUnitIdIOS = "[Please Set]";
        [ShowIf(nameof(IsMAXEnabled)), BoxGroup("IOS")] public string MAXRewardVideoAdUnitIdIOS = "[Please Set]";
        [ShowIf(nameof(IsMAXEnabled)), BoxGroup("Android")] public string MAXBannerAdUnitIdAndroid = "[Please Set]";
        [ShowIf(nameof(IsMAXEnabled)), BoxGroup("Android")] public string MAXInterstitialAdUnitIdAndroid = "[Please Set]";
        [ShowIf(nameof(IsMAXEnabled)), BoxGroup("Android")] public string MAXRewardVideoAdUnitIdAndroid = "[Please Set]";

        private bool m_IsAdMobAppIdAndroidEmpty { get { return AdMobAppIdAndroid == string.Empty; } }
        private bool m_IsAdMobAppIdIosEmpty { get { return AdMobAppIdIos == string.Empty; } }

        //private SDKVersions _SDKVersionsList;
        //private SDKVersions _SDKVersionsListLocal;

        //[PropertyOrder(1), ShowInInspector, InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.CompletelyHidden)]
        //public SDKVersions SDKVersionsList
        //{
        //    get { _SDKVersionsList = Resources.Load<SDKVersions>("SDKVersions"); return _SDKVersionsList; }
        //    set { _SDKVersionsList = value; }
        //}

        public bool IsIronSourceEnabled
        {
            get
            {
                return MediationNetworks == eMediationNetworks.ironSource;
            }
            set
            {
                MediationNetworks = eMediationNetworks.ironSource;
            }
        }

        public bool IsMAXEnabled
        {
            get
            {
                return MediationNetworks == eMediationNetworks.MAX;
            }
            set
            {
                MediationNetworks = eMediationNetworks.MAX;
            }
        }

        public bool IsSupereraEnabled
        {
            get
            {
                return MediationNetworks == eMediationNetworks.SUPERERA;
            }
            set
            {
                MediationNetworks = eMediationNetworks.SUPERERA;
            }
        }

        public bool IsBepicEnabled
        {
            get
            {
                return PPNetworks == ePPNetworks.BEPIC;
            }
            set
            {
                PPNetworks = ePPNetworks.BEPIC;
            }
        }

        public bool IsKobicEnabled
        {
            get
            {
                return PPNetworks == ePPNetworks.KOBIC;
            }
            set
            {
                PPNetworks = ePPNetworks.KOBIC;
            }
        }

        public void FetchMAXSDKKeyFromManifest()
        {
            var fetchResult = Utils.FetchAndroidManifestData("android:name='applovin.sdk.key'");

            if (!fetchResult.IsNullOrEmpty())
            {
                MAXSDKKey = fetchResult;
            }
        }

        public void SetMAXSDKKeyOnManifest()
        {
            Utils.SaveAndroidManifestData(MAXSDKKey, "android:name='applovin.sdk.key'");
        }

        public void FetchAdMobAppIdOnManifest()
        {
            var fetchResult = Utils.FetchAndroidManifestData("android:name='com.google.android.gms.ads.APPLICATION_ID'");

            if (!fetchResult.IsNullOrEmpty())
            {
                AdMobAppIdAndroid = fetchResult;
            }
        }

        public void SetAdMobAppIdOnManifest()
        {
            Utils.AddManifestNode("com.google.android.gms.ads.APPLICATION_ID", "None");
            Utils.SaveAndroidManifestData(AdMobAppIdAndroid, "android:name='com.google.android.gms.ads.APPLICATION_ID'");
        }

        //[Button]
        [Obsolete]
        public void AddMAXNetworkSecurityConfig()
        {
            // Utils.AddManifestAttributeNew("networkSecurityConfig", "@xml/network_security_config");
        }

        //[Button]
        public void RemoveMAXNetworkSecurityConfig()
        {
            Utils.RemoveManifestAttributeNew("android:networkSecurityConfig");
        }

        public void Load()
        {
#if UNITY_EDITOR && ENABLE_ADS && ENABLE_IRONSOURCE
            if (IsIronSourceEnabled)
            {
                SetAdMobAppIdOnManifest();
            }
#endif

#if UNITY_EDITOR && ENABLE_ADS && ENABLE_MAX
            if (IsMAXEnabled)
            {
                Utils.RemoveManifestAttribute("com.applovin.adview.AppLovinInterstitialActivity");
                Utils.RemoveManifestAttribute("com.applovin.adview.AppLovinConfirmationActivity");
                //Utils.RemoveManifestAttribute("com.google.android.gms.ads.APPLICATION_ID");
                Utils.RemoveManifestAttribute("com.google.android.gms.version");

                AddMAXNetworkSecurityConfig();

                Managers.Instance.Ads.AddMaxMediationPrefab();

                Managers.Instance.Ads.RemoveBepicMediationPrefab();

                if (!AppLovinSettings.Instance.QualityServiceEnabled ||
                    AppLovinSettings.Instance.SdkKey != MAXSDKKey)
                {
                    AppLovinSettings.Instance.QualityServiceEnabled = true;
                    AppLovinSettings.Instance.SdkKey = MAXSDKKey;
                    UnityEditor.EditorUtility.SetDirty(AppLovinSettings.Instance);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
#endif

#if UNITY_EDITOR && ENABLE_ADS && ENABLE_BEPIC
        if (IsBepicEnabled)
        {
            Managers.Instance.Ads.AddBepicMediationPrefab();
            Managers.Instance.Ads.AddTestSuiteCanvasPrefab();

            Managers.Instance.Ads.RemoveMaxMediationPrefab();
        }
#endif

            //#if UNITY_EDITOR && !ENABLE_ADS
            //            Managers.Instance.Ads.RemoveMaxMediationPrefab();
            //            Managers.Instance.Ads.RemoveIronSourceMediationPrefab();
            //#endif

#if UNITY_EDITOR && ((ENABLE_ADS && !ENABLE_MAX) || !ENABLE_ADS)
            RemoveMAXNetworkSecurityConfig();
#endif
        }
    }

    [Serializable]
    public class RemoteSettingsEditor
    {
        [Title("RemoteSettings")]
        [PropertyOrder(2), InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden), ShowInInspector, HideReferenceObjectPicker, HideLabel]
        public RemoteSettingsManager RemoteSettingsManager
        {
            get { return Managers.Instance.RemoteSettings; }
            set { Managers.Instance.RemoteSettings = value; }
        }

#if UNITY_EDITOR
        [PropertyOrder(2), Button("Select RemoteSettingsManager")]
        public void SelectRemoteSettingsManager()
        {
            //Debug.LogError(Managers.Instance.Global.RemoteSettings.GameRemoteSettings.IsBannerEnabled);

            Selection.activeObject = GameObject.FindObjectOfType<RemoteSettingsManager>();
        }
#endif
    }

    [Serializable]
    public class AnalyticsEditor
    {
        [Title("Analytics Manager"), ShowInInspector, InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden), HideReferenceObjectPicker, HideLabel]
        public AnalyticsManager AnalyticsManager { get { return Managers.Instance.Analytics; } set { Managers.Instance.Analytics = value; } }

#if ENABLE_GAv4
    [HideInInspector]
    public GoogleAnalyticsV4 GAv4 { get { return GameObject.FindObjectOfType<GoogleAnalyticsV4>(); } }
    [Title("Google Analytics")]
    [PropertyOrder(5), ShowInInspector, OnValueChanged(nameof(SetGAv4Params))]
    public string AndroidTrackerId; //{ get { return m_GAv4 != null ? m_GAv4.androidTrackingCode : string.Empty; } set { m_GAv4.androidTrackingCode = value; } }
    [PropertyOrder(5), ShowInInspector, OnValueChanged(nameof(SetGAv4Params))]
    public string iOSTrackerId; //{ get { return m_GAv4 != null ? m_GAv4.IOSTrackingCode : string.Empty; } set { m_GAv4.IOSTrackingCode = value; } }
    [PropertyOrder(5), Button("Select GAv4")]
    public void SelectGAv4()
    {
#if UNITY_EDITOR
        Selection.activeObject = GAv4;
#endif
    }
#endif

#if ENABLE_GAMEANALYTICS
        [PropertyOrder(4), OnValueChanged(nameof(Load))]
        public bool SubmitFPS = true;
        [PropertyOrder(4), ShowInInspector, ListDrawerSettings(Expanded = true), OnValueChanged(nameof(SetCustomDimension01), true)]
        public List<string> GameAnalyticsCustomDimensions01 = new List<string>() { "ConfigVersion_0.1", "ConfigVersion_0.1.0", "ConfigVersion_0.2", "ConfigVersion_0.2.0", "ConfigVersion_0.3", "ConfigVersion_0.3.0" };
        [PropertyOrder(4), ShowInInspector, ListDrawerSettings(Expanded = true), OnValueChanged(nameof(SetCustomDimension02), true)]
        public List<string> GameAnalyticsCustomDimensions02 = new List<string>() { "LevelComplete_1", "LevelComplete_2", "LevelComplete_3", "LevelComplete_4", "LevelComplete_5", "LevelComplete_10" };
        [PropertyOrder(4), ShowInInspector, ListDrawerSettings(Expanded = true), OnValueChanged(nameof(SetCustomDimension03), true)]
        public List<string> GameAnalyticsCustomDimensions03 = new List<string>() { };
#endif

        [PropertyOrder(4), Button]
        public void DeleteAnalyticsPlugins()
        {
            tryDeleteAppsFlyerPlugins();
            tryDeleteGoogleAnalytics();
            tryDeleteFirebaseAnalytics();
            tryDeleteGameAnalytics();
            tryDeleteFacebookPlugins();
        }

        [PropertyOrder(4), Button]
        public void DeleteAppsFlyerPlugins()
        {
            tryDeleteAppsFlyerPlugins();
        }

        [PropertyOrder(4), Button]
        public void DeleteFirebaseAnalyticsPlugins()
        {
            Managers.Instance.Analytics.AnalyticsServices &= ~eAnalyticsServices.Firebase;

            tryDeleteFirebaseAnalytics(true);
        }

        [PropertyOrder(4), Button]
        public void DeleteGoogleAnalyticsPlugins()
        {
            Managers.Instance.Analytics.AnalyticsServices &= ~eAnalyticsServices.GoogleAnalytics;

            tryDeleteGoogleAnalytics();
        }

        [PropertyOrder(4), Button]
        public void DeleteFacebookAnalyticsPlugins()
        {
            Managers.Instance.Analytics.AnalyticsServices &= ~eAnalyticsServices.Facebook;

            tryDeleteFacebookPlugins();
        }

        [NonSerialized]
        private string[] m_AnalyticsGameAnalyticsPathsToDelete =
        {
            "/_KobGamesSDK_Slim/_3rd_Party/Analytics/GameAnalytics",
            "/_KobGamesSDK_Slim/_3rd_Party/Analytics/Gizmos",
            "/Plugins/Android/gameanalytics-imei.jar",
            "/Plugins/Android/gameanalytics.aar",
            "/Plugins/Android/unity_gameanalytics.jar",
            "/Plugins/Android/GameAnalytics/gameanalytics-imei.jar",
            "/Plugins/Android/GameAnalytics/gameanalytics.aar",
            "/Plugins/Android/GameAnalytics/unity_gameanalytics.jar",
            "/Plugins/iOS/GameAnalytics.h",
            "/Plugins/iOS/GameAnalyticsUnity.m",
            "/Plugins/iOS/libGameAnalytics.a"
        };

        private void tryDeleteGameAnalytics()
        {
#if UNITY_EDITOR
            bool anyDeleted = false;

            if (!AnalyticsManager.IsGameAnalyticsEnabled)
            {
                foreach (var path in m_AnalyticsGameAnalyticsPathsToDelete)
                {
                    anyDeleted = Utils.RemoveFileOrDirectoryRelativePath(path);
                }
            }

            AssetDatabase.Refresh();

            if (!anyDeleted)
            {
                Debug.LogError($"GameAnalytics Analytics: Nothing Deleted out of {m_AnalyticsGameAnalyticsPathsToDelete.Length} files/directories");
            }
#endif
        }


        [NonSerialized]
        private string[] m_AnalyticsFirebasePathsToDelete =
        {
            "/Plugins/x86_64",
            "/Plugins/Android/Firebase",
            "/Plugins/iOS/Firebase",
            "/Editor Default Resources/CrashlyticsSettings.asset",
            "/Editor Default Resources/Firebase"
        };


        private void tryDeleteFirebaseAnalytics(bool i_Force = false)
        {
#if UNITY_EDITOR
            bool anyDeleted = false;

            if (!AnalyticsManager.IsFirebaseAnalyticsEnabled || i_Force)
            {
                foreach (var path in m_AnalyticsFirebasePathsToDelete)
                {
                    anyDeleted = Utils.RemoveFileOrDirectoryRelativePath(path);
                }
            }

            AssetDatabase.Refresh();

            GameSettings.StartRemovingFirebasePackages();

            if (!anyDeleted)
            {
                Debug.LogError($"Firebase Analytics: Nothing Deleted out of {m_AnalyticsGAv4PathsToDelete.Length} files/directories");
            }
#endif
        }

        [NonSerialized]
        private string[] m_AnalyticsGAv4PathsToDelete =
        {
            "/Plugins/GoogleAnalyticsV4",
            "/_KobGamesSDK_Slim/_3rd_Party/Analytics/GoogleAnalytics",
            "/Plugins/iOS/libGoogleAnalyticsServices.a",
            "/Plugins/iOS/GAI.h",
            "/Plugins/iOS/GAIDictionaryBuilder.h",
            "/Plugins/iOS/GAIEcommerceFields.h",
            "/Plugins/iOS/GAIEcommerceProduct.h",
            "/Plugins/iOS/GAIEcommerceProductAction.h",
            "/Plugins/iOS/GAIEcommercePromotion.h",
            "/Plugins/iOS/GAIFields.h",
            "/Plugins/iOS/GAIHandler.h",
            "/Plugins/iOS/GAIHandler.m",
            "/Plugins/iOS/GAILogger.h",
            "/Plugins/iOS/GAITrackedViewController.h",
            "/Plugins/iOS/GAITracker.h"
        };

        private void tryDeleteGoogleAnalytics()
        {
#if UNITY_EDITOR
            bool anyDeleted = false;

            if (!AnalyticsManager.IsGoogleAnalyticsEnabled)
            {
                foreach (var path in m_AnalyticsGAv4PathsToDelete)
                {
                    anyDeleted = Utils.RemoveFileOrDirectoryRelativePath(path);
                }
            }

            AssetDatabase.Refresh();

            if (!anyDeleted)
            {
                Debug.LogError($"GoogleAnalytics: Nothing Deleted out of {m_AnalyticsGAv4PathsToDelete.Length} files/directories");
            }
            else
            {
                var gav4 = Managers.Instance.Analytics.transform.FindDeepChild("GAv4");

                if (gav4 != null)
                {
                    string myPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Managers.Instance.gameObject);
                    //Debug.LogError(myPath);

                    PrefabUtility.UnpackPrefabInstance(Managers.Instance.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);

                    AnalyticsManagerBase.DestroyImmediate(gav4.gameObject);

                    PrefabUtility.SaveAsPrefabAssetAndConnect(Managers.Instance.gameObject, myPath, InteractionMode.UserAction);
                }
            }
#endif
        }

        [NonSerialized]
        private string[] m_AnalyticsFacebookPathsToDelete =
        {
            "/_KobGamesSDK_Slim/_3rd_Party/FacebookSDK",
            "/_KobGamesSDK_Slim/_3rd_Party/FacebookSDK.meta",
            "/_Project_Specific_Folder/Resources/FacebookSDK",
            "/_Project_Specific_Folder/Resources/FacebookSDK.meta",
            "/Plugins/Android/FacebookSDK-android-wrapper-7.19.2.aar",
            "/Plugins/Android/FacebookSDK-android-wrapper-8.1.0.aar",
            "/Plugins/Android/FacebookSDK-android-wrapper-8.1.1.aar"
        };

        private void tryDeleteFacebookPlugins()
        {
#if UNITY_EDITOR
            bool anyDeleted = false;

            if (!AnalyticsManager.IsFacebookAnalyticsEnabled)
            {
                foreach (var path in m_AnalyticsFacebookPathsToDelete)
                {
                    anyDeleted = Utils.RemoveFileOrDirectoryRelativePath(path);
                }
            }

            AssetDatabase.Refresh();

            if (!anyDeleted)
            {
                Debug.LogError($"Facebook: Nothing Deleted out of {m_AnalyticsFacebookPathsToDelete.Length} files/directories");
            }
#endif
        }

        [NonSerialized]
        private string[] m_AnalyticsAppsFlyerPathsToDelete =
        {
            "/_KobGamesSDK_Slim/_3rd_Party/Analytics/AppsFlyer",
            "/_KobGamesSDK_Slim/_3rd_Party/Analytics/AppsFlyer.meta"
        };

        private void tryDeleteAppsFlyerPlugins()
        {
#if UNITY_EDITOR
            bool anyDeleted = false;

            if (!AnalyticsManager.IsAppsFlyerEnabled)
            {
                foreach (var path in m_AnalyticsAppsFlyerPathsToDelete)
                {
                    anyDeleted = Utils.RemoveFileOrDirectoryRelativePath(path);
                }
            }

            AssetDatabase.Refresh();

            if (!anyDeleted)
            {
                Debug.LogError($"AppsFlyer: Nothing Deleted out of {m_AnalyticsAppsFlyerPathsToDelete.Length} files/directories");
            }
#endif
        }

        public void SetCustomDimension01()
        {
#if UNITY_EDITOR && ENABLE_GAMEANALYTICS
            GameAnalyticsSDK.GameAnalytics.SettingsGA.CustomDimensions01 = GameAnalyticsCustomDimensions01;
            EditorUtility.SetDirty(GameAnalyticsSDK.GameAnalytics.SettingsGA);
#endif
        }

        public void SetCustomDimension02()
        {
#if UNITY_EDITOR && ENABLE_GAMEANALYTICS
            GameAnalyticsSDK.GameAnalytics.SettingsGA.CustomDimensions02 = GameAnalyticsCustomDimensions02;
            EditorUtility.SetDirty(GameAnalyticsSDK.GameAnalytics.SettingsGA);
#endif
        }

        public void SetCustomDimension03()
        {
#if UNITY_EDITOR && ENABLE_GAMEANALYTICS
            GameAnalyticsSDK.GameAnalytics.SettingsGA.CustomDimensions03 = GameAnalyticsCustomDimensions03;
            EditorUtility.SetDirty(GameAnalyticsSDK.GameAnalytics.SettingsGA);
#endif
        }

        public void Load()
        {
#if UNITY_EDITOR && ENABLE_GAMEANALYTICS
            GameAnalyticsSDK.GameAnalytics.SettingsGA.UsePlayerSettingsBuildNumber = true;
            GameAnalyticsSDK.GameAnalytics.SettingsGA.SubmitErrors = true;
            GameAnalyticsSDK.GameAnalytics.SettingsGA.SubmitFpsAverage = SubmitFPS;
            GameAnalyticsSDK.GameAnalytics.SettingsGA.SubmitFpsCritical = SubmitFPS;
            GameAnalyticsSDK.GameAnalytics.SettingsGA.InfoLogBuild = false;
            GameAnalyticsSDK.GameAnalytics.SettingsGA.InfoLogEditor = false;

            EditorUtility.SetDirty(GameAnalyticsSDK.GameAnalytics.SettingsGA);

            if (SubmitFPS)
            {
                if (Managers.Instance.GetComponentInChildren<GameAnalyticsSDK.GameAnalytics>() == null)
                {
                    var GO = new GameObject("GameAnalytics");
                    GO.AddComponent<GameAnalyticsSDK.GameAnalytics>();
                    GO.transform.SetParent(Managers.Instance.transform);
                    Managers.Instance.gameObject.ApplyToPrefab();
                }
            }
            else
            {
                var GO = Managers.Instance.GetComponentInChildren<GameAnalyticsSDK.GameAnalytics>();
                if (GO != null)
                {
                    Managers.Instance.gameObject.RemovePrefab(GO.gameObject);
                }
            }
#endif

#if UNITY_EDITOR && ENABLE_GAv4
        if (GAv4 != null)
        {
            bool requiredSettingGAv4Params = false;

            //AndroidTrackerId = m_GAv4.androidTrackingCode;
            //iOSTrackerId = m_GAv4.IOSTrackingCode;
            if (GAv4.androidTrackingCode != AndroidTrackerId)
            {
                GAv4.androidTrackingCode = AndroidTrackerId;

                requiredSettingGAv4Params = true;
            }

            if (GAv4.IOSTrackingCode != iOSTrackerId)
            {
                GAv4.IOSTrackingCode = iOSTrackerId;

                requiredSettingGAv4Params = true;
            }

            if (GAv4.bundleIdentifier != GameSettings.Instance.General.BundleIdentifier)
            {
                requiredSettingGAv4Params = true;
            }

            if (requiredSettingGAv4Params)
            {
                SetGAv4Params();
            }
        }
#endif
        }

        public void SetGAv4Params()
        {
#if UNITY_EDITOR && ENABLE_GAv4
        if (GAv4 != null)
        {
            GAv4.productName = PlayerSettings.productName;
            GAv4.bundleIdentifier = PlayerSettings.applicationIdentifier;
            GAv4.bundleVersion = string.Empty;// PlayerSettings.bundleVersion; //setting to empty will make it take app bundleversion
            GAv4.UncaughtExceptionReporting = true;
            GAv4.sendLaunchEvent = true;
            GAv4.sessionTimeout = 600;
            GAv4.androidTrackingCode = AndroidTrackerId;
            GAv4.IOSTrackingCode = iOSTrackerId;

            if (!EditorApplication.isPlaying)
            {
                //Debug.LogError("CHANGING");
                //Debug.LogError($"m_IsSaved: {m_IsSaved}");
                //m_IsSaved = true;

                EditorUtility.SetDirty(GAv4);
                Managers.Instance.ApplyPrefabInstance();
                //PrefabUtility.ApplyPrefabInstance(Managers.Instance.gameObject, InteractionMode.UserAction);
                

                //Why Commented: caused the Manager prefab to be applied incase GameSettings was chosen during play mode - dangerous!
                //PrefabUtility.ReplacePrefab(m_GAv4.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(m_GAv4.gameObject), ReplacePrefabOptions.ConnectToPrefab);

                //PrefabUtility.ReplacePrefab(Managers.Instance.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(Managers.Instance), ReplacePrefabOptions.ConnectToPrefab);

                //Sirenix.OdinInspector.Editor.OdinPrefabUtility.UpdatePrefabInstancePropertyModifications(Managers.Instance, true);
            }
        }
#endif
        }

#if ENABLE_GAMEANALYTICS
#if UNITY_EDITOR
        [PropertyOrder(1), Title("GameAnalytics"), Button("Select GameAnalytics")]
        public void SelectGameAnalytics()
        {
            Selection.activeObject = GameAnalyticsSDK.GameAnalytics.SettingsGA;
        }
#endif

        [ReadOnly, ShowInInspector, LabelText("GA Android"), PropertyOrder(2)]
        public string GAAndroidName
        {
            get
            {
                return getGAName("Android");
            }
        }

        [ReadOnly, ShowInInspector, LabelText("GA iOS"), PropertyOrder(2)]
        public string GAIOSName
        {
            get
            {
                return getGAName("iOS");
            }
        }

        private string getGAName(string i_Platform)
        {
            var ga = GameAnalyticsSDK.GameAnalytics.SettingsGA;
            string result = "None";

            int i = 0;
            foreach (var item in ga.SelectedPlatformGame)
            {
                if (item.ToLower().Contains(i_Platform.ToLower()))
                {
                    result = item + " @ " + ((ga.SelectedPlatformStudio.Count >= i) ? ga.SelectedPlatformStudio[i] : "None");
                }

                i++;
            }

            //int platformNum = i_Platform.ToLower() == "android" ? 0 : 1;

            //if (result == "None")
            //{
            //    if (ga.SelectedPlatformGame.Count > platformNum)
            //    {
            //        result = ga.SelectedPlatformGame[platformNum];
            //    }

            //    if (ga.SelectedPlatformStudio.Count > platformNum)
            //    {
            //        result += " @ " + ga.SelectedPlatformStudio[platformNum];
            //    }
            //}

            return result;
        }
#endif
    }

    [Serializable]
    public class RateUsEditor
    {
#if UNITY_EDITOR
        [PropertyOrder(2), Button("Select RemoteSettingsManager")]
        public void SelectRemoteSettingsManager()
        {
            Selection.activeObject = GameObject.FindObjectOfType<RemoteSettingsManager>();
        }
#endif

        [Title("Rate"), ListDrawerSettings(Expanded = true)]
        public List<int> RateUsLevels = new List<int>() { 6, 15, 25 };

        [ShowInInspector]
        public ePopupMode RateUsPopupMode = ePopupMode.LevelComplete;

        [ShowInInspector]
        public bool IsRateUsEnabled
        {
            get
            {
                return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsRateUsEnabled.Value;
            }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsRateUsEnabled.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }
    }

    [Serializable]
    public class SyncEditor
    {
#if UNITY_EDITOR

#if UNITY_EDITOR_WIN
    [NonSerialized, ShowInInspector, ReadOnly]
    public string SDKPath = "D:\\GameDev\\KobGameSSDK-Slim";
#else
        [NonSerialized, ShowInInspector, ReadOnly]
        public string SDKPath = "/Users/kobyle/GamesProjects/KobGamesSDK_Slim";
#endif
        [SerializeField, InlineButton(nameof(SetPathToThisProject), "SetPath")]
        public string DestPath = string.Empty;

        [Title("Sync Paths"), ShowInInspector, OnValueChanged(nameof(saveFilesListPathPrefab), true), InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public SyncFilePaths FilesListPath
        {
            get { return Resources.Load<SyncFilePaths>("SyncFilePaths"); }
            set { var syncFilePaths = Resources.Load<SyncFilePaths>("SyncFilePaths"); syncFilePaths = value; }
        }

        private void saveFilesListPathPrefab()
        {
            EditorUtility.SetDirty(FilesListPath);
        }

        [ReadOnly]
        public bool IsLockedReloadAssemblies = false;

        public void CopyAdsFolder(Action i_Callback)
        {
            EditorApplication.LockReloadAssemblies();

            IsLockedReloadAssemblies = true;

            EditorUtility.DisplayProgressBar("Copying Ads Folder", "Please hold...", .8f);

            try
            {
                string fullSourcePath = $"{SDKPath}/Assets/_KobGamesSDK_Slim_Ads";
                string fullDestPath = $"{DestPath}/Assets/_KobGamesSDK_Slim_Ads";
                if (Directory.Exists(fullSourcePath))
                {
                    if (Directory.Exists(fullDestPath))
                    {
                        Directory.Delete(fullDestPath, true);
                    }

                    Utils.CopyFolder(fullSourcePath, fullDestPath, i_Overwrite: true);
                }

                Debug.LogError($"Copied Ads Folder Done.");

                EditorApplication.UnlockReloadAssemblies();
                IsLockedReloadAssemblies = false;

                i_Callback.InvokeSafe();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message} Stack: {ex.StackTrace}");
            }

            EditorApplication.UnlockReloadAssemblies();
            IsLockedReloadAssemblies = false;

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        public void CopyAdsFolder()
        {
            EditorApplication.LockReloadAssemblies();

            IsLockedReloadAssemblies = true;

            EditorUtility.DisplayProgressBar("Copying Ads Folder", "Please hold...", .8f);

            try
            {
                string fullSourcePath = $"{SDKPath}/Assets/_KobGamesSDK_Slim_Ads";
                string fullDestPath = $"{DestPath}/Assets/_KobGamesSDK_Slim_Ads";
                if (Directory.Exists(fullSourcePath))
                {
                    if (Directory.Exists(fullDestPath))
                    {
                        Directory.Delete(fullDestPath, true);
                    }

                    Utils.CopyFolder(fullSourcePath, fullDestPath, i_Overwrite: true);
                }

                Debug.LogError($"Copied Ads Folder Done.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message} Stack: {ex.StackTrace}");
            }

            EditorApplication.UnlockReloadAssemblies();
            IsLockedReloadAssemblies = false;

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        public void CopyAdsManagerProjectLayerFile()
        {
            EditorApplication.LockReloadAssemblies();

            IsLockedReloadAssemblies = true;

            EditorUtility.DisplayProgressBar("Copying AdsManager.cs", "Please hold...", .8f);

            try
            {
                string fullSourcePath = $"{SDKPath}/Assets/_Project_Specific_Folder/Scripts/Managers/Ads";
                string fullDestPath = $"{DestPath}/Assets/_Project_Specific_Folder/Scripts/Managers/Ads";
                if (Directory.Exists(fullSourcePath))
                {
                    if (Directory.Exists(fullDestPath))
                    {
                        Directory.Delete(fullDestPath, true);
                    }

                    Utils.CopyFolder(fullSourcePath, fullDestPath, i_Overwrite: true);

                    Debug.LogError($"Copied AdsManager Folder Done.");
                }
                else
                {
                    Debug.LogError($"Can't find folder: {fullSourcePath}");
                }

            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message} Stack: {ex.StackTrace}");
            }

            EditorApplication.UnlockReloadAssemblies();
            IsLockedReloadAssemblies = false;

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        [Button(ButtonSizes.Large)]
        public void ReSyncFiles()
        {
            Selection.activeGameObject = null;

            EditorApplication.LockReloadAssemblies();

            IsLockedReloadAssemblies = true;

            EditorUtility.DisplayProgressBar("Copying Files", "Please hold...", .8f);

            try
            {
                int totalCopied = 0;
                foreach (var filePath in FilesListPath.Paths)
                {
                    if (filePath.IsEnabled && filePath.Path != "")
                    {
                        string fullSourcePath = $"{SDKPath}/{filePath.Path}";
                        string fullDestPath = $"{DestPath}/{filePath.Path}";
                        if (Directory.Exists(fullSourcePath))
                        {
                            if (Directory.Exists(fullDestPath))
                            {
                                Directory.Delete(fullDestPath, true);
                            }

                            Utils.CopyFolder(fullSourcePath, fullDestPath, i_Overwrite: true);
                            totalCopied++;
                        }
                        else if (File.Exists(fullSourcePath))
                        {
                            Utils.CopyFile(fullSourcePath, fullDestPath, i_Overwrite: true);
                            totalCopied++;
                        }
                    }
                }

                Debug.LogError($"Copied total of {totalCopied} plugins files and folders");

                GameSettings.Instance.General.DeleteBepicUneededFiles();

                GameSettings.Instance.General.DeleteIAPPlugins();

#if !ENABLE_MAX
                DeleteMAXFiles();                
#endif

#if !ENABLE_IRONSOURCE
                DeleteIronSourceFiles();
#endif

                if (!GameSettings.Instance.General.IsAdsEnabled)
                    GameSettings.Instance.General.DeleteAdsPlugins();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message} Stack: {ex.StackTrace}");
            }

            EditorApplication.UnlockReloadAssemblies();
            IsLockedReloadAssemblies = false;

            AssetDatabase.Refresh();
            ReImportiAPTogglerScript();

            EditorUtility.ClearProgressBar();

            Debug.LogError($"Finished re-copying Files & Folders, Total: {FilesListPath.Paths.Count}");

            Selection.activeObject = GameSettings.Instance;
        }

        [NonSerialized]
        private string[] m_BepicFiles =
        {
            "/Fx",
            "/SimpleSDK",
            "/SimpleSDKAttribution",
            "/SimpleSDKTopon",
            "/Plugins"
        };

        [NonSerialized, ShowInInspector, ReadOnly]
        public string SDKBepicPath = "/Users/kobyle/GamesProjects/BepicSDK";

        [NonSerialized]
        private string[] m_BepicPathsToDelete =
        {
            "/Fx",
            "/SimpleSDK",
            "/SimpleSDKAttribution",
            "/SimpleSDKTopon"
        };

        public void TryDeleteBepicFiles()
        {
            bool anyDeleted = false;

            foreach (var path in m_BepicPathsToDelete)
            {
                anyDeleted = Utils.RemoveFileOrDirectoryRelativePath(path);
            }

            //AssetDatabase.Refresh();

            if (!anyDeleted)
            {
                Debug.LogError($"Bepic: Nothing Deleted out of {m_BepicPathsToDelete.Length} files/directories");
            }
        }

        [Button(ButtonSizes.Large)]
        public void ReSyncBepicFiles()
        {
            Selection.activeGameObject = null;

            EditorApplication.delayCall += () =>
            {
                EditorUtility.DisplayProgressBar("Copying Bepic Files", "Please hold...", .8f);

                try
                {
                    TryDeleteBepicFiles();

                    Utils.CopiedFiles = 0;
                    Utils.CopiedFolders = 0;

                    int totalCopied = 0;
                    foreach (var filePath in m_BepicFiles)
                    {
                        string fullSourcePath = $"{SDKBepicPath}/{filePath}";
                        string fullDestPath = $"{DestPath}Assets{filePath}";
                        if (Directory.Exists(fullSourcePath))
                        {
                            Utils.CopyFolder(fullSourcePath, fullDestPath, i_Overwrite: true);
                            totalCopied++;
                        }
                        else if (File.Exists(fullSourcePath))
                        {
                            Utils.CopyFile(fullSourcePath, fullDestPath, i_Overwrite: true);
                            totalCopied++;
                        }
                    }

                    Debug.LogError($"Copied total of {totalCopied} plugins files and folders");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception: {ex.Message} Stack: {ex.StackTrace}");
                }

                EditorUtility.ClearProgressBar();

                Debug.LogError($"Finished re-copying Files & Folders, Total Folders: {Utils.CopiedFolders} Files: {Utils.CopiedFiles}");

                GameSettings.Instance.Analytics.DeleteAppsFlyerPlugins();
                GameSettings.Instance.Analytics.DeleteFacebookAnalyticsPlugins();

                Selection.activeObject = GameSettings.Instance;

                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                };
            };
        }

        [Button(ButtonSizes.Large)]
        public void DeleteBepicUneededFiles()
        {
            GameSettings.Instance.General.DeleteBepicUneededFiles();
            AssetDatabase.Refresh();
        }

        [Button("Delete Max Files", ButtonSizes.Large)]
        public void DeleteMAXFiles()
        {
            GameSettings.Instance.General.DeleteFilesAndFolders(GameSettings.Instance.General.MAXPathsToDelete);
            UtilsEditor.RemoveDefineDirective("gameanalytics_max_enabled");
        }

        [Button("Delete ironSource Files", ButtonSizes.Large)]
        public void DeleteIronSourceFiles()
        {
            GameSettings.Instance.General.DeleteFilesAndFolders(GameSettings.Instance.General.IronSourcePathsToDelete);
            UtilsEditor.RemoveDefineDirective("gameanalytics_ironsource_enabled");
        }

        [Button(ButtonSizes.Large)]
        public void RefreshAssetDatabase()
        {
            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
        }

        [Button(ButtonSizes.Large, Name = "Re-Import iAPToggler Script")]
        public void ReImportiAPTogglerScript()
        {
            AssetDatabase.ImportAsset("Assets/_KobGamesSDK_Slim/Prefabs/iAP/iAPToggler.cs", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
        }

        [Button(ButtonSizes.Large)]
        public void LockReloadAssemblies()
        {
            EditorApplication.LockReloadAssemblies();
        }

        [Button(ButtonSizes.Large)]
        public void UnlockReloadAssemblies()
        {
            EditorApplication.UnlockReloadAssemblies();
        }

        public void SetPathToThisProject()
        {
            DestPath = Application.dataPath.Replace("Assets", "");
        }

        [Button(ButtonSizes.Large)]
        public void ReImportPostProcessBuildScripts()
        {
            string[] fileGUIDs = AssetDatabase.FindAssets("post", null);

            PostProcessImportAsset.s_PrintReimports = true;

            foreach (string fileGUID in fileGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(fileGUID);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            PostProcessImportAsset.s_PrintReimports = false;

            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
#endif
    }

#if UNITY_EDITOR
    public class PostProcessImportAsset : AssetPostprocessor
    {
        public static bool s_PrintReimports = false;
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (s_PrintReimports)
            {
                //Debug.Log("OnPostprocessAllAssets");

                foreach (var imported in importedAssets)
                    Debug.Log("Imported: " + imported);

                foreach (var deleted in deletedAssets)
                    Debug.Log("Deleted: " + deleted);

                foreach (var moved in movedAssets)
                    Debug.Log("Moved: " + moved);

                foreach (var movedFromAsset in movedFromAssetPaths)
                    Debug.Log("Moved from Asset: " + movedFromAsset);
            }
        }
    }
#endif
}