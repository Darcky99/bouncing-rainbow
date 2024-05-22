using System;
using DG.Tweening.Core;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.U2D;
using KobGamesSDKSlim.ProjectValidator;

namespace KobGamesSDKSlim
{
    [InitializeOnLoad]
    public class ProjectCustomBuildProcessor
    {
        private static bool s_IsSilentBuild = false;

        static ProjectCustomBuildProcessor()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(CustomBuildProcessor);
        }

        public static void PerformBuildSilent()
        {
            s_IsSilentBuild = true;

            //SAVE CURRENT PROJECT STATUS
            bool wasDebugEnable = UtilsEditor.IsDefineDirectiveExists(GameSettings.DirectiveConstants.k_ENABLE_LOGS_DIRECTIVE);

            GameSettingsEditor.SetDebugLogs(true);

            UnityEngine.Debug.Log($"BuildState: PerformBuildSilent Build Start {s_IsSilentBuild}");

            string[] args = Environment.GetCommandLineArgs();

            // Android Build
            if (args.Contains(GeneralEditor.k_BuildAndroid))
            {
                UnityEngine.Debug.Log($"BuildState: PerformBuildSilent Build Android");

                if (args.Contains("IL2CPP"))
                {
                    UnityEngine.Debug.Log($"BuildState: Switching to IL2CPP");
                    GameSettings.Instance.General.SwitchToIL2CPP();
                    GooglePlayServices.PlayServicesResolver.MenuForceResolve();
                }
                else
                {
                    UnityEngine.Debug.Log($"BuildState: Switching to Mono");
                    GameSettings.Instance.General.SwitchToMono();
                    GooglePlayServices.PlayServicesResolver.MenuForceResolve();
                }

                var orgBuildAndroidPath = GameSettings.Instance.General.BuildAndroidPath;
                var orgBuildOptionAndroid = GameSettings.Instance.General.BuildOptionAndroid;
                var orgIsAppBundle = GameSettings.Instance.General.IsAppBundle;
                var orgSignAPK = GameSettings.Instance.General.SignAPK;
                var orgTargetSdkVersion = PlayerSettings.Android.targetSdkVersion;

                GameSettings.Instance.General.IsAppBundle = args.Contains("AppBundle");
                GameSettings.Instance.General.BuildAndroidPath = "/Users/kobyle/Dropbox/Shared-VM/Builds/";
                GameSettings.Instance.General.BuildOptionAndroid = BuildOptions.ShowBuiltPlayer;
                GameSettings.Instance.General.SignAPK = true;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

                if (args.Contains("DevelopmentBuild"))
                {
                    GameSettings.Instance.General.BuildOptionAndroid |= BuildOptions.Development;
                }

                GameSettings.Instance.General.SetSplashAndIconSettings();
                GameSettingsOnFocusEditor.BuildAndroid();

                GameSettings.Instance.General.BuildAndroidPath = orgBuildAndroidPath;
                GameSettings.Instance.General.BuildOptionAndroid = orgBuildOptionAndroid;
                GameSettings.Instance.General.IsAppBundle = orgIsAppBundle;
                GameSettings.Instance.General.SignAPK = orgSignAPK;
                PlayerSettings.Android.targetSdkVersion = orgTargetSdkVersion;
            }

            // IOS Build
            if (args.Contains(GeneralEditor.k_BuildIOS))
            {
                if (args.Contains(nameof(GeneralEditor.k_None)))
                {
                    UnityEngine.Debug.Log($"BuildState: PerformBuildSilent with k_None");
                    GameSettings.Instance.General.BuildSuccessCmd = GeneralEditor.k_None;
                }
                else if (args.Contains(nameof(GeneralEditor.k_UpdateMetaData)))
                {
                    UnityEngine.Debug.Log($"BuildState: PerformBuildSilent with k_UpdateMetaData");
                    GameSettings.Instance.General.BuildSuccessCmd = GeneralEditor.k_UpdateMetaData;
                }
                else if (args.Contains(nameof(GeneralEditor.k_UpdatePolicy)))
                {
                    UnityEngine.Debug.Log($"BuildState: PerformBuildSilent with k_UpdatePolicy");
                    GameSettings.Instance.General.BuildSuccessCmd = GeneralEditor.k_UpdatePolicy;
                }
                else if (args.Contains(nameof(GeneralEditor.k_BuildAndUpload)))
                {
                    UnityEngine.Debug.Log($"BuildState: PerformBuildSilent with k_BuildAndUpload");
                    GameSettings.Instance.General.BuildSuccessCmd = GeneralEditor.k_BuildAndUpload;
                }
                else if (args.Contains(nameof(GeneralEditor.k_UploadOnly)))
                {
                    UnityEngine.Debug.Log($"BuildState: PerformBuildSilent with k_UploadOnly");
                    GameSettings.Instance.General.BuildSuccessCmd = GeneralEditor.k_UploadOnly;
                }
                else
                {
                    UnityEngine.Debug.Log($"BuildState: PerformBuildSilent with No Args, Using GameSettings.Instance.General.BuildSuccessCmd: {GameSettings.Instance.General.BuildSuccessCmd}");
                }

                UnityEngine.Debug.Log($"BuildState: PerformBuildSilent Build IOS");
                GameSettingsOnFocusEditor.BuildIOS();
            }

            UnityEngine.Debug.Log($"BuildState: PerformBuildSilent Build Done");
            GameSettingsEditor.SetDebugLogs(wasDebugEnable);
        }

