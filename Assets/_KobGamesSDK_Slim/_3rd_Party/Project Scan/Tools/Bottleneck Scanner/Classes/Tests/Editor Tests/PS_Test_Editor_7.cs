#if UNITY_EDITOR

using UnityEditor;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This class checks if Company Name is specified in Project Settings
    /// </summary>
    public class PS_Test_Editor_7 : PS_SubTest
    {
        public PS_Test_Editor_7()
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 30;
            TITLE = "Company name is not specified in Player Settings";
            DESCRIPTION = "Specifying Company name is good to make your work stand out from the others' especially when you're sharing the project source with others.";
            SOLUTION = "Change the Company Name by going to \"Edit > Project Settings > Player\"";
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            if (PlayerSettings.companyName == "DefaultCompany")
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, TITLE, DESCRIPTION, SOLUTION, false);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif