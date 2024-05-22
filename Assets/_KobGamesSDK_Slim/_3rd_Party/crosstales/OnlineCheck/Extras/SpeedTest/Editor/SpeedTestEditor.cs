#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.OnlineCheck.EditorExtension
{
   /// <summary>Custom editor for the 'SpeedTest'-class.</summary>
   //[InitializeOnLoad]
   [CustomEditor(typeof(Tool.SpeedTest))]
   public class SpeedTestEditor : Editor
   {
      #region Variables

      private Tool.SpeedTest script;

      #endregion


      #region Editor methods

      private void OnEnable()
      {
         script = (Tool.SpeedTest)target;
         EditorApplication.update += onUpdate;
      }

      private void OnDisable()
      {
         EditorApplication.update -= onUpdate;
      }

/*
        public override bool RequiresConstantRepaint()
        {
            return true;
        }
*/
      public override void OnInspectorGUI()
      {
         DrawDefaultInspector();

         if (string.IsNullOrEmpty(script.SmallUrl))
            EditorGUILayout.HelpBox("'Small URL' is empty - testing with 'SMALL' not possible!", MessageType.Warning);
         if (string.IsNullOrEmpty(script.MediumUrl))
            EditorGUILayout.HelpBox("'Medium URL' is empty - testing with 'MEDIUM' not possible!", MessageType.Warning);
         if (string.IsNullOrEmpty(script.LargeUrl))
            EditorGUILayout.HelpBox("'Large URL' is empty - testing with 'LARGE' not possible!", MessageType.Warning);

         EditorUtil.EditorHelper.SeparatorUI();

         if (script.isActiveAndEnabled)
         {
            GUILayout.Label("Speed Test Status", EditorStyles.boldLabel);

            if (script.LastDuration > 0)
            {
               GUILayout.Label($"URL:{Util.Constants.TAB}{script.LastURL}");
               GUILayout.Label($"Speed:{Util.Constants.TAB}{script.LastSpeedMBps:N3} MBps ({script.LastSpeed / 1000000:N3} Mbps)");
               GUILayout.Label($"Duration:\t{script.LastDuration:N3} seconds");
               GUILayout.Label($"Data size:\t{script.LastDataSizeMB:N2} MB");
            }
            else
            {
               EditorGUILayout.HelpBox(script.isBusy ? "Testing the download speed, please wait..." : "Speed not tested.", MessageType.Info);
            }

            if (Util.Helper.isEditorMode)
            {
               GUI.enabled = !script.isBusy;

               if (GUILayout.Button(new GUIContent(" Refresh", EditorUtil.EditorHelper.Icon_Reset, "Restart the speed test.")))
               {
                  script.Test();
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

      #endregion
   }
}
#endif
// © 2020-2021 crosstales LLC (https://www.crosstales.com)