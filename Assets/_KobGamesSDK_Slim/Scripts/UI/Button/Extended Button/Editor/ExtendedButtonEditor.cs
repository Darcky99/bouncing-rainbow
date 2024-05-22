using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.NiceVibrations;

namespace KobGamesSDKSlim
{

    [CustomEditor(typeof(ExtendedButton), true)]
    public class ExtendedButtonEditor : UnityEditor.UI.ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
    
            serializedObject.Update();
              
            ExtendedButton target = (ExtendedButton)this.target;

            //Serialized Properties
            var canDoScaleAnim   = serializedObject.FindProperty(nameof(target.CanDoScaleAnim));
            var scaleSizeType    = serializedObject.FindProperty(nameof(target.ScaleSizeType));
            var customScale      = serializedObject.FindProperty(nameof(target.CustomScale));
            var canDoHaptics     = serializedObject.FindProperty(nameof(target.CanDoHaptics));
            var hapticsType      = serializedObject.FindProperty(nameof(target.HapticType));
            var resetOnDisable   = serializedObject.FindProperty(nameof(target.ResetOnDisable));
            var materialOverride = serializedObject.FindProperty(nameof(target.MaterialOverride));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Extended Button Group", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(canDoScaleAnim, new GUIContent("Do Scale Tween"));
            
            if(target.CanDoScaleAnim)
            {
                EditorGUILayout.PropertyField(scaleSizeType, new GUIContent("Scale Size Type"));
    
                if(target.ScaleSizeType == eButtonScaleSizeType.CustomScale)
                    EditorGUILayout.PropertyField(customScale, new GUIContent("Custom Scale Size"));
            }
            
            EditorGUILayout.PropertyField(canDoHaptics, new GUIContent("Do Haptics"));

            if (target.CanDoHaptics)
                EditorGUILayout.PropertyField(hapticsType);

            EditorGUILayout.PropertyField(resetOnDisable);

            if(!target.ResetOnDisable)
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 14;
                style.normal.textColor = Color.yellow;
                style.wordWrap = true;
                EditorGUILayout.LabelField("WARNING - Reset On Disable is disabled. Don't forget to manually Reset the Button when no longer needed.", style);
            }
    
            EditorGUILayout.PropertyField(materialOverride);

    
            if (GUILayout.Button("SetRefs"))
            {
                if (!target.interactable)
                    Debug.LogError(target.gameObject.name + " - Button Refs should be set in Interactable mode. This will ensure that the Original/Default colors are setup properly", target.gameObject);
                target.SetRefs();
            }
    
            EditorGUILayout.LabelField("Debug");
    
            if (GUILayout.Button("Set Interactable"))
                target.SetInteractable(true);
    
            if (GUILayout.Button("Set Non Interactable"))
                target.SetInteractable(false);
    
      
            serializedObject.ApplyModifiedProperties();
        }
    }
}
