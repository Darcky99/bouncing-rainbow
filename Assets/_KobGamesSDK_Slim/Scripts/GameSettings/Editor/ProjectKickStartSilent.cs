using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace KobGamesSDKSlim
{
    [InitializeOnLoad]
    public class ProjectKickStartSilent
    {
        private const string JsonConfigName = "InitProjectConfig";

        private static bool s_ExecuteAllowedOnce
        {
            get { return EditorPrefs.GetBool(nameof(s_ExecuteAllowedOnce), true); }
            set { EditorPrefs.SetBool(nameof(s_ExecuteAllowedOnce), value); }
        }

        private static bool s_IsBashMode
        {
            get { return EditorPrefs.GetBool(nameof(s_IsBashMode), false); }
            set { EditorPrefs.SetBool(nameof(s_IsBashMode), value); }
        }

        [MenuItem("KobGamesSDK/Project Kick Start Silent %l", false, 51)]
        public static void ProjectKickStartMenu()
        {
            s_ExecuteAllowedOnce = true;
            EditorApplication.delayCall += () => ProjectKickStartSilent.Instance.ProcessInitJsonIfExists();
        }

        public static ProjectKickStartSilent Instance = new ProjectKickStartSilent();
        static ProjectKickStartSilent()
        {
            s_IsBashMode = false;

            // Will be called the moment unity launches
            // Will also be fired when batch mode script is calling ProcessInitJsonIfExistsFromBash()
            Instance.ProcessInitJsonIfExists();

            EditorApplication.quitting += onEditorQuitting;
        }

        private static string m_JsonConfigPath { get { return $"{Application.dataPath}/{JsonConfigName}"; } }
        private static bool m_IsRunning = false;

        public static void Test()
        {
            ProjectKickStart.SelectMainScene();
            ProjectKickStart projectKickStart = ProjectKickStart.ShowWindow(projectKickStartDone);
            projectKickStart.ShrinkWindow();
            projectKickStart.OnEnableManual();
            projectKickStart.SetGAStep();
            projectKickStart.Process();
        }

        public static void ProcessInitJsonIfExistsFromBash()
        {
            s_IsBashMode = true;
            s_ExecuteAllowedOnce = true;

            if (s_IsBashMode) Debug.LogError("ProcessInitJsonIfExistsFromBash #1");

            Instance.ProcessInitJsonIfExists();
        }

        public void ProcessInitJsonIfExists()
        {
            GameSettingsEditor.SetDebugLogs(true);

            if (s_IsBashMode) Debug.LogError("ProcessInitJsonIfExistsFromBash #2");

            if (s_ExecuteAllowedOnce && !m_IsRunning)
            {
                m_IsRunning = true;

                if (s_IsBashMode) Debug.LogError("ProcessInitJsonIfExistsFromBash #3");

                s_ExecuteAllowedOnce = false;

                EditorApplication.delayCall = () =>
                {
                    if (s_IsBashMode) Debug.LogError("ProcessInitJsonIfExistsFromBash #4");

                    if (File.Exists($"{m_JsonConfigPath}"))
                    {

                        GameSettingsEditor.SelectKobyLayout();

                        Debug.LogError("Found Project in Init State");
                        Debug.LogError("---------------------------");
                        Debug.LogError("Configuration Json File located, project Initialization Started...");

                        string jsonString = File.ReadAllText(m_JsonConfigPath);
                        //Debug.LogError(jsonString);

                        ProjectConfig projectConfig = JsonUtility.FromJson<ProjectConfig>(jsonString);

                        Debug.LogError($"Project Config: {projectConfig}");

                        ProjectKickStart.SelectMainScene();
                        ProjectKickStart projectKickStart = ProjectKickStart.ShowWindow(projectKickStartDone);

                        projectKickStart.RunSilently = projectConfig.RunSilently || s_IsBashMode;
                        projectKickStart.GameName = projectConfig.ProjectName;
                        projectKickStart.BundleIdentifier = projectConfig.BundleIdentifier;
                        projectKickStart.AppleStoreId = projectConfig.AppleId;
                        projectKickStart.FacebookId = projectConfig.FacebookId;



                        if (projectKickStart.RunSilently)
                        {
                            projectKickStart.ShrinkWindow();
                        }

                        Debug.LogError("---------------------------");

                        if (projectKickStart.RunSilently)
                        {
                            projectKickStart.Process();

                            if (s_IsBashMode)
                            {
                                ProjectKickStart.OnProjectKickStartDone += () =>
                                {
                                    projectKickStartFinalDone();
                                };
                            }
                        }
                    }
                    else
                    {
                        if (s_IsBashMode)
                        {
                            projectKickStartFinalDone();
                        }
                    }
                };
            }
        }

        private static void projectKickStartDone()
        {
            // Finished, remove the init file
            File.Delete(m_JsonConfigPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.LogError("Project KickStartSilent Done Successfully.");
        }

        private static void projectKickStartFinalDone()
        {
            m_IsRunning = false;

            Debug.LogError("Project Config DONE");
            EditorApplication.Exit(0);
        }

        private static void onEditorQuitting()
        {
            EditorPrefs.DeleteKey(nameof(s_ExecuteAllowedOnce));
        }
    }

    [Serializable]
    public class ProjectConfig
    {
        public bool RunSilently = false;
        public string ProjectName;
        public string BundleIdentifier;
        public string AppleId;
        public string FacebookId;

        public override string ToString()
        {
            return $"\n  RunSilently: {RunSilently} \n  ProjectName: {ProjectName} \n  BundleIdentifier: {BundleIdentifier} \n  AppleId: {AppleId} \n  FacebookId: {FacebookId}\n";
        }
    }
}