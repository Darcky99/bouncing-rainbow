using System;
using System.Linq;
using KobGamesSDKSlim.ProjectValidator.Modules.Build;
using UnityEditor;
using UnityEngine;
using static KobGamesSDKSlim.ProjectValidator.ValidatorUtils;

namespace KobGamesSDKSlim.ProjectValidator
{
	/// <summary>
	/// Preferences Window for Project Validator
	/// </summary>
	public static class Preferences
	{
		/// <summary>
		/// Validator Settings Window Path
		/// </summary>
		/// <returns></returns>
		[SettingsProvider]
		static SettingsProvider validatorPreferences()
		{
			return new ValidatorSettingsProvider("Preferences/_KobGames/Project Validator"); 
		}

		//Key and Value used to save Preferences data
		//Preferences is saved in EditorPrefs
		public static readonly string C_MACHINE_DATA_PATH = $"{Application.identifier}/ProjectValidator/Serialized Data";
		
		//Keeps track if the preferences system was init
		private static bool m_IsInit = false;
		
		//Data saved in this machine.
		private static ValidatorMachineData m_MachineData = new ValidatorMachineData();

	#region Accessors

		//If is init return true, otherwise try to init
		public static bool IsInit
		{
			get
			{
				if (!m_IsInit)
				{
					init();
				}

				return m_IsInit;
			}
		}
		
		public static ValidatorMachineData MachineData => m_MachineData;
		
		public static bool DisabledDevelopmentBuild => MachineData.ProjectData.DisableDevelopmentBuild;

		public static string[] IgnoreFolders =>
			MachineData.IgnoreFolders.Count > 0 && MachineData.DangerZoneEnabled ? MachineData.IgnoreFolders.Where(value => !string.IsNullOrEmpty(value)).ToArray() : new string[] { };

	#endregion
		
		/// <summary>
		/// Constructor used on Initialize
		/// </summary>
		private static void init()
		{
			//Load is inside try/catch since data might be different from previous versions
			try
			{
				var fetchedData = JsonUtility.FromJson<ValidatorMachineData>(EditorPrefs.GetString(C_MACHINE_DATA_PATH));
				if (fetchedData != null)
					m_MachineData = fetchedData;
			}
			catch
			{
				Debug.LogError("Couldn't recreate ValidatorSerializedData. Will create a new one");
				throw;
			}

			//Get all Validator Modules
			var validatorModules = getInheritedClassesTypes(typeof(ValidatorModuleBuild));

			//Remove Modules from Preference Data that don't exist anymore
			for (int i = m_MachineData.EnabledBuildModules.Count - 1; i >= 0; i--)
			{
				if (validatorModules.Find(module => m_MachineData.EnabledBuildModules[i].Key == module.Name) == null)
					m_MachineData.EnabledBuildModules.RemoveAt(i);
			}

			//Add new Validator Modules to Preferences Data
			foreach (var type in validatorModules)
			{
				if (m_MachineData.EnabledBuildModules.Find(module => module.Key == type.Name) == null)
					m_MachineData.EnabledBuildModules.Add(new ValidatorModulesEnabled(type.Name, true));
			}

			//Set Init
			m_IsInit = true;
		}
		
		/// <summary>
		/// Checks if a certain module is enabled
		/// </summary>
		/// <param name="i_ModuleType"></param>
		/// <returns></returns>
		public static bool IsModuleEnabled(Type i_ModuleType)
		{
			//If danger zone is disabled it means that everything is enabled
			if (!MachineData.DangerZoneEnabled)
				return true;

			var moduleData = m_MachineData.EnabledBuildModules.Find(module => i_ModuleType.Name == module.Key);
			return moduleData == null || moduleData.Value;
		}
	}
}