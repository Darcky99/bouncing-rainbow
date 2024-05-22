#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This subtest checks for missing components in GameObjects
    /// </summary>
    public class PS_Test_Editor_10 : PS_SubTest
    {
        private List<PS_Object>
                AllGameObjects,
                targetObjects;

        private List<PS_ObjectCategory> GAMEOBJECT_CATEGORIES;

        public PS_Test_Editor_10(List<PS_Object> AllGameObjects)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 33;
            TITLE = "Missing Component in GameObject";
            DESCRIPTION = "There's a missing/deleted Component in GameObject. This may have been caused because a MonoBehaviour script that referenced that Component has been removed.";
            SOLUTION = "Remove the null Component, or try recovering the deleted script";

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

                    Component[] gComps = gPos.GetComponents<Component>();

                    for (int k = 0; k < gComps.Length; k++)
                    {
                        if (gComps.Equals(null))
                        {
                            targetObjects.Add(CATEGORY.objectsList[j]);
                            continue;
                        }
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