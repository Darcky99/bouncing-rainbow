#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This test checks if there are any GameObjects that are too far away from other GameObjects
    /// </summary>
    public class PS_Test_Editor_6 : PS_SubTest
    {
        private List<PS_Object>
            AllGameObjects,
            targetObjects;

        private List<PS_ObjectCategory> GAMEOBJECT_CATEGORIES;

        public PS_Test_Editor_6(List<PS_Object> AllGameObjects)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 29;
            TITLE = "\"Lost\" GameObject in the Scene";
            DESCRIPTION = "A GameObject has been found to bee too far away from other GameObjects. It may have been placed by accident. Being far away may not serve its proper function or may be difficult to find.";
            SOLUTION = "Unless intended, delete the GameObject or move it closer to other GameObjects.";

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

                Vector3 averagePos = new Vector3();

                for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                {
                    Vector3 gPos = ((GameObject)CATEGORY.objectsList[j].obj).transform.position;

                    averagePos += gPos;
                }

                averagePos /= CATEGORY.objectsList.Count;

                for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                {
                    Vector3 gPos = ((GameObject)CATEGORY.objectsList[j].obj).transform.position;

                    if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_ENABLED && PS_Utils.GetData_BottleneckSettings().BENCHMARK_EDITOR_maxFurthestDistance > 0)
                    {
                        if (Vector3.Distance(gPos, averagePos) >= PS_Utils.GetData_BottleneckSettings().BENCHMARK_EDITOR_maxFurthestDistance)
                        {
                            targetObjects.Add(CATEGORY.objectsList[j]);
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