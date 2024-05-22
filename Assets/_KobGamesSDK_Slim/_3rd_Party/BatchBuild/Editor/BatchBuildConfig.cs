using UnityEditor;

public class BatchBuildConfig
{
    public static string APP_NAME = PlayerSettings.productName;// "Bounce Up";
    public static string APP_IDENTIFIER = PlayerSettings.applicationIdentifier; //"com.kobgames.bounceup";
    public static string APP_VERSION = PlayerSettings.bundleVersion;// "0.5.0";
    public static int APP_BUNDLEVERSIONCODE = PlayerSettings.Android.bundleVersionCode;// "2";
    public static string PLATFORM = "android";
    //public static string TARGET_DIR = Application.dataPath.Replace("/Assets", "") + "/Builds/Android";
    public static string TARGET_DIR = "E:/UnityAutomation/Builds";
}