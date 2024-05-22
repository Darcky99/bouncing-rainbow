#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.OnlineCheck.EditorExtension
{
   /// <summary>Custom editor for the 'PingCheck'-class.</summary>
   //[InitializeOnLoad]
   [CustomEditor(typeof(Tool.PingCheck))]
   public class PingCheckEditor : Editor
   {
      #region Variables

      private Tool.PingCheck script;

      #endregion


      #region Editor methods

      private void OnEnable()
      {
         script = (Tool.PingCheck)target;
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
#if UNITY_WEBGL
         EditorGUILayout.HelpBox("'Ping Check' is not supported under WebGL!", MessageType.Error);
#endif
         DrawDefaultInspector();

         if (string.IsNullOrEmpty(script.HostName))
            EditorGUILayout.HelpBox("'Host Name' is empty - test not possible!", MessageType.Warning);

         EditorUtil.EditorHelper.SeparatorUI();

         if (script.isActiveAndEnabled)
         {
            GUILayout.Label("Ping Status", EditorStyles.boldLabel);

            if (script.LastPingTimeMilliseconds > 0)
            {
               GUILayout.Label($"Host:\t{script.LastHost}");
               GUILayout.Label($"IP:\t{script.LastIP}");
               GUILayout.Label($"RTT:\t{script.LastPingTimeMilliseconds} ms");
            }
            else
            {
               EditorGUILayout.HelpBox(script.isBusy ? "Testing the ping, please wait..." : "Ping not tested.", MessageType.Info);
            }

            if (Util.Helper.isEditorMode)
            {
               //GUI.enabled = !PingCheck.isBusy;

               if (GUILayout.Button(new GUIContent(" Refresh", EditorUtil.EditorHelper.Icon_Reset, "Restart the Ping check.")))
               {
                  script.Ping();
               }

               //GUI.enabled = true;
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