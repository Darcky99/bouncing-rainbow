using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static KobGamesSDKSlim.ProjectValidator.ValidatorUtils;

namespace KobGamesSDKSlim.ProjectValidator
{
	/// <summary>
	/// Data Saved inside Editor Preferences
	/// </summary>
	[Serializable]
	public class ValidatorMachineData
	{
		/// <summary>
		/// Scriptable Object that holds Project Specific Data.
		/// This is not saved. Here only for organization purposes
		/// </summary>
		public ValidatorProjectData ProjectData
		{
			get
			{
				if(m_ProjectData == null)
				{
					var dataSO = GetAssets<ValidatorProjectData>();
					if (dataSO == null || dataSO.Count == 0)
					{
						Debug.LogError("Couldn't Find Validator Project Data");
						m_ProjectData = default;
					}

					var firstSO = dataSO.First();
					
					if(dataSO.Count > 1)
						Debug.LogError($"There are more than 1 Validator Project Data scriptable object. Will use {firstSO.name}", firstSO);

					m_ProjectData = firstSO;
				}

				return m_ProjectData;
			}
			
		}
		[NonSerialized]
		private ValidatorProjectData m_ProjectData;
		
		/// <summary>
		/// Enable Danger Zone to disable modules
		/// This is not saved as well. Here for organization purposes
		/// </summary>
		[NonSerialized]
		public bool DangerZoneEnabled = false;
		
		/// <summary>
		/// List of all Modules defining if it is Enabled or not
		/// </summary>
		public List<ValidatorModulesEnabled> EnabledBuildModules = new List<ValidatorModulesEnabled>();

		/// <summary>
		/// Folders to ignore from Validation
		/// </summary>
		public List<string> IgnoreFolders = new List<string>();
	}
	
	/// <summary>
	/// KeyValuePair structure used for Enabled Modules List.
	/// Could be a Dictionary but i found it easier this way
	/// </summary>
	[Serializable]
	public class ValidatorModulesEnabled
	{
		public string Key;
		public bool   Value;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Name of the Validator Module</param>
		/// <param name="value">Is Enabled bool</param>
		public ValidatorModulesEnabled(string key, bool value)
		{
			Key   = key;
			Value = value;
		}
	}

	
}