using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KobGamesSDKSlim.ProjectValidator
{
    public static class ValidatorUtils
    {
        /// <summary>
        /// Get all classes inherited from Type. Class can't be abstract
        /// </summary>
        /// <param name="i_ParentType"></param>
        /// <returns></returns>
        //TODO - add to Extension Methods?
        public static List<Type> getInheritedClassesTypes(Type i_ParentType)
            => Assembly.GetAssembly(i_ParentType)
                       .GetTypes()
                       .Where(TheType => TheType.IsClass
                                      && !TheType.IsAbstract
                                      && TheType.IsSubclassOf(i_ParentType))
                       .ToList();

        /// <summary>
        /// Get Objects from the Project Hierarchy (not scene) based on type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAssets<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();

            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] {"Assets"});
            assets.AddRange(guids.Select(AssetDatabase.GUIDToAssetPath)
                                 .Select(AssetDatabase.LoadAssetAtPath<T>)
                                 .ToList());
            
            
            guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] {"Packages"});
            assets.AddRange(guids.Select(AssetDatabase.GUIDToAssetPath)
                                 .Where(path => path.Contains("kobgames")) //Only looking for our packages since we can't do anything about other packages
                                 .Select(AssetDatabase.LoadAssetAtPath<T>)
                                 .ToList());
            
            return assets;
        }
        
        /// <summary>
        /// Check if an Object is part of the Project Hierarchy or if it is in a Scene
        /// </summary>
        /// <param name="i_Object"></param>
        /// <param name="i_FolderPaths"></param>
        /// <returns></returns>
        public static bool isObjectInFolder(Object i_Object, params string[] i_FolderPaths)
        {
            /*if (i_FolderPaths.Length == 0)
            {
                Debug.LogError("Folders names not Provided. You should provide at least 1.");
                return false;
            }*/

            i_FolderPaths = i_FolderPaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray();
            if (i_FolderPaths.Length == 0)
                return false;
            

            var assetPath = AssetDatabase.GetAssetPath(i_Object);

            //If couldn't find path it means it is in the Scene
            if (assetPath == null)
                return false;

            var paths        = assetPath.Split('/');
            var foldersFound = paths.Where(i_FolderPaths.Contains);

            return foldersFound.Any();
        }
    }
}