#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used to store Assets, GameObjects or Folders. Unlike stock Object, PS_Object can serialize GameObjects and get their local ID
    /// </summary>
    [System.Serializable]
    public class PS_Object
    {
        public UnityEngine.Object obj;
        public string objName;
        public int objLocalID;                      //local ID of a GameObject. Local ID always remains constant which is a great way to track GameObject
        public string objectAssetPath;              //Object path if it's qualified as an asset file
        public string fullObjectPath;               //Full file path if it's qualified as an asset file
        public TYPE objectType;

        public Scene sourceScene;                   //Object's Scene if it's of GameObject type

        public enum TYPE
        { GAMEOBJECT, ASSET, FOLDER }

        public GameObject[] SceneObjects;

        /// <summary>
        /// This constructor is for adding ASSETS and GAMEOBJECTS
        /// </summary>
        /// <param name="obj">Object that represents a given ASSET/GAMEOBJECT</param>
        /// <param name="objectType">Type of object</param>
        public PS_Object(UnityEngine.Object obj, TYPE objectType)
        {
            this.objectType = objectType;
            this.obj = obj;

            objName = obj.name;

            if (objectType == TYPE.ASSET)
            {
                objectAssetPath = UnityEditor.AssetDatabase.GetAssetPath(obj.GetInstanceID());

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    objectAssetPath = objectAssetPath.Replace("/", @"\");
                }
                else
                {
                    objectAssetPath = objectAssetPath.Replace(@"\", "/");
                }

                fullObjectPath = System.IO.Path.GetFullPath(objectAssetPath);
            }
            else if (objectType == TYPE.GAMEOBJECT)
            {
                objLocalID = PS_Utils.GetLocalID(obj);
            }
        }

        /// <summary>
        /// This constructor is for adding FOLDERS
        /// </summary>
        /// <param name="objPath">Path to the folder</param>
        public PS_Object(string objPath)
        {
            objectType = TYPE.FOLDER;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                objPath = objPath.Replace("/", @"\");
            }
            else
            {
                objPath = objPath.Replace(@"\", "/");
            }

            fullObjectPath = System.IO.Path.GetFullPath(objPath);

            objName = objPath;
            objectAssetPath = objPath;
        }

        /// <summary>
        /// Fetch an Object if it's null. PS_Object stores object data with two method. If it's a GameObject, then it stores local ID. If it's an Asset file, then it stores its directory path and re-assigned "obj" as necessary
        /// Also re-assigns its source Scene
        /// </summary>
        public void getData()
        {
            if (objectType != TYPE.FOLDER)
            {
                if (obj == null)
                {
                    try
                    {
                        if (objectType == TYPE.ASSET)
                        {
                            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(objectAssetPath, typeof(UnityEngine.Object));
                        }
                        else if (objectType == TYPE.GAMEOBJECT)
                        {
                            SceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                            for (int i = 0; i < SceneObjects.Length; i++)
                            {
                                GameObject game_obj = SceneObjects[i];

                                if (PS_Utils.GetLocalID(game_obj) == objLocalID)
                                {
                                    obj = game_obj;
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        obj = null;
                    }
                }
            }
        }
    }
}

#endif