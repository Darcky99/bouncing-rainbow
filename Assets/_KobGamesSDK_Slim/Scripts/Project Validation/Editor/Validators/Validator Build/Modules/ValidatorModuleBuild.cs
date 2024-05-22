using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Build
{
	/// <summary>
	/// Base class for Validator Modules use for Builds
	/// </summary>
	public abstract class ValidatorModuleBuild
	{
		public abstract eBuildValidatorResult Validate();

	#region Utils

		/// <summary>
		/// Triggers a Unity Log that appends a GameObject Path and Reference to a Unity Log
		/// </summary>
		/// <param name="i_Log"></param>
		/// <param name="i_GameObject"></param>
		/// <param name="i_IsError"></param>
		protected static string showLogWithHierarchyPath(string i_Log, GameObject i_GameObject, bool i_IsError)
		{
			var message = i_Log + "\n" + "Path - " + getObjectHierarchyPath(i_GameObject);

			if (i_IsError)
				Debug.LogError(message, i_GameObject);
			else
				Debug.LogWarning(message, i_GameObject);

			return message;
		}

		/// <summary>
		/// Get Object Hierarchy Path
		/// </summary>
		/// <param name="i_GameObject"></param>
		/// <returns></returns>
		private static string getObjectHierarchyPath(GameObject i_GameObject)
		{
			var paths     = new List<string>();
			var finalPath = "";

			//Recreating Path based on Parents of the GameObject
			while (i_GameObject != null)
			{
				paths.Add(i_GameObject.name);
				if (i_GameObject.transform.parent != null)
					i_GameObject = i_GameObject.transform.parent.gameObject;
				else
					break;
			}

			//By default we assume that the top Parent is not found. This is to avoid confusion in corner cases
			string objectParentLocalization = "Parent Not Found";
			//Then we check the evaluated object dictionary which contains a KeyValuePair of string (origin of the object) gameobject (the object itself)
			//If we find the object there we will pick the correct parent location
			foreach (var keyValuePair in ValidatorBuild.EvaluatedObjectsDictionary)
			{
				if (keyValuePair.Value.Contains(i_GameObject))
				{
					objectParentLocalization = keyValuePair.Key;
					break;
				}
			}

			paths.Add(objectParentLocalization);

			//Reversing the hierarchy and separating by /
			paths.Reverse();
			paths.ForEach(x => finalPath += "/" + x);

			return finalPath;
		}

	#endregion
	}
}