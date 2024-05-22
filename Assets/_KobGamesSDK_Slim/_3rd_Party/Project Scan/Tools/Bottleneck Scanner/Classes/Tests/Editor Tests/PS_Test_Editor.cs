#if UNITY_EDITOR

using System.Collections.Generic;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs Editor subtests
    /// </summary>
    [System.Serializable]
    public class PS_Test_Editor : PS_Test
    {
        public PS_Test_Editor(List<PS_Object> AllGameObjects)
        {
            TESTS = new PS_SubTest[10];

            TestCategory = PS_Result.CATEGORY.EDITOR;

            TESTS[0] = new PS_Test_Editor_1();
            TESTS[1] = new PS_Test_Editor_2();
            TESTS[2] = new PS_Test_Editor_3();
            TESTS[3] = new PS_Test_Editor_4();
            TESTS[4] = new PS_Test_Editor_5(AllGameObjects);
            TESTS[5] = new PS_Test_Editor_6(AllGameObjects);
            TESTS[6] = new PS_Test_Editor_7();
            TESTS[7] = new PS_Test_Editor_8();
            TESTS[8] = new PS_Test_Editor_9(AllGameObjects);
            TESTS[9] = new PS_Test_Editor_10(AllGameObjects);
        }

        public override void RetrieveData()
        {
        }

        public override void Run()
        {
            for (int i = 0; i < TESTS.Length; i++)
            {
                PS_Utils.CallProgressBar("Executing Editor Tests", i, TESTS.Length);
                TESTS[i].Check();
            }

            for (int i = 0; i < TESTS.Length; i++)
            {
                PS_Utils.CallProgressBar("Compiling Editor Results", i, TESTS.Length);
                RESULTS.Add(TESTS[i].REPORT);
            }
        }
    }
}

#endif