        public static void CustomBuildProcessor(BuildPlayerOptions i_BuildPlayerOptions)
        {
            //SAVE CURRENT PROJECT STATUS
            bool wasDebugEnable = UtilsEditor.IsDefineDirectiveExists(GameSettings.DirectiveConstants.k_ENABLE_LOGS_DIRECTIVE);

            //BUILD PROJECT
            BuildReport report = BuildPlayerWithReport(i_BuildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string buildFilePath = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? GameSettings.Instance.General.BuildAndroidFullPathIncludingFolder : GameSettings.Instance.General.BuildIOSFullPath;
                UnityEngine.Debug.LogError($"BuildState: Build succeeded: {summary.totalSize} bytes File: {buildFilePath}");

                GameSettings.Instance.General.OnBuildSuccess();
            }

            if (summary.result == BuildResult.Failed)
            {
                UnityEngine.Debug.LogError("BuildState: Build failed");
            }

            //LOAD PREVIOUS PROJECT STATUS
            UnityEngine.Debug.Log($"Changing IsDebugLogs from {GameSettings.Instance.General.IsDebugEnabled} to {wasDebugEnable}");
            GameSettingsEditor.SetDebugLogs(wasDebugEnable);
        }

        public static BuildReport BuildPlayerWithReport(BuildPlayerOptions i_BuildPlayerOptions)
        {

            UnityEngine.Debug.LogError($"BuildState: BuildPlayerWithReport: {EditorUserBuildSettings.activeBuildTarget}");

            bool isAddressableBuild = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && GameSettings.Instance.General.AddressableAndroid) ||
                                 (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS && GameSettings.Instance.General.AddressableIOS);

            UnityEngine.Debug.LogError($"BuildState: IsAddressableBuild: {isAddressableBuild}");

            ValidateBuild(i_BuildPlayerOptions, isAddressableBuild);
            
