#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs UI subtests
    /// </summary>
    [System.Serializable]
    public class PS_Test_UI : PS_Test
    {
        private List<PS_Object> AllGameObjects, AllCanvases;

        public PS_Test_UI(List<PS_Object> AllGameObjects)
        {
            TESTS = new PS_SubTest[2];

            TestCategory = PS_Result.CATEGORY.UI;

            this.AllGameObjects = AllGameObjects;
            AllCanvases = new List<PS_Object>();
            RetrieveData();

            TESTS[0] = new PS_Test_UI_1(AllCanvases);
            TESTS[1] = new PS_Test_UI_2(AllCanvases);
        }

        public override void RetrieveData()
        {
            for (int i = 0; i < AllGameObjects.Count; i++)
            {
                GameObject psGO = (GameObject)AllGameObjects[i].obj;

                if (psGO.GetComponent<Canvas>())
                {
                    PS_Utils.CallProgressBar("Gathering UI Objects", psGO.name, i, AllGameObjects.Count);
                    AllCanvases.Add(AllGameObjects[i]);
                }
            }
        }

        public override void Run()
        {
            if (AllCanvases.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing UI Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling UI Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif