#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  Checks if there are too many Rigidbodies in the scene
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    [System.Serializable]
    public class PS_Test_Physics_3 : PS_SubTest
    {
        private List<PS_ObjectCategory> RIGIDBODY_CATEGORIES;
        private List<PS_Object> TargetObjects;
        private List<PS_Object> AllRigidbodies;

        public PS_Test_Physics_3(List<PS_Object> AllRigidbodies)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 16;
            TITLE = "There are lots of Rigidbodies in the scene";
            DESCRIPTION = "There are X Rigidbodies in the scene. This can compromise the performance on some weaker machines, causing a visible loss of FPS.";
            SOLUTION = "Try minimizing the number of active Rigidbodies to reduce performance bottlenecks.";

            this.AllRigidbodies = AllRigidbodies;

            TargetObjects = new List<PS_Object>();
            RIGIDBODY_CATEGORIES = new List<PS_ObjectCategory>();
        }

        public override void Check()
        {
            if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_ENABLED && PS_Utils.GetData_BottleneckSettings().BENCHMARK_PHYSICS_maxRigidbodiesPerScene > 0)
            {
                for (int i = 0; i < AllRigidbodies.Count; i++)
                {
                    GameObject psGO = (GameObject)AllRigidbodies[i].obj;

                    int resultIndex = RIGIDBODY_CATEGORIES.FindIndex(x => x.categoryScene == psGO.scene);

                    if (resultIndex >= 0)
                    {
                        RIGIDBODY_CATEGORIES[resultIndex].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                    else
                    {
                        RIGIDBODY_CATEGORIES.Add(new PS_ObjectCategory(psGO.scene));
                        RIGIDBODY_CATEGORIES[RIGIDBODY_CATEGORIES.Count - 1].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                }

                for (int i = 0; i < RIGIDBODY_CATEGORIES.Count; i++)
                {
                    PS_ObjectCategory CATEGORY = RIGIDBODY_CATEGORIES[i];

                    if (CATEGORY.objectsList.Count > PS_Utils.GetData_BottleneckSettings().BENCHMARK_PHYSICS_maxRigidbodiesPerScene)
                    {
                        TargetObjects.AddRange(CATEGORY.objectsList);
                    }
                }

                if (TargetObjects.Count > 0)
                {
                    REPORT.hasPassed = false;
                    DESCRIPTION = "There are " + TargetObjects.Count + " Rigidbodies in the scene. This can compromise the performance on some weaker machines, causing a visible loss of FPS.";
                    REPORT.Populate(ID, TargetObjects, TITLE, DESCRIPTION, SOLUTION);
                }
                else
                {
                    REPORT.hasPassed = true;
                }
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif