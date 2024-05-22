#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs Physics tests
    /// </summary>
    [System.Serializable]
    public class PS_Test_Physics : PS_Test
    {
        private List<PS_Object>
            AllRigidbodies,
            AllGameObjects;

        public PS_Test_Physics(List<PS_Object> AllGameObjects)
        {
            TESTS = new PS_SubTest[3];

            TestCategory = PS_Result.CATEGORY.PHYSICS;

            this.AllGameObjects = AllGameObjects;

            RetrieveData();

            TESTS[0] = new PS_Test_Physics_1(AllRigidbodies);
            TESTS[1] = new PS_Test_Physics_2(AllRigidbodies);
            TESTS[2] = new PS_Test_Physics_3(AllRigidbodies);
        }

        public override void RetrieveData()
        {
            AllRigidbodies = new List<PS_Object>();

            for (int i = 0; i < AllGameObjects.Count; i++)
            {
                PS_Object OBJ = AllGameObjects[i];

                if (OBJ.objectType == PS_Object.TYPE.GAMEOBJECT)
                {
                    GameObject gObj = (GameObject)AllGameObjects[i].obj;

                    if (gObj.GetComponent<Rigidbody>() || gObj.GetComponent<MeshCollider>() || gObj.GetComponent<BoxCollider>() || gObj.GetComponent<SphereCollider>()
                        || gObj.GetComponent<Rigidbody2D>() || gObj.GetComponent<Collider>() || gObj.GetComponent<BoxCollider2D>() || gObj.GetComponent<CircleCollider2D>())
                    {
                        PS_Utils.CallProgressBar("Gathering RigidBodies", gObj.name, i, AllGameObjects.Count);

                        AllRigidbodies.Add(OBJ);
                    }
                }
            }
        }

        public override void Run()
        {
            if (AllRigidbodies.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing RigidBody Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling RigidBody Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif