#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This test checks if developer uses Instantiate() or Destroy()
    /// </summary>
    public class PS_Test_Code_3 : PS_SubTest
    {
        private List<PS_Object> AllScripts;
        private List<PS_Object> AffectedScripts;

        public PS_Test_Code_3(List<PS_Object> AllScripts)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.CODE;
            ID = 5;
            TITLE = "Instantiate() and/or Destroy() have been found";
            DESCRIPTION = "Instantiate() and Destroy() are heavy operations: the system must allocate a lot of memory blocks to instantiate a single object, and free these blocks when destroying the object.";
            SOLUTION = "Create your own system for Object Pooling, which is basically re-use your old GameObjects instead of destroying them only to create new ones.";
            URL = "http://ps.hardcodelab.com/2017/05/how-instantiate-and-destroy-affect-performance.html";

            this.AllScripts = AllScripts;
            AffectedScripts = new List<PS_Object>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            for (int i = 0; i < AllScripts.Count; i++)
            {
                string scriptPath = AllScripts[i].objectAssetPath;
                bool isAffected = false;
                string[] scriptContent = File.ReadAllLines(scriptPath);

                PS_Utils.CallProgressBar("Assessing Scripts", scriptPath, i, AllScripts.Count);

                for (int j = 0; j < scriptContent.Length; j++)
                {
                    if ((scriptContent[j].Contains("Instantiate") || scriptContent[j].Contains("Destroy")) && !scriptContent[j].Contains("//"))
                    {
                        isAffected = true;
                    }
                }

                if (isAffected)
                {
                    AffectedScripts.Add(AllScripts[i]);
                }
            }

            if (AffectedScripts.Count > 0)
            {
                REPORT.Populate(ID, AffectedScripts, TITLE, DESCRIPTION, SOLUTION, false, URL);

                REPORT.hasPassed = false;
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif