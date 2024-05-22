using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace KobGamesSDKSlim
{
	[CustomEditor(typeof(ExtendedButton_RV), true)]
	public class ExtendedButton_RVEditor : ExtendedButtonEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			ExtendedButton_RV target = (ExtendedButton_RV)this.target;

			//Serialized Properties
			var disableButtonAfterAdShown = serializedObject.FindProperty(nameof(target.DisableButtonAfterAdShown));
			var disableVisualOnClick      = serializedObject.FindProperty(nameof(target.DisableVisualOnClick));
			var openConfirmationScreen  = serializedObject.FindProperty(nameof(target.OpenConfirmationScreen));

			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("RV Group", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(openConfirmationScreen);

			EditorGUILayout.PropertyField(disableButtonAfterAdShown);
			EditorGUILayout.PropertyField(disableVisualOnClick);

			serializedObject.ApplyModifiedProperties();
		}
	}
}