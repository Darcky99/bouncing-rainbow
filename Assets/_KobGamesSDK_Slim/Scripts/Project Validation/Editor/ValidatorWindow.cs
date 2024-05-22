using KobGamesSDKSlim.ProjectValidator.Modules.Build;
using KobGamesSDKSlim.ProjectValidator.Modules.Compilation;
using UnityEditor;

namespace KobGamesSDKSlim.ProjectValidator
{
	[InitializeOnLoad]
	public static class ValidatorWindow
	{
		/// <summary>
		/// Initialization Constructor
		/// </summary>
		static ValidatorWindow()
		{
			validateCompilation();
		}

		/// <summary>
		/// Function called by Validate Menu Button
		/// </summary>
		[MenuItem("KobGamesSDK/Validate Project", false, 60)]
		private static void validateProjectMenu()
		{
			validateCompilation();
			ValidatorBuild.ValidateOnMenu();
		}

		/// <summary>
		/// Build Validation called by Build Processor
		/// </summary>
		/// <param name="i_IsDevelopmentBuild"></param>
		/// <returns></returns>
		public static bool ValidateOnBuild(bool i_IsDevelopmentBuild) => ValidatorBuild.ValidateOnBuild(i_IsDevelopmentBuild);

		/// <summary>
		/// Validate on Compilation
		/// </summary>
		private static void validateCompilation()
		{
			ValidatorCompilation.Validate();
		}
	}
}