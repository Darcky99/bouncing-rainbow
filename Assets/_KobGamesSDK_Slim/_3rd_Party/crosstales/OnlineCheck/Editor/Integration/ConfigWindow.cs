#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Crosstales.OnlineCheck.EditorUtil;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor window extension.</summary>
   public class ConfigWindow : ConfigBase
   {
      #region Variables

      private int tab;
      private int lastTab;

      private Vector2 scrollPosPrefabs;
      private Vector2 scrollPosTD;

      #endregion


      #region EditorWindow methods

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Configuration...", false, EditorHelper.MENU_ID + 1)]
      public static void ShowWindow()
      {
         GetWindow(typeof(ConfigWindow));
      }

      public static void ShowWindow(int tab)
      {
         ConfigWindow window = GetWindow(typeof(ConfigWindow)) as ConfigWindow;
         if (window != null) window.tab = tab;
      }

      private void OnEnable()
      {
         titleContent = new GUIContent(Util.Constants.ASSET_NAME_SHORT, EditorHelper.Logo_Asset_Small);
      }

      private void OnGUI()
      {
         tab = GUILayout.Toolbar(tab, new[] {"Config", "Prefabs", "TD", "Help", "About"});

         if (tab != lastTab)
         {
            lastTab = tab;
            GUI.FocusControl(null);
         }

         switch (tab)
         {
            case 0:
            {
               showConfiguration();

               EditorHelper.SeparatorUI();

               GUILayout.BeginHorizontal();
               {
                  if (GUILayout.Button(new GUIContent(" Save", EditorHelper.Icon_Save, "Saves the configuration settings for this project")))
                  {
                     save();
                  }

                  if (GUILayout.Button(new GUIContent(" Reset", EditorHelper.Icon_Reset, "Resets the configuration settings for this project.")))
                  {
                     if (EditorUtility.DisplayDialog("Reset configuration?", $"Reset the configuration of {Util.Constants.ASSET_NAME}?", "Yes", "No"))
                     {
                        Util.Config.Reset();
                        EditorConfig.Reset();
                        save();
                     }
                  }
               }
               GUILayout.EndHorizontal();

               GUILayout.Space(6);
               break;
            }
            case 1:
               showPrefabs();
               break;
            case 2:
               showTestDrive();
               break;
            case 3:
               showHelp();
               break;
            default:
               showAbout();
               break;
         }
      }

      private void OnInspectorUpdate()
      {
         Repaint();
      }

      #endregion


      #region Private methods

      private void showPrefabs()
      {
         scrollPosPrefabs = EditorGUILayout.BeginScrollView(scrollPosPrefabs, false, false);
         {
            GUILayout.Label("Available Prefabs", EditorStyles.boldLabel);

            GUILayout.Space(6);

            GUI.enabled = !EditorHelper.isOnlineCheckInScene;

            GUILayout.Label(Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME);

            if (GUILayout.Button(new GUIContent(" Add", EditorHelper.Icon_Plus, $"Adds an {Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME}-prefab to the scene.")))
            {
               EditorHelper.InstantiatePrefab(Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME);
            }

            GUI.enabled = !EditorHelper.isPingInScene;
            EditorHelper.SeparatorUI();

            GUILayout.Label(Util.Constants.PINGCHECK_SCENE_OBJECT_NAME);
            if (GUILayout.Button(new GUIContent(" Add", EditorHelper.Icon_Plus, "Adds a 'PingCheck'-prefab to the scene.")))
            {
               EditorHelper.InstantiatePrefab(Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/PingCheck/Resources/Prefabs/");
            }

            EditorHelper.SeparatorUI();

            GUI.enabled = !EditorHelper.isSpeedTestInScene;

            GUILayout.Label(Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME);
            if (GUILayout.Button(new GUIContent(" Add", EditorHelper.Icon_Plus, "Adds a 'SpeedTest'-prefab to the scene.")))
            {
               EditorHelper.InstantiatePrefab(Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/SpeedTest/Resources/Prefabs/");
            }
/*
            GUI.enabled = !EditorHelper.isSpeedTestNETInScene;
            GUILayout.Space(6);

            GUILayout.Label(Util.Constants.SPEEDTESTNET_SCENE_OBJECT_NAME);
            if (GUILayout.Button(new GUIContent(" Add", EditorHelper.Icon_Plus, "Adds a 'SpeedTestNET'-prefab to the scene.")))
            {
               EditorHelper.InstantiatePrefab(Util.Constants.SPEEDTESTNET_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/SpeedTestNET/Resources/Prefabs/");
            }
*/
            GUI.enabled = !EditorHelper.isProxyInScene;
            EditorHelper.SeparatorUI();

            GUILayout.Label(Util.Constants.PROXY_SCENE_OBJECT_NAME);
            if (GUILayout.Button(new GUIContent(" Add", EditorHelper.Icon_Plus, "Adds a 'Proxy'-prefab to the scene.")))
            {
               EditorHelper.InstantiatePrefab(Util.Constants.PROXY_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/Proxy/Resources/Prefabs/");
            }

            GUI.enabled = true;

            if (EditorHelper.isOnlineCheckInScene && EditorHelper.isProxyInScene && EditorHelper.isPingInScene && EditorHelper.isSpeedTestInScene) // && EditorHelper.isSpeedTestNETInScene)
            {
               GUILayout.Space(6);
               EditorGUILayout.HelpBox("All available prefabs are already in the scene.", MessageType.Info);
            }

            GUILayout.Space(6);
         }
         EditorGUILayout.EndScrollView();
      }

      private void showTestDrive()
      {
         GUILayout.Space(3);
         GUILayout.Label("Test-Drive", EditorStyles.boldLabel);

         if (Util.Helper.isEditorMode)
         {
            if (EditorHelper.isOnlineCheckInScene)
            {
               scrollPosTD = EditorGUILayout.BeginScrollView(scrollPosTD, false, false);
               {
                  GUILayout.Label($"Internet Available:\t{(OnlineCheck.Instance.isInternetAvailable ? "Yes" : "No")}");
#if UNITY_2019_1_OR_NEWER
                  GUILayout.Label($"Reachability:\t\t{OnlineCheck.Instance.NetworkReachabilityShort}");
#else
                  GUILayout.Label($"Reachability:\t{OnlineCheck.Instance.NetworkReachabilityShort}");
#endif
                  GUILayout.Label($"Public IP:\t\t{Util.NetworkInfo.LastPublicIP}");
                  GUILayout.Label("Active Interfaces:");
                  GUILayout.Label(Util.NetworkInfo.LastNetworkInterfaces.CTDump());

                  EditorHelper.SeparatorUI();

                  GUILayout.Label("Checks", EditorStyles.boldLabel);
                  GUILayout.Label($"Last checked:\t{OnlineCheck.Instance.LastCheck}");
                  GUILayout.Label($"Total:\t\t{Util.Context.NumberOfChecks}");
               }
               if (!Util.Helper.isEditorMode)
               {
                  GUILayout.Label($"Per Minute:\t{Util.Context.ChecksPerMinute:#0.0}");
                  GUILayout.Label($"Data downloaded:\t{Util.Helper.FormatBytesToHRF(OnlineCheck.Instance.DataDownloaded)}");

                  EditorHelper.SeparatorUI();

                  GUILayout.Label("Timers", EditorStyles.boldLabel);
                  GUILayout.Label($"Runtime:\t\t{Util.Helper.FormatSecondsToHourMinSec(Util.Context.Runtime)}");
                  GUILayout.Label($"Uptime:\t\t{Util.Helper.FormatSecondsToHourMinSec(Util.Context.Uptime)}");
                  GUILayout.Label($"Downtime:\t\t{Util.Helper.FormatSecondsToHourMinSec(Util.Context.Downtime)}");
               }

               EditorHelper.SeparatorUI();

               EditorGUILayout.EndScrollView();

               EditorHelper.SeparatorUI();

               if (Util.Helper.isEditorMode)
               {
                  if (GUILayout.Button(new GUIContent(" Refresh", EditorHelper.Icon_Reset, "Restart the Internet availability check.")))
                  {
                     OnlineCheck.Instance.Refresh();
                     Util.NetworkInfo.Refresh();
                  }
               }
            }
            else
            {
               EditorHelper.OCUnavailable();
            }
         }
         else
         {
            EditorGUILayout.HelpBox("Disabled in Play-mode!", MessageType.Info);
         }
      }

      #endregion
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)