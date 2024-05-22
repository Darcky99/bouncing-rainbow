#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs Lighting tests
    /// </summary>
    [System.Serializable]
    public class PS_Test_Lighting : PS_Test
    {
        private List<PS_Object>
            AllGameObjects,
            AllLights;

        public PS_Test_Lighting(bool IS_MOBILE, List<PS_Object> AllGameObjects)
        {
            TESTS = new PS_SubTest[3];

            TestCategory = PS_Result.CATEGORY.LIGHTING;

            this.AllGameObjects = AllGameObjects;

            RetrieveData();

            TESTS[0] = new PS_Test_Lighting_1(IS_MOBILE);
            TESTS[1] = new PS_Test_Lighting_2(IS_MOBILE, AllLights);
            TESTS[2] = new PS_Test_Lighting_3(AllLights);
        }

        public override void RetrieveData()
        {
            AllLights = new List<PS_Object>();

            for (int i = 0; i < AllGameObjects.Count; i++)
            {
                GameObject gObj = (GameObject)AllGameObjects[i].obj;

                if (gObj.GetComponent<Light>())
                {
                    PS_Utils.CallProgressBar("Gathering Lights", gObj.name, i, AllGameObjects.Count);

                    AllLights.Add(AllGameObjects[i]);
                }
            }
        }

        public override void Run()
        {
            if (AllLights.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing Lighting Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling Lighting Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif