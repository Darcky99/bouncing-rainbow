#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs Code subtests
    /// </summary>
    public class PS_Test_Code : PS_Test
    {
        private List<PS_Object> AllScripts;

        public PS_Test_Code()
        {
            TESTS = new PS_SubTest[3];

            TestCategory = PS_Result.CATEGORY.CODE;

            RetrieveData();

            TESTS[0] = new PS_Test_Code_1(AllScripts);
            TESTS[1] = new PS_Test_Code_2(AllScripts);
            TESTS[2] = new PS_Test_Code_3(AllScripts);
        }

        public override void RetrieveData()
        {
            AllScripts = new List<PS_Object>();

            string[] scriptFormats = new string[] { "cs", "js" };

            int count = PS_Utils.ALL_FILE_PATHS.Length;

            for (int i = 0; i < count; i++)
            {
                string fileExtension = Path.GetExtension(PS_Utils.ALL_FILE_PATHS[i]).Replace(".", "");

                if (scriptFormats.Contains(fileExtension))
                {
                    string scriptPath = "Assets" + PS_Utils.ALL_FILE_PATHS[i].Substring(Application.dataPath.Length);

                    PS_Utils.CallProgressBar("Gathering Scripts", scriptPath, i, count);

                    AllScripts.Add(new PS_Object(
                        AssetDatabase.LoadAssetAtPath(scriptPath, typeof(System.Object)), PS_Object.TYPE.ASSET
                        ));
                }
            }
        }

        public override void Run()
        {
            for (int i = 0; i < TESTS.Length; i++)
            {
                PS_Utils.CallProgressBar("Executing Code Tests", i, TESTS.Length);
                TESTS[i].Check();
            }

            for (int i = 0; i < TESTS.Length; i++)
            {
                PS_Utils.CallProgressBar("Compiling Code Results", i, TESTS.Length);
                RESULTS.Add(TESTS[i].REPORT);
            }
        }
    }
}

#endif