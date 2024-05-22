using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using static KobGamesSDKSlim.ProjectValidator.Preferences;

namespace KobGamesSDKSlim.ProjectValidator
{
	/// <summary>
	/// Drawing Preference Settings
	/// </summary>
	public class ValidatorSettingsProvider : SettingsProvider
	{
		public ValidatorSettingsProvider(string path, SettingsScope scopes = SettingsScope.User, IEnumerable<string> keywords = null)
			: base(path, scopes, keywords) { }

		public override void OnGUI(string searchContext)
		{
			//If it is not init we don't draw
			if (!IsInit)
				return;

			//Prepare GUI Settings
			GUIStyle style = new GUIStyle
			                 {
				                 normal =
				                 {
					                 textColor = Color.red
				                 },
				                 fontSize  = 14,
				                 fontStyle = FontStyle.Bold
			                 };

			EditorGUI.BeginChangeCheck();

			//Draw Development Build
			if(MachineData.ProjectData != null)
				MachineData.ProjectData.DisableDevelopmentBuild = Toggle("Disable In Dev Build", MachineData.ProjectData.DisableDevelopmentBuild);

			Space(20);

			//Draw Danger Zone Toggle
			BeginHorizontal();
			MachineData.DangerZoneEnabled = Toggle(MachineData.DangerZoneEnabled, GUILayout.Width(20));
			LabelField("DANGER ZONE. ENABLE AT YOUR OWN RISK!!!!", style);
			EndHorizontal();

			if (MachineData.DangerZoneEnabled)
			{
				//Draw Enabled Modules
				LabelField("Enabled Build Validator Modules");
				foreach (var module in MachineData.EnabledBuildModules)
				{
					module.Value = Toggle(module.Key.Skip(4), module.Value);
				}

				Space();

				//Draw Ignore Folders
				BeginHorizontal();
				LabelField("Ignore Folders");
				if (GUILayout.Button("+", GUILayout.Width(30)))
					MachineData.IgnoreFolders.Add("");
				EndHorizontal();
				for (var i = 0; i < MachineData.IgnoreFolders.Count; i++)
				{
					BeginHorizontal();
					MachineData.IgnoreFolders[i] = TextField(MachineData.IgnoreFolders[i]);
					if (GUILayout.Button("-", GUILayout.Width(30)))
					{
						MachineData.IgnoreFolders.RemoveAt(i);
						break;
					}

					EndHorizontal();
				}
			}


			EditorGUI.EndChangeCheck();

			//Save Data
			if (GUI.changed)
			{
				EditorPrefs.SetString(C_MACHINE_DATA_PATH, JsonUtility.ToJson(MachineData));
				EditorUtility.SetDirty(MachineData.ProjectData);
			}
		}
	}

	

}