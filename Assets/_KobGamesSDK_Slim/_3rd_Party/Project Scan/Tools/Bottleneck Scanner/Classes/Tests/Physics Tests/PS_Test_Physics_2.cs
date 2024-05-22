#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  Checks if static GameObjects have Rigidbodies attached to them
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    [System.Serializable]
    public class PS_Test_Physics_2 : PS_SubTest
    {
        private List<PS_Object> AllRigidbodies;
        private List<PS_Object> TargetObjects;

        public PS_Test_Physics_2(List<PS_Object> AllRigidbodies)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 15;
            TITLE = "Static GameObjects have Rigidbodies attached to them";
            DESCRIPTION = "There are X GameObjects which are marked as static but have Rigidbodies attached to them. Rigidbodies are useless for static objects, unless you're doing some sort of collision checking.";
            SOLUTION = "Remove a Rigid-body Component from GameObjects that are marked static.";

            this.AllRigidbodies = AllRigidbodies;
            TargetObjects = new List<PS_Object>();
        }

        public override void Check()
        {
            for (int i = 0; i < AllRigidbodies.Count; i++)
            {
                PS_Object rigidObj = AllRigidbodies[i];

                if (((GameObject)rigidObj.obj).isStatic)
                {
                    if (((GameObject)rigidObj.obj).GetComponent<Rigidbody>())
                    {
                        TargetObjects.Add(rigidObj);
                    }
                }
            }

            if (TargetObjects.Count > 1)
            {
                DESCRIPTION = "There are " + TargetObjects.Count + " GameObjects which are marked as static but have Rigidbodies attached to them. Rigidbodies are useless for static objects, unless you're doing some sort of collision checking.";
                REPORT.hasPassed = false;
                REPORT.Populate(ID, TargetObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else if (TargetObjects.Count == 1)
            {
                DESCRIPTION = "There is a GameObject which is marked as static but has a Rigid-body attached to it. Rigidbodies are useless for static objects, unless you're doing some sort of collision checking.";
                REPORT.hasPassed = false;
                REPORT.Populate(ID, TargetObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else if (TargetObjects.Count == 0)
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif