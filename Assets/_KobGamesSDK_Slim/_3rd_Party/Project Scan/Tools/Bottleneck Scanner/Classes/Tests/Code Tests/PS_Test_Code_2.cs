#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;

/*  DESCRIPTION =================================================================
 *  This test checks if foreach() loop is present in any of the scripts
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Code_2 : PS_SubTest
    {
        private List<PS_Object> AllScripts;
        private List<PS_Object> AffectedScripts;

        public PS_Test_Code_2(List<PS_Object> AllScripts)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.CODE;
            ID = 4;
            TITLE = "Usage of Foreach loop";
            DESCRIPTION = "A Foreach loop is heavy on performance and that is due to memory allocation that needs to be taken care of by the garbage collector";
            SOLUTION = "Use a \"for()\" loop instead";
            URL = "http://ps.hardcodelab.com/2017/05/why-is-foreach-bad.html";

            this.AllScripts = AllScripts;
            AffectedScripts = new List<PS_Object>();
        }

        public override void Check()
        {
            for (int i = 0; i < AllScripts.Count; i++)
            {
                string scriptPath = AllScripts[i].objectAssetPath;

                PS_Utils.CallProgressBar("Assessing Scripts", scriptPath, i, AllScripts.Count);

                string scriptContent = File.ReadAllText(scriptPath);

                if (scriptContent.Contains("foreach"))
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