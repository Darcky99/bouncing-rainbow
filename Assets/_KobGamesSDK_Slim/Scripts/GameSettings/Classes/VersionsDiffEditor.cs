using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KobGamesSDKSlim
{
    [Serializable]
    public class VersionsDiffEditor
    {
        [PropertyOrder(-102), BoxGroup, ShowInInspector, DisplayAsString, HideLabel, EnableGUI, ShowIf(nameof(IsDiffExists)), GUIColor(nameof(GetDiffStatusColor))]
        public string VersionsDiffStatus
        {
            get
            {
                //Debug.LogError("HI");

                var versionsSDK = VersionsSDK.VersionsList.Versions;
                var versionsProject = VersionsProject.VersionsList.Versions;

                string diffString = string.Empty;

                if (versionsProject.Count() == 0)
                {
                    diffString = "SDK Sync Needed: can't find versions json in project folder";
                }
                else
                {
                    foreach (var versionSDK in versionsSDK)
                    {
                        var index = versionsProject.FindIndex(x => x.Name == versionSDK.Name);

                        if (index >= 0)
                        {
                            if (versionsProject[index].Version != versionSDK.Version)
                            {
                                if (diffString == string.Empty)
                                {
                                    diffString = "SDK Sync Needed: ";
                                }

                                diffString += $"{versionsProject[index].Name} | ";
                            }
                        }
                    }
                }

                return diffString;
            }
        }

        public Color GetDiffStatusColor()
        {
            return VersionsDiffStatus == string.Empty ? Color.green : Color.yellow;
        }

        public bool IsDiffExists()
        {
            return VersionsDiffStatus != string.Empty;
        }

        [BoxGroup("SDK"), HideLabel]
        public VersionsData VersionsSDK = new VersionsData(true);

        [BoxGroup("SDK"), Button]
        public void SyncJsonToProject()
        {
            string appPath = Application.dataPath.Replace("Assets", "");

            Utils.CopyFile($"{appPath}/{VersionsSDK.JsonFilePath}", $"{appPath}/{VersionsProject.JsonFilePath}", true);

            Debug.LogError("Copied Json file");
        }

        [BoxGroup("Project"), HideLabel]
        public VersionsData VersionsProject = new VersionsData(false);
    }

    [Serializable]
    public class Versions
    {
    }

    [Serializable]
    public class VersionsData
    {
        private bool m_IsSDK = false;
        public VersionsData(bool i_IsSDK = false)
        {
            m_IsSDK = i_IsSDK;
        }

        [PropertyOrder(1), HideLabel, OnInspectorInit(nameof(onInspectorInit))]
        public VersionsList VersionsList = new VersionsList();

        private void onInspectorInit()
        {
            setJsonFilePath();
            LoadFromJson();
        }


        private string m_JsonFilePath = string.Empty;
        [PropertyOrder(2), ShowInInspector, HideLabel]
        public string JsonFilePath => m_JsonFilePath;
        //{
        //    get
        //    {
        //        if (m_JsonFilePath == string.Empty)
        //        {
        //            setJsonFilePath();
        //        }

        //        return m_JsonFilePath;
        //    }
        //}

        private void setJsonFilePath()
        {
            //Debug.LogError(m_JsonFilePath);
#if UNITY_EDITOR
            string name = m_IsSDK ? "VersionsSDK" : "VersionsProject";
            foreach (string guid in AssetDatabase.FindAssets(name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.Contains("json"))
                {
                    m_JsonFilePath = path;
                    break;
                }
            }
#endif
        }

        //private string m_VersionsJsonLoaded;
        //[PropertyOrder(4), OnInspectorInit(nameof(loadJson)), EnableGUI, ShowInInspector, MultiLineProperty(5), FoldoutGroup("Json Text"), HideLabel, ShowIf(nameof(m_IsSDK))]
        //public string VersionsJson
        //{
        //    get => m_VersionsJsonLoaded;
        //    set
        //    {
        //        try
        //        {
        //            VersionsList = JsonUtility.FromJson<VersionsList>(value);
        //        }
        //        catch { }
        //    }
        //}

        //private void loadJson()
        //{
        //    m_VersionsJsonLoaded = JsonUtility.ToJson(VersionsList, true);
        //}

        [PropertyOrder(5), HorizontalGroup("Json"), VerticalGroup("Json/1"), Button, ShowIf(nameof(m_IsSDK))]
        public void SaveToJson()
        {
            try
            {
                //File.WriteAllText(JsonFilePath, VersionsJson);
                File.WriteAllText(JsonFilePath, JsonUtility.ToJson(VersionsList, true));

                Debug.LogError($"Saved Json: {JsonFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to Save To Json, {ex.Message}");
            }
        }

        [PropertyOrder(5), HorizontalGroup("Json"), VerticalGroup("Json/2"), Button]
        public void LoadFromJson()
        {
            try
            {
                VersionsList = JsonUtility.FromJson<VersionsList>(File.ReadAllText(JsonFilePath));

                Debug.LogError($"Loaded Json: {JsonFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to Load From Json, {ex.Message}");
            }
        }

        //[Button]
        //public void IsIronSource()
        //{
        //     Debug.LogError(TypeExists("IronSource"));
        //}

        [PropertyOrder(3), Button, ShowIf(nameof(m_IsSDK))]
        public void UpdateVersions()
        {
            for (int i = 0; i < VersionsList.Versions.Count; i++)
            {
                string key = VersionsList.Versions[i].Name;

                string version = string.Empty;

                switch (key)
                {
                    case "IronSource":
                        version = Utils.GetFieldValue("IronSource", "UNITY_PLUGIN_VERSION")?.ToString();
                        break;
                    case "MaxSdk":
                        version = Utils.GetFieldValue("MaxSdk", "_version")?.ToString();
                        break;
                    case "AppsFlyer":
                        version = Utils.GetFieldValue("AppsFlyerSDK.AppsFlyer", "kAppsFlyerPluginVersion")?.ToString();
                        break;
                    case "GameAnalytics":
                        version = Utils.GetFieldValue("GameAnalyticsSDK.Setup.Settings", "VERSION")?.ToString();
                        break;
                    case "AppMetrica":
                        version = Utils.GetFieldValue("AppMetrica", "VERSION")?.ToString();
                        break;
                    case "Facebook":
                        version = Utils.GetFieldValue("Facebook.Unity.FacebookSdkVersion", "Build")?.ToString();
                        break;
                    case "Fx":
                        version = Utils.GetFieldValue("FxNS.FxSdk", "FX_SDK_VERSION")?.ToString();
                        break;
                    case "SimpleSDK":
                        version = Utils.GetFieldValue("SimpleSDKNS.SimpleSDK", "SDK_VERSION")?.ToString();
                        break;
                    case "SimpleSDKAttribution":
                        version = Utils.GetFieldValue("SimpleSDKNS.AppsflyerHelper", "getAttrVersion")?.ToString();
                        break;
                    case "SimpleSDKTopon":
                        version = Utils.GetFieldValue("SimpleSDKTopon", "TOPON_VERSION")?.ToString();
                        break;
                    default:
                        break;
                }

                if (version != string.Empty && version != null)
                {
                    VersionsList.Versions[i].Version = version;
                }
            }
        }

        //[Button]
        //public void GetVersion()
        //{
        //    Debug.LogError(GetFieldValue("IronSource", "UNITY_PLUGIN_VERSION"));
        //}
    }

    [Serializable]
    public class VersionsList
    {
        [TableList, ListDrawerSettings(Expanded = true)]
        public List<VersionInfo> Versions = new List<VersionInfo>();
    }

    [Serializable]
    public class VersionInfo
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public string Version;
    }
}