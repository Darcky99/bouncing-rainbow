using UnityEditor;
using UnityEngine;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Compilation
{
	/// <summary>
	/// Project Settings -> EnterPlayModeOptions should be disabled
	/// </summary>
	public class VMC_EnterPlayOptions : ValidatorModuleCompilation
	{
		public override void Validate()
		{
			if (!EditorSettings.enterPlayModeOptionsEnabled) return;

			Debug.LogError("Current Enter Play Mode Options is set to true and should be false. Switching...");
			EditorSettings.enterPlayModeOptionsEnabled = false;
		}
	}
}