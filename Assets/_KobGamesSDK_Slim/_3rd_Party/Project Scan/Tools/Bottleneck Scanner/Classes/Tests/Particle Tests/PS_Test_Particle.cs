#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// For running tests of Particle category
    /// </summary>
    public class PS_Test_Particle : PS_Test
    {
        private List<PS_Object>
            AllGameObjects,
            AllParticles;

        public PS_Test_Particle(List<PS_Object> AllGameObjects)
        {
            TESTS = new PS_SubTest[1];

            TestCategory = PS_Result.CATEGORY.PARTICLE;

            this.AllGameObjects = AllGameObjects;

            RetrieveData();

            TESTS[0] = new PS_Test_Particle_1(AllParticles);
        }

        public override void RetrieveData()
        {
            AllParticles = new List<PS_Object>();

            for (int i = 0; i < AllGameObjects.Count; i++)
            {
                GameObject gObj = (GameObject)AllGameObjects[i].obj;

                if (gObj.GetComponent<ParticleSystem>())
                {
                    PS_Utils.CallProgressBar("Gathering RigidBodies", gObj.name, i, AllGameObjects.Count);

                    AllParticles.Add(AllGameObjects[i]);
                }
            }
        }

        public override void Run()
        {
            if (AllParticles.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing Particle Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling Particle Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif