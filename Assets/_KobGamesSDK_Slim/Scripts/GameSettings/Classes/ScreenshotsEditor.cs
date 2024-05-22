using System;
using System.Collections;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using ShadowResolution = UnityEngine.ShadowResolution;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.Universal;
#endif

namespace KobGamesSDKSlim
{
    [Serializable]
    public class ScreenshotsEditor
    {
#if UNITY_EDITOR
        [Title("General")]
        public bool ShowLogOnCreation = true;
        public bool OpenOnFinish = true;
        public bool IsScreenShotMode = false;

        [Title("Camera")]
        public Camera Camera;
        //public bool IsTransparent = false;

        [Title("Screen Size")]
        public int ScreenWidth;
        public int ScreenHeight;
        [Range(1, 5)]
        public int ScreenSizeMultiplier = 1;

        [Button(ButtonSizes.Medium)]
        public void SetAsGameScreenSize()
        {
            ScreenWidth = (int)Handles.GetMainGameViewSize().x;
            ScreenHeight = (int)Handles.GetMainGameViewSize().y;
        }

        [Button(ButtonSizes.Medium, Name = "Set As 1080x1920")]
        public void SetAs1080x1920()
        {
            GameViewSize.SelectSize(GameViewSize.SetCustomSize(1080, 1920));

            ScreenWidth = (int)Handles.GetMainGameViewSize().x;
            ScreenHeight = (int)Handles.GetMainGameViewSize().y;
        }

        [HorizontalGroup("1024500"), Button(ButtonSizes.Medium, Name = "Set As 1024x500")]
        public void SetAs1024x500()
        {
            GameViewSize.SelectSize(GameViewSize.SetCustomSize(1024, 500));

            ScreenWidth = (int)Handles.GetMainGameViewSize().x;
            ScreenHeight = (int)Handles.GetMainGameViewSize().y;
        }

        [BoxGroup("Save Path")]
        [InlineButton(nameof(BrowseSavePath), "Browse"), ShowInInspector]
        public string SavePath { get => EditorPrefs.GetString(Constants.k_ScreenShotsEditorPrefKey, "/Users/kobyle/Dropbox/Games/KobGamesSDK/"); set => EditorPrefs.SetString(Constants.k_ScreenShotsEditorPrefKey, value); }

        [BoxGroup("Save Path"), PropertyOrder(2)]
        public bool SeperateToResolutionFolders = true;

        public void BrowseSavePath()
        {
            SavePath = EditorUtility.SaveFolderPanel("Path to Save Images", SavePath, Application.dataPath);
        }

        private IEnumerator m_ScreenshotCoroutine;

        public Tuple<int, int> GetCurrentScreenSize()
        {
            return new Tuple<int, int>((int)Handles.GetMainGameViewSize().x, (int)Handles.GetMainGameViewSize().y);
        }

        // iPhone
        private readonly Tuple<int, int, string> r_Iphone8 = new Tuple<int, int, string>(1242, 2688, "AppStore/1242x2688 - Iphone 6.5");
        private readonly Tuple<int, int, string> r_Iphone5 = new Tuple<int, int, string>(1242, 2208, "AppStore/1242x2208 - Iphone 5.5");
        //private readonly Tuple<int, int, string> r_IphoneX = new Tuple<int, int, string>(1125, 2436, "AppStore/1125x2436 - Iphone 5.8");
        private readonly Tuple<int, int, string> r_IphoneIpad = new Tuple<int, int, string>(2048, 2732, "AppStore/2048x2732 - iPAD");

        // Android
        private readonly Tuple<int, int, string> r_Android7Inch = new Tuple<int, int, string>(600, 1024, "GooglePlay/600x1024");
        private readonly Tuple<int, int, string> r_AndroidFullHD = new Tuple<int, int, string>(1080, 1920, "GooglePlay/1080x1920");
        private readonly Tuple<int, int, string> r_Android10Inch = new Tuple<int, int, string>(1200, 1920, "GooglePlay/1200x1920");

