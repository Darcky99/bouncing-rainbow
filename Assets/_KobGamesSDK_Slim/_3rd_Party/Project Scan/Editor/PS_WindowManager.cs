#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    [System.Serializable]
    public class PS_WindowManager : EditorWindow
    {
        public static PS_Window_BottleneckScan PS_Window_BottleneckScan;
        public static PS_Window_BottleneckSettings PS_Window_BottleneckSettings;
        //public static PS_Window_UsageMap PS_Window_UsageMap;
        //public static PS_Window_MemoryInspector PS_Window_MemoryInspector;

        [MenuItem("Window/Project Scan/Bottleneck Scanner")]
        private static void Init_Window_BottleneckScanner()
        {
            PS_Utils.GetData_ProjectScanPath();
            PS_Window_BottleneckScan = (PS_Window_BottleneckScan)GetWindow(typeof(PS_Window_BottleneckScan), false, "Bottlenecks", true);
            PS_Window_BottleneckScan.minSize = new Vector2(830, 250);
        }

        [MenuItem("Assets/Bottleneck Scanner/Ignore this File", false, 30)]
        public static void Action_IgnoreFiles()
        {
            try
            {
                string targetPath = System.IO.Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject));
                PS_Utils.AddFileToFilters(targetPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        [MenuItem("Assets/Bottleneck Scanner/Ignore Files of this Extension", false, 30)]
        public static void Action_IgnoreExtensions()
        {
            try
            {
                string targetPath = System.IO.Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject));
                PS_Utils.AddExtensionToFilters(targetPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        [MenuItem("Assets/Bottleneck Scanner/Ignore this File", true)]
        [MenuItem("Assets/Bottleneck Scanner/Ignore Files of this Extension", true)]
        public static bool CheckFileValidity()
        {
            return PS_Utils.ShowContextMenu();
        }

        [MenuItem("Window/Project Scan/Bottleneck Settings")]
        public static void Init_Window_BottleneckSettings()
        {
            PS_Window_BottleneckSettings = (PS_Window_BottleneckSettings)GetWindow(typeof(PS_Window_BottleneckSettings), true, "Bottleneck Settings", true);
            PS_Window_BottleneckSettings.minSize = new Vector2(720, 500);
        }

        //[MenuItem("GameObject/Show Usage Map", false, 30)]
        //[MenuItem("Assets/Show in Usage Map", false, 30)]
        //[MenuItem("Window/Project Scan/Usage Map")]
        private static void Init_Window_UsageMap()
        {
            //PS_Window_UsageMap = (PS_Window_UsageMap)GetWindow(typeof(PS_Window_UsageMap), false, "Usage Map", true);
            //PS_Window_UsageMap.minSize = new Vector2(500, 500);

            //PS_Window_UsageMap.CheckObjectDependencies(Selection.activeObject);
        }

        //[MenuItem("Window/Project Scan/Memory Inspector")]
        private static void Init_Window_MemoryInspector()
        {
            //PS_Window_MemoryInspector = (PS_Window_MemoryInspector)GetWindow(typeof(PS_Window_MemoryInspector), false, "Memory", true);
        }

        [MenuItem("Window/Project Scan/Help and Support")]
        private static void Init_Window_HelpSupport()
        {
            System.Diagnostics.Process.Start("https://hardcodelab.com/support/");
        }
    }
}

#endif