using System;
using System.Collections.Generic;
using System.Linq;
using KobGamesSDKSlim.ProjectValidator.Modules;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static KobGamesSDKSlim.ProjectValidator.ValidatorUtils;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Build
{
	/// <summary>
	/// Class that handles Validation related to Build
	/// </summary>
	public static class ValidatorBuild
	{
	#region Variables

		//Private
		/// <summary>
		/// Dictionary with key string (prefab / scene name) and value gameobject (object)
		/// </summary>
		private static Dictionary<string, List<GameObject>> m_EvaluatedObjectsDictionary = new Dictionary<string, List<GameObject>>();

		private static List<string> m_WarningsList = new List<string>();
		private static List<string> m_ErrorList    = new List<string>();

		//Public
		public static Dictionary<string, List<GameObject>> EvaluatedObjectsDictionary => m_EvaluatedObjectsDictionary;

		/// <summary>
		/// Get all objects in m_EvaluatedObjectsDictionary
		/// </summary>
		public static List<GameObject> EvaluatedObjectsList
		{
			get
			{
				var allObjects = new List<GameObject>();
				m_EvaluatedObjectsDictionary.ForEachIfNotNull(kvp => allObjects.AddRange(kvp.Value));
				return allObjects;
			}
		}

		public static List<string> WarningsList => m_WarningsList;
		public static List<string> ErrorList    => m_ErrorList;

	#endregion

	#region Validate Public

		/// <summary>
		/// Function called by Validate Menu Button
		/// </summary>
		public static void ValidateOnMenu()
		{
			validate();
		}

		/// <summary>
		/// Called from Build Processor.
		/// </summary>
		/// <param name="i_IsDevelopmentBuild"></param>
		/// <returns></returns>
		public static bool ValidateOnBuild(bool i_IsDevelopmentBuild)
		{
			//Result for Validation
			var validationResult = validate();

			//If is development build and validation is disabled for development build it will allow to continue the build process
			if (i_IsDevelopmentBuild && Preferences.DisabledDevelopmentBuild)
			{
				if (!validationResult)
					Debug.LogError("Project has errors but Validation is disabled for development build. Ensure those errors are fixed for final builds");
				return true;
			}
			else
				return validationResult;
		}

	#endregion

	#region Validate Private

		/// <summary>
		/// Start Validation process from asset setup to module validation
		/// </summary>
		/// <returns></returns>
		private static bool validate()
		{
			openAllScenes();

			initializeAssets(); //Load Assets (Prefabs/Scenes/Etc) into Memory
			initializeVars();   //Fetch All Objects

			var result = validateModules();

			reopenPreviousScene(); //Closes all scene aside from the original opened scene

			switch (result)
			{
				case eBuildValidatorResult.AllGood:
					Debug.LogError("Validator Build - All Good!!!");
					break;
				case eBuildValidatorResult.WarningsOnly:
					Debug.LogError("Validator Build - A Few Warnings");
					break;
				case eBuildValidatorResult.ErrorsWithModuleDisabled:
					Debug.LogError("Validator Build - Ignoring Errors due to Preferences Settings");
					break;
				case eBuildValidatorResult.ErrorsNeedFixing:
					Debug.LogError("Validator Build - Found Errors that need fixing");
					break;
			}

			return result != eBuildValidatorResult.ErrorsNeedFixing;
		}

		/// <summary>
		/// Starts Validation for all Modules and Get Result
		/// </summary>
		/// <returns></returns>
		private static eBuildValidatorResult validateModules()
		{
			//List of Modules. It is reorder using ValidatorModuleOrderAttribute
			var types = getInheritedClassesTypes(typeof(ValidatorModuleBuild))
			           .OrderBy(type => getOrderAttribute(type)?.Order ?? 0).ToList();

			//Get ValidatorModuleOrderAttribute
			ValidatorModuleOrderAttribute getOrderAttribute(Type i_Type) =>
				Attribute.GetCustomAttributes(i_Type).ToList()
				         .Find(attribute => attribute.GetType() == typeof(ValidatorModuleOrderAttribute)) as ValidatorModuleOrderAttribute;


			//Cache all Validator Modules Results
			var results = new List<eBuildValidatorResult>();
			types.ForEach(type =>
			              {
				              //Validate the module and get result
				              var validationResult = ((ValidatorModuleBuild)Activator.CreateInstance(type)).Validate();
								
				              //If Module is disabled and we have errors we change the result type
				              if (validationResult == eBuildValidatorResult.ErrorsNeedFixing && !Preferences.IsModuleEnabled(type))
					              validationResult = eBuildValidatorResult.ErrorsWithModuleDisabled;
				              
				              results.Add(validationResult);
			              });


			if (results.Contains(eBuildValidatorResult.ErrorsNeedFixing)) return eBuildValidatorResult.ErrorsNeedFixing;
			if (results.Contains(eBuildValidatorResult.ErrorsWithModuleDisabled)) return eBuildValidatorResult.ErrorsWithModuleDisabled;
			if (results.Contains(eBuildValidatorResult.WarningsOnly)) return eBuildValidatorResult.WarningsOnly;
			return eBuildValidatorResult.AllGood;
		}

	#endregion


	#region Initialization

		/// <summary>
		/// Loads all assets into memory
		/// </summary>
		private static void initializeAssets()
		{
			var progressTime = Environment.TickCount;

			var allAssetPaths = AssetDatabase.GetAllAssetPaths();
			for (int i = 0; i < allAssetPaths.Length; i++)
			{
				if (Environment.TickCount - progressTime > 250)
				{
					progressTime = Environment.TickCount;
					EditorUtility.DisplayProgressBar("Project Validation", "Searching prefabs", (float)i / (float)allAssetPaths.Length);
				}

				AssetDatabase.LoadMainAssetAtPath(allAssetPaths[i]);
			}

			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// Sets Evaluated Objects Dictionary so we can use them later inside the modules
		/// </summary>
		private static void initializeVars()
		{
			var sceneObjects    = getSceneObjects();                //Objects in the scene
			var sceneReferences = getSceneReferences(sceneObjects); //Prefabs references found in scene objects (include children)
			var prefabObjects   = getProjectPrefabs();              //All Prefabs in the project (include children)

			//Only prefabs with references in the scene or Resource folder. We don't need the others ones since they won't be part of the build
			var prefabsWithReferences = prefabObjects.FindAll(prefab => (sceneReferences.Contains(prefab) || isObjectInFolder(prefab, "Resources")) && !isObjectInFolder(prefab, Preferences.IgnoreFolders)).ToList();

			// sceneReferences.ForEach(prefab => Debug.LogError($"Scene Reference - {prefab.name}", prefab));
			// prefabsWithReferences.ForEach(prefab => Debug.LogError($"Prefab With Reference - {prefab.name}", prefab));
			// Debug.LogError($"Prefabs Count - {prefabObjects.Count}      Scene References - {sceneReferences.Count}        PrefabsWithReferences - {prefabsWithReferences.Count}");

			//Set Evaluated Objects Dictionary with scene and prefab objects
			m_EvaluatedObjectsDictionary.Clear();
			sceneObjects.ForEach(sceneObject => addToEvaluatedObjectsDictionary($"Scene - {sceneObject.scene.name}", sceneObject));
			prefabsWithReferences.ForEach(prefab => addToEvaluatedObjectsDictionary($"Prefab",                       prefab));


			//Creates Dictionary entry or adds to existing entry 
			void addToEvaluatedObjectsDictionary(string i_Key, GameObject i_Value)
			{
				if (m_EvaluatedObjectsDictionary.ContainsKey(i_Key))
					m_EvaluatedObjectsDictionary[i_Key].Add(i_Value);
				else
					m_EvaluatedObjectsDictionary.Add(i_Key, new List<GameObject>() { i_Value });
			}
		}

	#endregion


	#region Scene Management

		private static Scene m_CurrentScene;

		/// <summary>
		/// Open All Scenes in the Editor
		/// </summary>
		private static void openAllScenes()
		{
			m_CurrentScene = SceneManager.GetActiveScene();
			//Closing current scene so we can open all scene sequentially
			if (m_CurrentScene.path != EditorBuildSettings.scenes[0].path)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
			}

			//Open all scenes
			for (int i = 1; i < EditorBuildSettings.scenes.Length; i++)
			{
				EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path, OpenSceneMode.Additive);
			}
		}

		/// <summary>
		/// Closes All Scenes Except the original opened scene
		/// </summary>
		private static void reopenPreviousScene()
		{
			for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
			{
				var evaluatedScene = SceneManager.GetSceneAt(i);
				if (evaluatedScene != m_CurrentScene)
					EditorSceneManager.CloseScene(evaluatedScene, true);
			}
		}

	#endregion

	#region Get GameObjects

		private static List<GameObject> getSceneObjects() => Object.FindObjectsOfType<GameObject>(true).ToList();

		/// <summary>
		/// Get All Project Prefabs and its children
		/// </summary>
		/// <returns></returns>
		private static List<GameObject> getProjectPrefabs()
		{
			var allPrefabs = new List<GameObject>();

			var prefabs = GetAssets<GameObject>();
			prefabs.ForEach(prefab => addProjectPrefabsChildrenRecursively(prefab, ref allPrefabs));

			// allPrefabs.ForEach(prefab => Debug.LogError($"This is a Prefab {prefab.name}", prefab));

			//Recursively get all children of a prefab
			static void addProjectPrefabsChildrenRecursively(GameObject i_ParentPrefab, ref List<GameObject> i_RefPrefabList)
			{
				i_RefPrefabList.Add(i_ParentPrefab);

				for (int i = 0; i < i_ParentPrefab.transform.childCount; i++)
				{
					addProjectPrefabsChildrenRecursively(i_ParentPrefab.transform.GetChild(i).gameObject, ref i_RefPrefabList);
				}
			}

			return allPrefabs;
		}

		/// <summary>
		/// Get Scene References to Prefabs and its children
		/// </summary>
		/// <param name="i_SceneObjects"></param>
		/// <returns></returns>
		private static List<Object> getSceneReferences(List<GameObject> i_SceneObjects)
		{
			var references       = new List<Object>();
			var evaluatedObjects = new List<Object>();

			foreach (var sceneObject in i_SceneObjects)
			{
				addSceneReferencesRecursively(sceneObject);
			}

			void addSceneReferencesRecursively(Object i_ObjectToEvaluate)
			{
				//If references is not a GameObject we skip
				if (evaluatedObjects.Contains(i_ObjectToEvaluate) || !(i_ObjectToEvaluate is GameObject gameObject)) return;

				//We shouldn't add references to objects in the scene, only prefabs
				if (string.IsNullOrEmpty(gameObject.scene.name))
					references.Add(gameObject);

				//Keeps track of checked objects so we don't check circular references
				evaluatedObjects.Add(gameObject);

				var components = gameObject.GetComponents<Component>();
				//Goes through all components to check for potential GameObject references
				foreach (var component in components)
				{
					if (component == null)
						continue;

					//Get Serializable Data from each Components
					var so   = new SerializedObject(component);
					var prop = so.GetIterator();

					prop.Next(true);
					//Iterates through all Serialized data to find GameObjects
					while (prop.Next(true))
					{
						//Found a Object Reference
						if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null)
						{
							//If Object is GameObject we should check all its children
							if (prop.objectReferenceValue is GameObject go)
								addPrefabChildRecursively(go.transform);
							//Else we just check that specific object
							else
								addSceneReferencesRecursively(prop.objectReferenceValue);
						}
					}
				}
			}

			//Check All GameObject child for references
			void addPrefabChildRecursively(Transform i_Transform)
			{
				addSceneReferencesRecursively(i_Transform.gameObject);

				for (int i = 0; i < i_Transform.childCount; i++)
				{
					addPrefabChildRecursively(i_Transform.GetChild(i));
				}
			}

			return references;
		}

	#endregion
	}
}