            return BuildPipeline.BuildPlayer(i_BuildPlayerOptions);
        }

        public static void ValidateBuild(BuildPlayerOptions i_BuildPlayerOptions, bool i_IsAddressableBuild = true)
        {
            //Rule #0
            if (GameSettings.Instance.General.IOSTeamIDTest)
            {
                PlayerSettings.iOS.appleEnableAutomaticSigning = false;

                //if (GameSettings.Instance.General.SignIOSTeamID == GeneralEditor.k_BepNew)
                //{
                //    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.gamestest.test");
                //}
                //else
                //if (GameSettings.Instance.General.SignIOSTeamID == GeneralEditor.k_Kpic)
                {
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.beptest.test");
                }
            }

            //Rule #1 - DOTween Safe Mode force true for production build
            if (!i_BuildPlayerOptions.options.HasFlag(BuildOptions.Development))
            {
                if (Resources.Load<DOTweenSettings>("DOTweenSettings").useSafeMode == false)
                {
                    throw new Exception("DOTWeen Safe Mode is set to false, please set to true for production build.\nBuilding Stopped.");
                }
            }

            //Rule #2 - Setting up Addressables
            if (i_IsAddressableBuild)
            {
                if (s_IsSilentBuild)
                {
                    UnityEngine.Debug.Log("BuildState: Calling BuildAddresssableAssetPreBuild()");
                    BuildAddresssableAssetPreBuild();
                }
                else
                {
                    // With dialog...
                    if (EditorUtility.DisplayDialog("Build with Addressables",
                       "Do you want to build a clean addressables before export?",
                       "Build with Addressables", "Skip"))
                    {
                        BuildAddresssableAssetPreBuild();
                    }
                }
            }

            //Rule #3 - Force remove Logs
            if (!i_BuildPlayerOptions.options.HasFlag(BuildOptions.Development))
            {
                GameSettingsEditor.SetDebugLogs(false);
            }

            //Rule #4 - Force ASTC for Sprites on iOS
            ForceSpriteAtlasOptions();
            //Note - Not a fan of automatic setting up sprite configuration when building since we might want specific formats like uncompressed sometimes. In any case i'll leave the code here in case we want to use in the future
            //Note 2 - we already have default texture importer so we might not need to use it at all
            //ForceSpriteOptions();
            
            //Rule #5 - Project Validation
            if (!ValidatorWindow.ValidateOnBuild(i_BuildPlayerOptions.options.HasFlag(BuildOptions.Development)))
                throw new Exception("Project has issues\nBuilding Stopped.");
        }

        static public void BuildAddresssableAssetPreBuild()
        {
            UnityEngine.Debug.Log("BuildState: BuildAddresssableAssetPreBuild start");
            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent();
            UnityEngine.Debug.Log("BuildState: BuildAddresssableAssetPreBuild done");
        }

        #region Force Sprites Options
        public static void ForceSpriteAtlasOptions()
        {
            string[] guids1 = AssetDatabase.FindAssets("t:SpriteAtlas", null);

            for (int i = 0; i < guids1.Length; i++)
            {
                SpriteAtlas spriteAtlas = (SpriteAtlas)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids1[i]), typeof(SpriteAtlas));

                ForceSpriteAtlasPlatform(spriteAtlas, BuildTarget.iOS.ToString(), TextureImporterFormat.ASTC_4x4);
                //This is deprecated but for some reason it's the one that changes the editor variables. Just in case, leaving it here
                ForceSpriteAtlasPlatform(spriteAtlas, "iPhone", TextureImporterFormat.ASTC_4x4);

                //Debug.LogError(spriteAtlas.name, spriteAtlas);
            }
        }
        private static void ForceSpriteAtlasPlatform(SpriteAtlas i_SpriteAtlas, string i_BuildTarget, TextureImporterFormat i_Format)
        {
            TextureImporterPlatformSettings settings = i_SpriteAtlas.GetPlatformSettings(i_BuildTarget);
            settings.overridden = true;
            settings.format = i_Format;
            i_SpriteAtlas.SetPlatformSettings(settings);
        }

        public static void ForceSpriteOptions()
        {
            string[] guids1 = AssetDatabase.FindAssets("t:Texture2D", null);

            for (int i = 0; i < guids1.Length; i++)
            {
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids1[i]), typeof(Texture2D));

                //getting error in some cases. Using Try to bypass that
                try
                {
                    TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guids1[i]));

                    if (textureImporter.textureType == TextureImporterType.Sprite)
                    {
                        ForceSpritePlatform(textureImporter, BuildTarget.iOS.ToString(), TextureImporterFormat.ASTC_4x4);
                        //This is deprecated but for some reason it's the one that changes the editor variables. Just in case, leaving it here
                        ForceSpritePlatform(textureImporter, "iPhone", TextureImporterFormat.ASTC_4x4);

                        //Debug.LogError(texture.name, texture);
                    }
                }
                catch { }
            }
        }

        private static void ForceSpritePlatform(TextureImporter i_TextureImporter, string i_BuildTarget, TextureImporterFormat i_Format)
        {
            TextureImporterPlatformSettings settings = i_TextureImporter.GetPlatformTextureSettings(i_BuildTarget);
            settings.overridden = true;
            settings.format = i_Format;
            i_TextureImporter.SetPlatformTextureSettings(settings);
        }

        #endregion
    }
}