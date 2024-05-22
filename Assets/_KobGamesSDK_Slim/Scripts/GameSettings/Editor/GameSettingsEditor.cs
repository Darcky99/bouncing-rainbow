using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEditor.PackageManager.Requests;
using System;
#if ENABLE_FACEBOOK
using Facebook.Unity.Editor;
#endif

namespace KobGamesSDKSlim
{
    public class GameSettingsEditor : MonoBehaviour
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void onScriptReload()
        {
            EditorApplication.delayCall += () =>
            {
                //Update DebugLogs correctly
                SetDebugLogs(GameSettings.Instance.General.IsDebugEnabled);

                //Replace missing scripts
                ScriptReplacer[] scripts = FindObjectsOfType<ScriptReplacer>(true);
                for (int i = 0; i < scripts.Length; i++)
                {
                    scripts[i].Replace();
                }

                EditorApplication.update += RemoveUnityPackages;
            };
        }

        [MenuItem("KobGamesSDK/Select Main Scene", false, -4)]
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

        [MenuItem("KobGamesSDK/Select KobyLayout", false, -4)]
        public static void SelectKobyLayout()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets/_KobGamesSDK_Slim/EditorLayouts/KobyLayout.wlt");

            if (File.Exists(path))
            {
                EditorUtility.LoadWindowLayout(path);
            }
        }

        [MenuItem("KobGamesSDK/Select GameSettings %g", false, -3)]
        [MenuItem("GameObject/Select KobGamesSDK GameSettings")]
        public static void SelectGameSettings()
        {
            Selection.activeObject = GameSettings.Instance;
        }

        [MenuItem("KobGamesSDK/Open BuildSettings #%g")]
        public static void OpenBuildSettings()
        {
            GameSettings.Instance.General.OpenBuildSettings();
        }

        [MenuItem("KobGamesSDK/Select RemoteSettings #%y")]
        public static void OpenRemoteSettings()
        {
            Selection.activeObject = Managers.Instance.RemoteSettings;
        }

        [MenuItem("KobGamesSDK/Select StorageManager #%h")]
        public static void OpenStorage()
        {
            Selection.activeObject = Managers.Instance.Storage;
        }

        [MenuItem("KobGamesSDK/Open PlayerSettings #%w")]
        public static void OpenPlayerSettings()
        {
#if UNITY_ANDROID
            Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
#else
            SettingsService.OpenProjectSettings("Project/Player");
#endif
        }

        [MenuItem("KobGamesSDK/Toggle GameObject Active (Alt+Shift+X) &%x")]
        public static void ToggleGameObjectActive()
        {
            Selection.activeGameObject.SetActive(!Selection.activeGameObject.activeSelf);
        }

        [MenuItem("KobGamesSDK/Toggle GameObject AutoSelect (Alt+Shift+C) &%z")]
        public static void SetGameObjectToSelectAfterSceneLoad()
        {
            if (Selection.activeGameObject == null || Selection.activeGameObject.GetComponent<GameObjectAutoSelect>() != null)
            {
                DestroyImmediate(Selection.activeGameObject.GetComponent<GameObjectAutoSelect>());

                clearAutoSelectGameObjectScript();
            }
            else
            {
                clearAutoSelectGameObjectScript();

                Selection.activeGameObject.AddComponent<GameObjectAutoSelect>();
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        private static void clearAutoSelectGameObjectScript()
        {
            foreach (var x in Resources.FindObjectsOfTypeAll<GameObjectAutoSelect>().Where(x => !EditorUtility.IsPersistent(x.gameObject)))
            {
                DestroyImmediate(x.GetComponent<GameObjectAutoSelect>());
            }
        }

        [MenuItem("KobGamesSDK/Export/ExportTest")]
        static void export()
        {
            AssetDatabase.ExportPackage("Tests", string.Format(@"{0}\{1}.unitypackage", Application.dataPath, "xxx"), ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);
        }

        [MenuItem("KobGamesSDK/Clear PlayerPrefs Data", false, 100)]
        static void ClearData()
        {
            File.Delete(string.Format("{0}\\{1}", Application.persistentDataPath, "SavedGame.es3"));
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("KobGamesSDK/Clear EditorPrefs Data (EDITOR)", false, 101)]
        static void ClearEditorData()
        {
            foreach (string editorPrefKey in Constants.EditorPrefKeys)
            {
                EditorPrefs.DeleteKey(editorPrefKey);
            }
        }

        //[MenuItem("KobGamesSDK/Clear Data")]
        //static void ClearData()
        //{
        //    File.Delete(string.Format("{0}\\{1}", Application.persistentDataPath, "SavedGame.es3"));
        //}

        public static void SetDebugLogs(bool i_Enabled)
        {
            if (i_Enabled && !UtilsEditor.IsDefineDirectiveExists(GameSettings.DirectiveConstants.k_ENABLE_LOGS_DIRECTIVE))
            {
                UtilsEditor.SetDefineDirective(GameSettings.DirectiveConstants.k_ENABLE_LOGS_DIRECTIVE);
                GameSettings.Instance.General.IsDebugEnabled = true;

                UnityEngine.Debug.LogError($"Changing ENABLE_LOGS Directive from False to True");
            }

            if (!i_Enabled && UtilsEditor.IsDefineDirectiveExists(GameSettings.DirectiveConstants.k_ENABLE_LOGS_DIRECTIVE))
            {
                UtilsEditor.RemoveDefineDirective(GameSettings.DirectiveConstants.k_ENABLE_LOGS_DIRECTIVE);
                GameSettings.Instance.General.IsDebugEnabled = false;

                UnityEngine.Debug.LogError($"Changing ENABLE_LOGS Directive from True to False");
            }
        }

        private static string m_PackageToAdd1 = "";//"com.unity.recorder@2.5.5";
        private static string m_PackageToRemove1 = "com.google.external-dependency-manager";

        private static bool m_IsPackageSearchDone = false;
        private static ListRequest m_SearchRequest = null;
        public static void SearchUnityPackage(string i_Package, Action i_FoundCallback, Action i_NotFoundCallback)
        {
            if (string.IsNullOrEmpty(i_Package))
            {
                i_NotFoundCallback.InvokeSafe();
                return;
            }

            if (m_SearchRequest == null)
            {
                Debug.LogWarning($"Search Package: '{i_Package}'");
                m_SearchRequest = UnityEditor.PackageManager.Client.List();
            }
            else
            {
                switch (m_SearchRequest.Status)
                {
                    case UnityEditor.PackageManager.StatusCode.Success:
                        Debug.LogWarning($"SearchUnityPackage Done '{i_Package}'");
                        m_IsPackageSearchDone = true;
                        break;
                    case UnityEditor.PackageManager.StatusCode.InProgress:
                        //Debug.LogWarning($"SearchUnityPackage {i_Package} InProgress...");
                        break;
                    case UnityEditor.PackageManager.StatusCode.Failure:
                        Debug.LogWarning($"SearchUnityPackage Failed '{i_Package}'... Stopping.");
                        m_IsPackageSearchDone = true;
                        break;
                    default:
                        m_IsPackageSearchDone = true;
                        break;
                }

                if (m_IsPackageSearchDone)
                {
                    m_IsPackageSearchDone = false;
                    bool isFound = m_SearchRequest?.Result?.Where(x => i_Package.Contains(x.name)).ToArray().Length > 0;

                    if (isFound)
                    {
                        // Package Found
                        i_FoundCallback.InvokeSafe();
                    }
                    else
                    {
                        // Package Not-Found
                        i_NotFoundCallback.InvokeSafe();
                    }

                    //Debug.LogError("DONE!");
                    m_SearchRequest = null;
                }
            }
        }

        private static Request m_CurrentRequest = null;
        public static void RemoveUnityPackages()
        {
            string package = m_PackageToRemove1;
            SearchUnityPackage(package,
                () =>
                {
                    if (!string.IsNullOrEmpty(package))
                    {
                        Debug.LogError($"[Removal] Package '{package}' Found. Removing...");
                        m_CurrentRequest = UnityEditor.PackageManager.Client.Remove(package);
                    }

                    EditorApplication.update -= RemoveUnityPackages;
                    EditorApplication.update += AddUnityPackages;
                },
                () =>
                {
                    Debug.LogWarning($"[Removal] Package '{package}' Not Found. Nothing to do...");
                    EditorApplication.update -= RemoveUnityPackages;
                    EditorApplication.update += AddUnityPackages;
                });
        }

        public static void AddUnityPackages()
        {
            if (m_CurrentRequest != null && m_CurrentRequest.Status == UnityEditor.PackageManager.StatusCode.InProgress)
                return;

            string package = m_PackageToAdd1;

            SearchUnityPackage(package,
                i_FoundCallback: () =>
                {
                    Debug.LogWarning($"[Adding] Package '{package}' Found. Skipping...");
                    EditorApplication.update -= AddUnityPackages;
                },
                i_NotFoundCallback: () =>
                {

                    if (!string.IsNullOrEmpty(package))
                    {
                        Debug.LogError($"[Adding] Package '{package}' Not Found. Adding Package...");
                        m_CurrentRequest = UnityEditor.PackageManager.Client.Add(package);
                    }

                    EditorApplication.update -= AddUnityPackages;
                });
        }
    }

    [CustomEditor(typeof(GameSettings))]
    public class GameSettingsOnFocusEditor : OdinEditor
    {
        private GameSettings m_GameSettings { get { return this.target as GameSettings; } }

        private bool m_IsDebugEnabled;
        private bool m_IsAdsEnabled;
        private bool hasFocused = false;

        private string m_PackageToAdd1 = "";// com.google.external-dependency-manager";
        private string m_PackageToRemove1 = "com.google.external-dependency-manager";
        private ListRequest m_PackagesListRequest;

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                //processPackagesListRequet();

                if (this.hasFocused == false)
                {
                    onGameSettingFocus();

                    this.hasFocused = true;
                }

                onGameSettingsGUI();
            }

            //if (!EditorApplication.isCompiling && !EditorApplication.isUpdating && m_IsReadyToShow)
            {
                base.OnInspectorGUI();
            }
        }

        // Occour once
        private void onGameSettingFocus()
        {
            m_GameSettings.OnGameSettingFocus();

            m_IsAdsEnabled = m_GameSettings.General.IsAdsEnabled = UtilsEditor.IsDefineDirectiveExists(GameSettings.DirectiveConstants.k_ENABLE_ADS_DIRECTIVE);
            //m_PackagesListRequest = UnityEditor.PackageManager.Client.List();
        }

        private void onGameSettingsGUI()
        {
            if (m_IsDebugEnabled != m_GameSettings.General.IsDebugEnabled)
            {
                m_IsDebugEnabled = m_GameSettings.General.IsDebugEnabled;
                GameSettingsEditor.SetDebugLogs(m_IsDebugEnabled);
            }

            if (m_IsAdsEnabled != m_GameSettings.General.IsAdsEnabled)
            {
                m_IsAdsEnabled = m_GameSettings.General.IsAdsEnabled;
                Debug.LogError("IsAdsEnabled Changed: " + m_IsAdsEnabled);

                if (m_IsAdsEnabled)
                {
                    GameSettings.Instance.Sync.CopyAdsFolder(() =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            UtilsEditor.SetDefineDirective(GameSettings.DirectiveConstants.k_ENABLE_ADS_DIRECTIVE);
                        };
                    });
                }
                else
                {
                    UtilsEditor.RemoveDefineDirective(GameSettings.DirectiveConstants.k_ENABLE_ADS_DIRECTIVE);
                    m_GameSettings.AdsMediation.MediationNetworks = eMediationNetworks.None;
                }
            }

            m_GameSettings.OnGameSettingsGUI();

