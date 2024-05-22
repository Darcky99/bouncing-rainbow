#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This checks there are any GameObjects with disconnected prefabs
    /// </summary>
    public class PS_Test_Editor_5 : PS_SubTest
    {
        private List<PS_Object>
            AllGameObjects,
            targetObjects;

        private List<PS_ObjectCategory> GAMEOBJECT_CATEGORIES;

        public PS_Test_Editor_5(List<PS_Object> AllGameObjects)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 28;
            TITLE = "Disconnected prefabs found";
            DESCRIPTION = "Some Prefabs are either disconnected from their original prefab or broken";
            SOLUTION = "Unless it's intentional, you could drag a disconnected/broken prefab into Project Folder to fix the prefab. Otherwise, you could \"Break Prefab Instance\" from the GameObject's menu to disconnect it from its original prefab";

            this.AllGameObjects = AllGameObjects;
            targetObjects = new List<PS_Object>();
            GAMEOBJECT_CATEGORIES = new List<PS_ObjectCategory>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            for (int i = 0; i < AllGameObjects.Count; i++)
            {
                GameObject psGO = (GameObject)AllGameObjects[i].obj;

                int resultIndex = GAMEOBJECT_CATEGORIES.FindIndex(x => x.categoryScene == psGO.scene);

                if (resultIndex >= 0)
                {
                    GAMEOBJECT_CATEGORIES[resultIndex].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                }
                else
                {
                    GAMEOBJECT_CATEGORIES.Add(new PS_ObjectCategory(psGO.scene));
                    GAMEOBJECT_CATEGORIES[GAMEOBJECT_CATEGORIES.Count - 1].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                }
            }

            for (int i = 0; i < GAMEOBJECT_CATEGORIES.Count; i++)
            {
                PS_ObjectCategory CATEGORY = GAMEOBJECT_CATEGORIES[i];

                for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                {
                    GameObject gOBJ = (GameObject)CATEGORY.objectsList[j].obj;

                    PrefabType prefType = PrefabUtility.GetPrefabType(gOBJ);

                    if (prefType == PrefabType.DisconnectedPrefabInstance || prefType == PrefabType.DisconnectedModelPrefabInstance || prefType == PrefabType.MissingPrefabInstance)
                    {
                        targetObjects.Add(CATEGORY.objectsList[j]);
                        continue;
                    }
                }
            }

            if (targetObjects.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, targetObjects, TITLE, DESCRIPTION, SOLUTION, true);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif