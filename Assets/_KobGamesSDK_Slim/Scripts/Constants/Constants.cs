using UnityEngine;

namespace KobGamesSDKSlim
{
    public class Constants
    {
        public const string k_AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
        public const string k_None = "None";


        public static readonly string k_IsDebugLogsEditorPrefKey = $"{Application.dataPath}_IsDebugLogs";
        public static readonly string k_ScreenShotsEditorPrefKey = $"{Application.dataPath}_SavePath";
        public static readonly string k_BuildSaveEditorPrefKey = $"{Application.dataPath}_BuildSavePath";
        public static readonly string k_BuildAndroidEditorPrefKey = $"{Application.dataPath}_BuildAndroidPath";
        public static readonly string k_BuildIOSEditorPrefKey = $"{Application.dataPath}_BuildIOSPath";
        public static readonly string k_SignAPKEditorPrefKey = $"{Application.dataPath}_SignAPK";
        public static readonly string k_KeyStorePathEditorPrefKey = $"{Application.dataPath}_KeyStorePath";

        public static string[] EditorPrefKeys => new string[]
        {
            k_IsDebugLogsEditorPrefKey,
            k_ScreenShotsEditorPrefKey,
            k_BuildSaveEditorPrefKey,
            k_BuildAndroidEditorPrefKey,
            k_BuildIOSEditorPrefKey,
            k_SignAPKEditorPrefKey
        };
    }
}
