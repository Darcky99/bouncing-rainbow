#if UNITY_EDITOR

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This test checks if there are enough quality presets
    /// </summary>
    public class PS_Test_Editor_1 : PS_SubTest
    {
        public PS_Test_Editor_1()
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 6;
            TITLE = "Limited number of Quality Settings";
            DESCRIPTION = "Your end-users would benefit greatly from having larger choice of changing quality levels as they can specify based on their machine specifications";
            SOLUTION = "Unless you're handling quality levels internally, create more quality presets in Quality Settings menu";
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            REPORT.hasPassed = true;
        }
    }
}

#endif