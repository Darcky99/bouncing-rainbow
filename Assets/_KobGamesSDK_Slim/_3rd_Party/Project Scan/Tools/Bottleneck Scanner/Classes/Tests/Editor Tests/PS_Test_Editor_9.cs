#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This test checks if there are any undefined tags
    /// </summary>
    public class PS_Test_Editor_9 : PS_SubTest
    {
        private List<PS_Object>
                AllGameObjects,
                targetObjects;

        private List<PS_ObjectCategory> GAMEOBJECT_CATEGORIES;

        public PS_Test_Editor_9(List<PS_Object> AllGameObjects)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 32;
            TITLE = "GameObjects with an Undefined Tag";
            DESCRIPTION = "May be a problem in the future if you would try accessing GameObject's Tag for any reason";
            SOLUTION = "Change GameObject's tag to either relevant one, or \"Untagged\"";

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
                    GameObject gPos = (GameObject)CATEGORY.objectsList[j].obj;

                    if (string.IsNullOrEmpty(gPos.tag))
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