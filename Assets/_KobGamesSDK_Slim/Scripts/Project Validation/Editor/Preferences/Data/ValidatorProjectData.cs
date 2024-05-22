using UnityEngine;

namespace KobGamesSDKSlim.ProjectValidator
{
	/// <summary>
	/// Data Saved inside Scriptable Object
	/// </summary>
	[CreateAssetMenu(fileName = "Validator Project Data", menuName = "KobGames/Project Validator/Validator Project Data")]
	public class ValidatorProjectData : ScriptableObject
	{
		/// <summary>
		/// Bool used to disable Validator for Development Builds
		/// </summary>
		public bool DisableDevelopmentBuild;
	}
}