        [HorizontalGroup("1024500"), Button(ButtonSizes.Medium, Name = "Take ScreenShot 1024x500")]
        public void TakeScreenshot1024x500()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { new Tuple<int, int, string>(ScreenWidth, ScreenHeight, $"{ScreenWidth}x{ScreenHeight}") });
        }

        [Button(ButtonSizes.Medium)]
        public void TakeScreenshot()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { new Tuple<int, int, string>(ScreenWidth, ScreenHeight, $"{ScreenWidth}x{ScreenHeight}") });
        }

        [HorizontalGroup("1"), Button("1242x2688 - Iphone 6.5", ButtonSizes.Gigantic)]
        public void TakeScreenshotIphone6point5()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_Iphone8 });
        }

        [HorizontalGroup("1"), Button("1242x2208 - Iphone 5.5", ButtonSizes.Gigantic)]
        public void TakeScreenshotIphone5point5()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_Iphone5 });
        }

        [HorizontalGroup("1"), Button("2048x2732 (iPAD)", ButtonSizes.Gigantic)]
        public void TakeScreenshotIpad()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_IphoneIpad });
        }

        [HorizontalGroup("2"), Button("600x1024 (7'')", ButtonSizes.Gigantic)]
        public void TakeScreenshotGP1()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_Android7Inch });
        }

        [HorizontalGroup("2"), Button("1080x1920 (FullHD)", ButtonSizes.Gigantic)]
        public void TakeScreenshotGP2()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_AndroidFullHD });
        }

        [HorizontalGroup("2"), Button("1200x1920 (10'')", ButtonSizes.Gigantic)]
        public void TakeScreenshotGP3()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_Android10Inch });
        }

        [Button("Take All Screenshots At Once (iPhone)", ButtonSizes.Large)]
        public void TakeAllScreenshotsAtOnceIphone()
        {
            //TakeScreenShot(new Tuple<int, int, string>[] { r_Iphone8, r_Iphone5, r_IphoneX, r_IphoneIpad });
            TakeScreenShot(new Tuple<int, int, string>[] { r_Iphone8, r_Iphone5, r_IphoneIpad });
        }

        [Button("Take All Screenshots At Once (Android)", ButtonSizes.Large)]
        public void TakeAllScreenshotsAtOnceAndroid()
        {
            TakeScreenShot(new Tuple<int, int, string>[] { r_Android7Inch, r_AndroidFullHD, r_Android10Inch });
        }

        [Button("Prepare Fastlane", ButtonSizes.Large)]
        public void PrepareFastlaneScreenshots()
        {
            var resolutions = new Tuple<int, int, string>[] { r_Iphone8, r_Iphone5, r_IphoneIpad, r_IphoneIpad };
            var destPath = "AppStore/FastLane_Screenshots/en-US";

            int resolutionIndex = 1;

            if (Directory.Exists($"{SavePath}/{destPath}"))
            {
                Directory.Delete($"{SavePath}/{destPath}", true);
            }

            Directory.CreateDirectory($"{SavePath}/{destPath}");

            foreach (var resolution in resolutions)
            {
                var path = Path.Combine(SavePath, resolution.Item3);

                if (Directory.Exists(path))
                {
                    var files = Directory.GetFileSystemEntries(path);

                    foreach (var file in files)
                    {
                        if (file.Contains("png"))
                        {
                            var addition = resolutionIndex == 4 ? "_ipadPro129_" : "";

                            File.Copy(file, $"{SavePath}/{destPath}/{resolutionIndex}_{addition}{Path.GetFileName(file)}", true);
                            //Debug.LogError(Path.GetFileName(file));
                        }
                    }
                }

                resolutionIndex++;
            }
        }

        private const string k_VMFastlaneScreenshots = "/Users/kobyle/Dropbox/Shared-VM/Screenshots";

        public string VMFastlaneScreenshotsPath => $"{k_VMFastlaneScreenshots}/{GameSettings.Instance.General.GameName}/FastLane_Screenshots";

        [Button("Copy Fastlane Screenshots To VM", ButtonSizes.Large)]
        public void CopyFastlaneScreenshotsToVM()
        {
            var destPathNoLang = "AppStore/FastLane_Screenshots";

            Utils.CopyFolder($"{SavePath}/{destPathNoLang}", VMFastlaneScreenshotsPath, i_Overwrite: true);

            GameSettings.Instance.General.SetScreenshotsPath();
        }

        private void TakeScreenShot(Tuple<int, int, string>[] i_Resolutions)
        {
            if (SavePath == string.Empty)
                BrowseSavePath();

            if (SavePath != string.Empty)
            {
                if (EditorApplication.isPaused || !EditorApplication.isPlaying)
                {
                    EditorApplication.update += coroutineHandler;

                    m_ScreenshotCoroutine = TakeScreenShotCo(i_Resolutions);
                }
                else
                {
                    Managers.Instance.StartCoroutine(TakeScreenShotCo(i_Resolutions));
                }
            }
        }

        void coroutineHandler()
        {
            m_ScreenshotCoroutine.MoveNext();
        }

        private IEnumerator TakeScreenShotCo(Tuple<int, int, string>[] i_Resolutions)
        {
            string      path        = string.Empty;
            EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
            

            //Original values
            bool eventSystemStateOrg = eventSystem != null ? eventSystem.gameObject.activeSelf : true;
            //Standard Render Pipeline
            ShadowResolution shadowResolutionOrg = QualitySettings.shadowResolution;
            int antiAliasingOrg = QualitySettings.antiAliasing;
            //URP
            var renderPipeline   = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
            var renderPipelineHQ = getURPAssetHQ();
            
            
            //Best Values
            Time.timeScale = 0;
            eventSystem?.gameObject.SetActive(false);
            //Standard Render Pipeline
            QualitySettings.antiAliasing = 8;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            //URP
            if(renderPipeline != null)
                renderPipelineHQ.shadowDistance = renderPipeline.shadowDistance;
            QualitySettings.renderPipeline  = renderPipelineHQ;

            
            foreach (Tuple<int, int, string> resolution in i_Resolutions)
            {
                path = ScreenShotName(resolution, SavePath, SeperateToResolutionFolders);
                GameViewSize.BackupCurrentSize();
                GameViewSize.SelectSize(GameViewSize.SetCustomSize(resolution.Item1 * ScreenSizeMultiplier, resolution.Item2 * ScreenSizeMultiplier));

                yield return new WaitForSecondsRealtime(.1f);

                if (Application.isPlaying && EditorApplication.isPaused)
                    EditorApplication.Step();

                ScreenCapture.CaptureScreenshot(path);

                yield return new WaitForSecondsRealtime(.1f);

                if (Application.isPlaying && EditorApplication.isPaused)
                    EditorApplication.Step();

                yield return new WaitForSecondsRealtime(.1f);

                GameViewSize.RestoreSize();
            }

            if (ShowLogOnCreation)
                Debug.LogErrorFormat(string.Format("Finished taking {0} screenshots", i_Resolutions.Length));

            if (OpenOnFinish && i_Resolutions.Length == 1)
                Application.OpenURL(path);

            EditorApplication.update -= coroutineHandler;

            
            //Retrieving original values
            eventSystem?.gameObject.SetActive(eventSystemStateOrg);
            //Standard Render Pipeline
            QualitySettings.antiAliasing = antiAliasingOrg;
            QualitySettings.shadowResolution = shadowResolutionOrg;
            //URP
            QualitySettings.renderPipeline = renderPipeline;

            
            Time.timeScale = 1;
        }

        private UniversalRenderPipelineAsset getURPAssetHQ()
        {
            var guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[] { "Assets" });

            
            foreach (string guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) == "URP_Asset HQ")
                    return AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
            }
            
            Debug.LogError("Couldn't find HQ version of URP");
            return null;
        }

        public string ScreenShotName(Tuple<int, int, string> i_Resolution, string i_Path, bool i_SeperateToResolutionFolders)
        {
            string strPath = string.Empty;
            string filename = string.Format("{0}x{1}_{2}.png", i_Resolution.Item1, i_Resolution.Item2, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

            if (i_SeperateToResolutionFolders)
            {
                strPath = string.Format("{0}/{1}", i_Path, i_Resolution.Item3);
            }
            else
            {
                strPath = i_Path;
            }

            if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);

            strPath = string.Format("{0}/{1}", strPath, filename);

            return strPath;
        }

        [OnInspectorGUI]
        void OnInspector()
        {
            if (Camera == null) Camera = GameObject.FindObjectOfType<Camera>();

            if (ScreenWidth == 0 || ScreenHeight == 0)
                SetAsGameScreenSize();
        }
#endif
    }
}