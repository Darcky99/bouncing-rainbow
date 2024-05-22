#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Common variables and methods used by Project Scan Tools
    /// </summary>
    public static class PS_Utils
    {
        /// <summary>
        /// The current version of the Project Scan
        /// </summary>
        public static readonly string PS_VERSION = "1.0.8";

        /// <summary>
        /// The release date of the Project Scan version
        /// </summary>
        public static readonly string PS_RELEASE_DATE = "27 February 2020";

        private static string PSPath;
        private static PS_Data_BottleneckSettings PS_Data_BottleneckSettings;
        private static PS_Data_BottleneckResults PS_Data_BottleneckResults;

        public static string[] ALL_FILE_PATHS;

        /// <summary>
        /// Returns true if the Project is 2D
        /// </summary>
        public static bool IsProject2D
        {
            get
            {
                if (GetData_BottleneckSettings().GENERAL_PROJECT_TYPE_ID == 0)
                {
                    return (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D);
                }

                return (GetData_BottleneckSettings().GENERAL_PROJECT_TYPE_ID == 2);
            }
        }

        /// <summary>
        /// Converts bytes into a user-friendly format
        /// </summary>
        /// <param name="byteCount">Memory in bytes which is to be converted into user-friendly format</param>
        /// <returns>String that represents a user-friendly format of a file</returns>
        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 2);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        /// <summary>
        /// Retrieve local ID of an Object
        /// </summary>
        /// <param name="targetObject">Object to get Local ID from</param>
        /// <returns>Integer representing a Local ID of a given Object</returns>
        public static int GetLocalID(UnityEngine.Object targetObject)
        {
            System.Reflection.PropertyInfo inspectorModeInfo =
                typeof(SerializedObject).GetProperty("inspectorMode",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            SerializedObject serializedObject = new SerializedObject(targetObject);
            inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

            SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");

            int localId = localIdProp.intValue;

            return localId;
        }

        /// <summary>
        /// Returns the list all Scene files that are loaded
        /// </summary>
        /// <returns>List of Scenes</returns>
        public static List<Scene> GetBuildScenes()
        {
            string[] fileDirectories = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);
            List<Scene> scenes = new List<Scene>();

            for (int i = 0; i < fileDirectories.Length; i++)
            {
                string path = fileDirectories[i];

                if (path.EndsWith(".unity"))
                {
                    Scene newScene = SceneManager.GetSceneByName(Path.GetFileName(path));

                    if (newScene.isLoaded)
                    {
                        scenes.Add(newScene);
                    }
                }
            }

            return scenes;
        }

        /// <summary>
        /// Returns the Project Scan file path, making it possible to have "Project Scan" folder moved wherever without affecting it
        /// </summary>
        /// <returns>Returns string that contains file path of the Project Scan</returns>
        public static string GetData_ProjectScanPath()
        {
            if (String.IsNullOrEmpty(PSPath))
            {
                string[] results = AssetDatabase.FindAssets("Project Scan");

                for (int i = 0; i < results.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(results[i]);

                    if (Path.GetFileName(path) == "Project Scan")
                    {
                        PSPath = path;
                        break;
                    }
                }

                return PSPath;
            }
            else
            {
                return PSPath;
            }
        }

        /// <summary>
        /// Finds a Bottleneck Results file and returns it. A new one is created if it's missing
        /// </summary>
        /// <returns>Returns ScriptableObject</returns>
        public static PS_Data_BottleneckResults GetData_BottleneckResults()
        {
            if (PS_Data_BottleneckResults == null)
            {
                string psPath = GetData_ProjectScanPath();

                FileInfo dataDir = new FileInfo(psPath + "/Tools/Bottleneck Scanner/Data/");
                dataDir.Directory.Create();

                PS_Data_BottleneckResults = (PS_Data_BottleneckResults)AssetDatabase.LoadAssetAtPath(
                    psPath + "/Tools/Bottleneck Scanner/Data/PS_Data_BottleneckResults.asset",
                    typeof(PS_Data_BottleneckResults));

                if (PS_Data_BottleneckResults == null)
                {
                    PS_Data_BottleneckResults = ScriptableObject.CreateInstance<PS_Data_BottleneckResults>();

                    AssetDatabase.CreateAsset(PS_Data_BottleneckResults,
                        psPath + "/Tools/Bottleneck Scanner/Data/PS_Data_BottleneckResults.asset");
                    AssetDatabase.SaveAssets();
                }

                return PS_Data_BottleneckResults;
            }
            else
            {
                return PS_Data_BottleneckResults;
            }
        }

        /// <summary>
        /// Finds a Bottleneck Settings file and returns it
        /// </summary>
        /// <returns>Returns ScriptableObject</returns>
        public static PS_Data_BottleneckSettings GetData_BottleneckSettings()
        {
            if (PS_Data_BottleneckSettings == null)
            {
                string psPath = GetData_ProjectScanPath();

                FileInfo dataDir = new FileInfo(psPath + "/Tools/Bottleneck Scanner/Data/");
                dataDir.Directory.Create();

                PS_Data_BottleneckSettings = (PS_Data_BottleneckSettings)AssetDatabase.LoadAssetAtPath(
                    psPath + "/Tools/Bottleneck Scanner/Data/PS_Data_BottleneckSettings.asset",
                    typeof(PS_Data_BottleneckSettings));

                if (PS_Data_BottleneckSettings == null)
                {
                    PS_Data_BottleneckSettings = ScriptableObject.CreateInstance<PS_Data_BottleneckSettings>();

                    AssetDatabase.CreateAsset(PS_Data_BottleneckSettings, psPath + "/Tools/Bottleneck Scanner/Data/PS_Data_BottleneckSettings.asset");
                    AssetDatabase.SaveAssets();
                }

                return PS_Data_BottleneckSettings;
            }
            else
            {
                return PS_Data_BottleneckSettings;
            }
        }

        /// <summary>
        /// Returns all file paths that pass the filters. This function must be used ONCE per Scan
        /// </summary>
        /// <returns>An array of file paths</returns>
        public static string[] GetData_FilePaths()
        {
            string[] bannedFolders = GetData_BottleneckSettings().IGNORED_DIRECTORIES.ToArray();
            string[] bannedPrefixes = GetData_BottleneckSettings().FILTER_EXCLUDE_PREFIXES.Split(',')
                .Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string[] bannedExtensions = GetData_BottleneckSettings().FILTER_EXCLUDE_EXTENSIONS.Split(',')
                .Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string[] bannedFiles = GetData_BottleneckSettings().FILTER_EXCLUDE_FILES.Split(',').Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();

            string[] allDirectories = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

            List<string> filteredPaths = new List<string>();

            for (int i = 0; i < allDirectories.Length; i++)
            {
                string filePath = allDirectories[i].Replace('\\', '/');

                string fileName = Path.GetFileNameWithoutExtension(allDirectories[i]);
                string nameWithExtension = Path.GetFileName(allDirectories[i]);
                string fileExtension = Path.GetExtension(allDirectories[i]).ToLower().Replace(".", "");

                if (bannedExtensions.Contains(nameWithExtension))
                    continue;

                if (bannedExtensions.Contains(fileExtension))
                    continue;

                if (bannedFiles.Contains(fileName))
                    continue;

                if (fileExtension.Contains("meta"))
                    continue;

                if (InsideOfDirectory(bannedFolders, filePath))
                    continue;

                bool validPrefix = true;
                foreach (var bannedPrefix in bannedPrefixes)
                {
                    if (!fileName.StartsWith(bannedPrefix))
                        continue;

                    validPrefix = false;
                    break;
                }

                if (validPrefix)
                    filteredPaths.Add(filePath);
            }

            ALL_FILE_PATHS = filteredPaths.ToArray();
            return ALL_FILE_PATHS;
        }

        private static bool InsideOfDirectory(string[] bannedFolders, string path)
        {
            path = path.Replace('\\', '/');

            foreach (var bannedDir in bannedFolders)
            {
                var bannedFolder = bannedDir.Replace('\\', '/');

                if (path.IndexOf(bannedFolder, StringComparison.OrdinalIgnoreCase) != -1)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes Bottleneck Settings file and creates a new one, thus resetting Bottleneck Settings
        /// </summary>
        public static void DeleteData_BottleneckSettings()
        {
            try
            {
                string PSPath = GetData_ProjectScanPath();
                AssetDatabase.DeleteAsset(PSPath + "/Tools/Bottleneck Scanner/Data/PS_Data_BottleneckSettings.asset");
            }
            finally
            {
                GetData_BottleneckSettings();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Deletes Bottleneck Results file and creates a new one, thus resetting Bottleneck Results
        /// </summary>
        public static void DeleteData_BottleneckResults()
        {
            try
            {
                string psPath = GetData_ProjectScanPath();
                AssetDatabase.DeleteAsset(psPath + "/Tools/Bottleneck Scanner/Data/PS_Data_BottleneckResults.asset");
            }
            finally
            {
                GetData_BottleneckResults();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Used for determining whether or not to use the Dark Skin
        /// </summary>
        /// <returns>Returns true if Unity Skin is set to be Dark</returns>
        public static bool ProSkinEnabled()
        {
            if (GetData_BottleneckSettings().SKIN_TYPE == PS_Data_BottleneckSettings.SKINTYPE.Auto)
            {
                return EditorGUIUtility.isProSkin;
            }
            else
            {
                return GetData_BottleneckSettings().SKIN_TYPE == PS_Data_BottleneckSettings.SKINTYPE.Dark;
            }
        }

        /// <summary>
        /// Adds the file to filters.
        /// </summary>
        /// <param name="obj">Full path to target file</param>
        public static void AddFileToFilters(object obj)
        {
            string fileName = Path.GetFileName((string)obj);
            string[] bannedFiles = GetData_BottleneckSettings().FILTER_EXCLUDE_FILES.Split(',').Select(x => x.Trim())
                .Where(y => !string.IsNullOrEmpty(y)).ToArray();

            if (bannedFiles.Length > 0)
            {
                if (!bannedFiles.Contains(fileName))
                {
                    GetData_BottleneckSettings().FILTER_EXCLUDE_FILES += ", " + fileName;
                }
            }
            else
            {
                GetData_BottleneckSettings().FILTER_EXCLUDE_FILES += fileName;
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Adds the extension to filters.
        /// </summary>
        /// <param name="obj">Full path of target file</param>
        public static void AddExtensionToFilters(object obj)
        {
            string fileExtension = Path.GetExtension((string)obj).Replace(".", "");

            string[] bannedExtensions = GetData_BottleneckSettings().FILTER_EXCLUDE_EXTENSIONS.Split(',')
                .Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (bannedExtensions.Length > 0)
            {
                if (!bannedExtensions.Contains(fileExtension))
                {
                    GetData_BottleneckSettings().FILTER_EXCLUDE_EXTENSIONS += ", " + fileExtension;
                }
            }
            else
            {
                GetData_BottleneckSettings().FILTER_EXCLUDE_EXTENSIONS += fileExtension;
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Displays or updates a Progress Bar with updated process and the specified file that it processes at a given progress
        /// </summary>
        /// <param name="processName">Name of the Process that Editor goes through</param>
        /// <param name="fileName">Name of the File that Bottleneck Test Scans</param>
        /// <param name="currentProgress">Current progress</param>
        /// <param name="maxProgress">Maximum progress</param>
        public static void CallProgressBar(string processName, string fileName, int currentProgress, int maxProgress)
        {
            EditorUtility.DisplayProgressBar("Scanning for Performance Bottlenecks...", processName + " " + fileName,
                Mathf.InverseLerp(0, maxProgress, currentProgress));
        }

        /// <summary>
        /// Displays or updates a Progress Bar with updated process that it processes at a given progress
        /// </summary>
        /// <param name="processName">Name of the Process that Editor goes through</param>
        /// <param name="currentProgress">Current progress</param>
        /// <param name="maxProgress">Maximum progress</param>
        public static void CallProgressBar(string processName, int currentProgress, int maxProgress)
        {
            EditorUtility.DisplayProgressBar("Scanning for Performance Bottlenecks...", processName,
                Mathf.InverseLerp(0, maxProgress, currentProgress));
        }

        /// <summary>
        /// Displays or updates the final progress bar before removing it completely
        /// </summary>
        public static void RemoveProgressBar()
        {
            EditorUtility.DisplayProgressBar("Scanning for Performance Bottlenecks...", "Finishing up...", 1.0f);
            EditorUtility.ClearProgressBar();
        }

        public static bool ShowContextMenu()
        {
            string targetPath = Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject));

            FileAttributes pathAttr = File.GetAttributes(targetPath);

            return (pathAttr != FileAttributes.Directory);
        }

        public static Texture2D GetCategoryIcon(PS_Result.CATEGORY Category)
        {
            switch (Category)
            {
                case PS_Result.CATEGORY.AUDIO:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(AudioSource));

                case PS_Result.CATEGORY.CODE:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript));

                case PS_Result.CATEGORY.EDITOR:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(EditorSettings));

                case PS_Result.CATEGORY.LIGHTING:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(Light));

                case PS_Result.CATEGORY.MESH:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(MeshFilter));

                case PS_Result.CATEGORY.PARTICLE:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(ParticleSystem));

                case PS_Result.CATEGORY.PHYSICS:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(Rigidbody));

                case PS_Result.CATEGORY.SHADER:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(Shader));

                case PS_Result.CATEGORY.TEXTURE:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(Texture));

                case PS_Result.CATEGORY.UI:

                    return AssetPreview.GetMiniTypeThumbnail(typeof(CanvasGroup));

                default:

                    return null;
            }
        }
    }
}

#endif