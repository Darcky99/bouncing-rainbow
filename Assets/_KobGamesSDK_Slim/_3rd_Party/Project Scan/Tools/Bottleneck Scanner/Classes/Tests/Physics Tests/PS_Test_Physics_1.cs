#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Checks if any GameObject has a Mesh Collider attached to it
    /// </summary>
    [System.Serializable]
    public class PS_Test_Physics_1 : PS_SubTest
    {
        private List<PS_Object> MeshColliderObjects;
        private List<PS_Object> AllRigidbodies;

        public PS_Test_Physics_1(List<PS_Object> AllGameObjects)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 14;
            TITLE = "Some GameObjects use Mesh Collider for collision checking";
            DESCRIPTION = "There are X GameObjects that use a Mesh Collider. While collisions are more precise, there's longer computation time.";
            SOLUTION = "Unless you need super precise collisions, replace Mesh Collider with primitives such as Box or Sphere Collider.";

            AllRigidbodies = AllGameObjects;
            MeshColliderObjects = new List<PS_Object>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            //check whether any of GameObjects has a MeshCollider attached to them
            for (int i = 0; i < AllRigidbodies.Count; i++)
            {
                PS_Object rigidObj = AllRigidbodies[i];

                if (((GameObject)rigidObj.obj).GetComponent<MeshCollider>() != null)
                {
                    MeshColliderObjects.Add(rigidObj);
                }
            }

            //evaluate the data...
            if (MeshColliderObjects.Count > 1)          //FAILS THE TEST
            {
                DESCRIPTION = "There are " + MeshColliderObjects.Count + " GameObjects that use a Mesh Collider. While collisions are more precise, there's longer computation time, which affects performance.";
                REPORT.hasPassed = false;
                REPORT.Populate(ID, MeshColliderObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else if (MeshColliderObjects.Count == 1)    //FAILS THE TEST
            {
                DESCRIPTION = "There is a GameObject that uses a Mesh Collider. While collisions are more precise, there's longer computation time, which affects performance.";
                REPORT.hasPassed = false;
                REPORT.Populate(ID, MeshColliderObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else if (MeshColliderObjects.Count == 0)    //PASSES THE TEST
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif