#if UNITY_EDITOR

using System.Collections.Generic;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This test checks if Debug.Log() is used in Update(), OnGUI, FixedUpdate() and LateUpdate()
    /// </summary>
    public class PS_Test_Code_1 : PS_SubTest
    {
        //List<PS_Object> AllScripts;
        private List<PS_Object> AffectedScripts;

        public PS_Test_Code_1(List<PS_Object> AllScripts)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.CODE;
            ID = 3;
            TITLE = "Debug.Log() in Update()";
            DESCRIPTION = "You have a Debug.Log() method running in constantly running functions like Update(), FixedUpdate() and LateUpdate(). This has a strong effect on a runtime performance and usually doesn't do anything useful outside of Unity Editor (unless you're Debugging)";
            SOLUTION = "Unless you're at a debugging stage of the project, get rid of Debug.Log()";

            //this.AllScripts = AllScripts;
            AffectedScripts = new List<PS_Object>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            /*
            BindingFlags bindingFlags = BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly;
            */

            if (AffectedScripts.Count > 0)
            {
                REPORT.Populate(ID, AffectedScripts, TITLE, DESCRIPTION, SOLUTION, false);

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