#if ENABLE_FACEBOOK
            if (m_GameSettings.General.UpdateFacebookManifest)
            {
                //Debug.LogError("UpdateFacebookManifest");
                ManifestMod.GenerateManifest();
                m_GameSettings.General.UpdateFacebookManifest = false;
            }
#endif

            //if (Event.current.type == EventType.Layout)
            //{
            //    m_IsReadyToShow = true;
            //}

            //m_DelayCounter++;

            //if (m_DelayCounter > 100)
            //{
            //    {
            //        //Debug.LogError("SAVE");
            //        //AssetDatabase.SaveAssets();


            //    }
            //    m_DelayCounter = 0;
            //}

            if (m_GameSettings.General.IsBuildIOS)
            {
                m_GameSettings.General.IsBuildIOS = false;

                EditorApplication.delayCall += () =>
                {
                    BuildIOS();
                };
            }

            if (m_GameSettings.General.IsBuildAndroid)
            {
                m_GameSettings.General.IsBuildAndroid = false;

                EditorApplication.delayCall += () =>
                {
                    BuildAndroid();
                };
            }
        }

        public static void BuildAndroid()
        {
            EditorSceneManager.SaveOpenScenes();

            if (!Directory.Exists(GameSettings.Instance.General.BuildAndroidFolderPath))
            {
                Directory.CreateDirectory(GameSettings.Instance.General.BuildAndroidFolderPath);
            }

            if (GameSettings.Instance.General.SignAPK)
            {
                GameSettings.Instance.General.SetSignAPK();
            }

            EditorUserBuildSettings.buildAppBundle = GameSettings.Instance.General.IsAppBundle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.installInBuildFolder = false;
            EditorUserBuildSettings.SetBuildLocation(BuildTarget.Android, GameSettings.Instance.General.BuildAndroidFolderPath);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
            buildPlayerOptions.locationPathName = GameSettings.Instance.General.BuildAndroidFullPathIncludingFolder;

            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | GameSettings.Instance.General.BuildOptionAndroid;

            if (!GameSettings.Instance.AdsMediation.IsIronSourceEnabled)
            {
                UtilsEditor.RemoveDefineDirective("gameanalytics_ironsource_enabled");
            }

            if (!GameSettings.Instance.AdsMediation.IsMAXEnabled)
            {
                UtilsEditor.RemoveDefineDirective("gameanalytics_max_enabled");
            }

            ProjectCustomBuildProcessor.CustomBuildProcessor(buildPlayerOptions);
        }

        public static void BuildIOS()
        {
            EditorSceneManager.SaveOpenScenes();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
            buildPlayerOptions.locationPathName = GameSettings.Instance.General.BuildIOSFullPath;
            buildPlayerOptions.target = BuildTarget.iOS;
            if (Directory.Exists(buildPlayerOptions.locationPathName))
            {
                buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.AcceptExternalModificationsToPlayer | GameSettings.Instance.General.BuildOptionIOS;
            }
            else
            {
                buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | GameSettings.Instance.General.BuildOptionIOS;
            }

            ProjectCustomBuildProcessor.CustomBuildProcessor(buildPlayerOptions);
        }

        private void processPackagesListRequet()
        {
            if (m_PackagesListRequest != null)
            {
                if (m_PackagesListRequest.Result != null)
                {
                    bool isPackageToAddFound = false;
                    bool isPackageToRemoveFound = false;
                    foreach (var item in m_PackagesListRequest.Result)
                    {
                        Debug.LogError(item.name);
                        if (item.name.Equals(m_PackageToAdd1))
                        {
                            //Debug.LogError($"PackageManager: Found: {item.name}");
                            isPackageToAddFound = true;
                            break;
                        }
                    }

                    foreach (var item in m_PackagesListRequest.Result)
                    {
                        if (item.name.Equals(m_PackageToRemove1))
                        {
                            //Debug.LogError($"PackageManager: Found: {item.name}");
                            isPackageToRemoveFound = true;
                            break;
                        }
                    }

                    if (!isPackageToAddFound)
                    {
                        Debug.LogError($"PackageManager: None Found '{m_PackageToAdd1}' Adding Now...");

                        m_PackagesListRequest = null;

                        UnityEditor.PackageManager.Client.Add(m_PackageToAdd1);
                    }

                    if (isPackageToRemoveFound)
                    {
                        Debug.LogError($"PackageManager: Found '{m_PackageToRemove1}' Removing Now...");

                        m_PackagesListRequest = null;

                        UnityEditor.PackageManager.Client.Remove(m_PackageToRemove1);
                    }
                }
            }
        }
    }
}