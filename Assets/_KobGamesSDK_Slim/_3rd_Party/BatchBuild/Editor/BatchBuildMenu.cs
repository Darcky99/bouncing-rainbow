using UnityEngine;
using System.Collections;
using UnityEditor;

namespace KobGamesSDKSlim
{
	public class BatchBuildMenu
	{

		[MenuItem("Tools/Build Android")]
		public static void BuildAndroid()
		{

			GameSettings.Instance.General.SetSignAPK();
			// TODO: Build streaming assets for Android
			BatchBuild.Build(
							  BatchBuildConfig.APP_NAME,
							  BatchBuildConfig.APP_IDENTIFIER,
							  BatchBuildConfig.APP_VERSION,
							  BuildTarget.Android,
							  BuildOptions.None);
			//BuildOptions.AutoRunPlayer);	
		}

		[MenuItem("Tools/Build Android (.Bat File)")]
		public static void BuildAndroidBat()
		{
			System.Diagnostics.Process.Start("E:\\UnityAutomation\\build_BounceUp.bat");
		}

		[MenuItem("Tools/Build IOS")]
		public static void BuildIOS()
		{
			// TODO: Build streaming assets for iOS

			BatchBuild.Build(
							  BatchBuildConfig.APP_NAME,
							  BatchBuildConfig.APP_IDENTIFIER,
							  BatchBuildConfig.APP_VERSION,
							  BatchBuild.BuildTarget_iOS,
							  BuildOptions.Development | BuildOptions.ConnectWithProfiler);
		}


		[MenuItem("Tools/Build Current Configured")]
		public static void BuildConfig()
		{
			switch (BatchBuildConfig.PLATFORM)
			{
				case "ios":
					BuildIOS();
					break;
				case "android":
					BuildAndroid();
					break;
				default:
					break;
			}
		}
	}
}