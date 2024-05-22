using System;
using System.Linq;
using KobGamesSDKSlim.ProjectValidator.Modules;
using static KobGamesSDKSlim.ProjectValidator.ValidatorUtils;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Compilation
{
	/// <summary>
	/// Class that handles Validation related to Project Compilation
	/// </summary>
	public class ValidatorCompilation
	{
		public static void Validate()
		{
			//List of Modules. It is reorder using ValidatorModuleOrderAttribute
			var types = getInheritedClassesTypes(typeof(ValidatorModuleCompilation))
			           .OrderBy(type => getOrderAttribute(type)?.Order ?? 0).ToList();

			//Get ValidatorModuleOrderAttribute
			ValidatorModuleOrderAttribute getOrderAttribute(Type i_Type) =>
				Attribute.GetCustomAttributes(i_Type).ToList()
				         .Find(attribute => attribute.GetType() == typeof(ValidatorModuleOrderAttribute)) as ValidatorModuleOrderAttribute;

			//Validate each Module
			types.ForEach(type => ((ValidatorModuleCompilation)Activator.CreateInstance(type)).Validate());
		}
	}
}