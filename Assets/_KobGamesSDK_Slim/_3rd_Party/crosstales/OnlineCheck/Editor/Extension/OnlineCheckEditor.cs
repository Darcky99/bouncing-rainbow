#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.OnlineCheck.EditorExtension
{
   /// <summary>Custom editor for the 'OnlineCheck'-class.</summary>
   [InitializeOnLoad]
   [CustomEditor(typeof(OnlineCheck))]
   public class OnlineCheckEditor : Editor
   {
      #region Variables

      private OnlineCheck script;

      #endregion


      #region Static constructor

      static OnlineCheckEditor()
      {
         EditorApplication.hierarchyWindowItemOnGUI += hierarchyItemCB;
      }

      #endregion


      #region Editor methods

      private void OnEnable()
      {
         script = (OnlineCheck)target;
         EditorApplication.update += onUpdate;
         //onUpdate();
      }

      private void OnDisable()
      {
         EditorApplication.update -= onUpdate;
      }

      public override void OnInspectorGUI()
      {
         DrawDefaultInspector();

         if (!script.Microsoft && !script.Google204 && !script.GoogleBlank && !script.Apple && !script.Ubuntu && script.CustomCheck == null)
            EditorGUILayout.HelpBox("No check selected - please select at least one or add a custom check!", MessageType.Warning);

         if (script.CustomCheck != null && !string.IsNullOrEmpty(script.CustomCheck.URL) && (script.CustomCheck.URL.Contains("crosstales.com") || script.CustomCheck.URL.Contains("207.154.226.218")))
            EditorGUILayout.HelpBox($"'Custom Check' uses 'crosstales.com' for detection: this is only allowed for test-builds and the check interval will be limited!{System.Environment.NewLine}Please use your own URL for detection.", MessageType.Warning);

         EditorUtil.EditorHelper.SeparatorUI();

         if (script.isActiveAndEnabled)
         {
            //onUpdate();
            GUILayout.Label("Network Environment", EditorStyles.boldLabel);

            /*
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Internet Available:");
                GUI.enabled = false;
                EditorGUILayout.Toggle(new GUIContent(string.Empty, "Is Internet currently available?"), script.isInternetAvailable);
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            */

            GUILayout.Label($"Internet Available:\t{(script.isInternetAvailable ? "Yes" : "No")}");

            GUILayout.Label($"Reachability:{Util.Constants.TAB}{script.NetworkReachabilityShort}");
            GUILayout.Label($"Public IP:\t\t{Util.NetworkInfo.LastPublicIP}");
            GUILayout.Label("Active Interfaces:");
            GUILayout.Label(Util.NetworkInfo.LastNetworkInterfaces.CTDump());

            EditorUtil.EditorHelper.SeparatorUI();

            GUILayout.Label("Checks", EditorStyles.boldLabel);
            GUILayout.Label($"Last checked:{Util.Constants.TAB}{script.LastCheck}");
            GUILayout.Label($"Total:{Util.Constants.TAB}\t{Util.Context.NumberOfChecks}");

            if (!Util.Helper.isEditorMode)
            {
               GUILayout.Label($"Checks/Minute:{Util.Constants.TAB}{Util.Context.ChecksPerMinute:#0.0}");
               GUILayout.Label($"Data Downloaded:\t{Util.Helper.FormatBytesToHRF(script.DataDownloaded)}");
               EditorUtil.EditorHelper.SeparatorUI();

               GUILayout.Label("Timers", EditorStyles.boldLabel);
               GUILayout.Label($"Runtime:\t\t{Util.Helper.FormatSecondsToHourMinSec(Util.Context.Runtime)}");
               GUILayout.Label($"Uptime:{Util.Constants.TAB}\t{Util.Helper.FormatSecondsToHourMinSec(Util.Context.Uptime)}");
               GUILayout.Label($"Downtime:\t\t{Util.Helper.FormatSecondsToHourMinSec(Util.Context.Downtime)}");
            }

            if (Util.Helper.isEditorMode)
            {
               GUI.enabled = !script.isBusy;

               if (GUILayout.Button(new GUIContent(" Refresh", EditorUtil.EditorHelper.Icon_Reset, "Restart the Internet availability check.")))
               {
                  script.Refresh();
                  Util.NetworkInfo.Refresh();
               }

               GUI.enabled = true;
            }
         }
         else
         {
            EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
         }
      }

      #endregion


      #region Private methods

      private void onUpdate()
      {
         Repaint();
      }

      private static void hierarchyItemCB(int instanceID, Rect selectionRect)
      {
         if (EditorUtil.EditorConfig.HIERARCHY_ICON)
         {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (go != null && go.GetComponent<OnlineCheck>())
            {
               Rect r = new Rect(selectionRect);
               r.x = r.width - 4;

               GUI.Label(r, EditorUtil.EditorHelper.Logo_Asset_Small);
            }
         }
      }

      #endregion
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)