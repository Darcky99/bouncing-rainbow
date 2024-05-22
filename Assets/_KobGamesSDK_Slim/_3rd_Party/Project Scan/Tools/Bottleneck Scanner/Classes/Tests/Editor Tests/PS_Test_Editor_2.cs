#if UNITY_EDITOR

using UnityEditor;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Checks whether Asset Serialization is set to "Force Text"
    /// </summary>
    public class PS_Test_Editor_2 : PS_SubTest
    {
        public PS_Test_Editor_2()
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 7;
            TITLE = "Asset Serialization is not set to \"Force Text\"";
            DESCRIPTION = "With \"Force Text\" enabled, GIT would work with commits much faster as it now deals with text. One drawback of using such mode, is that files get bigger in the working directory.";
            SOLUTION = "If you're working with version control systems, change the Asset Serialization to \"Force Text\"";
            URL = "http://answers.unity3d.com/questions/222281/asset-serialization-mixed-vs-force-text.html";
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            if (EditorSettings.serializationMode == SerializationMode.ForceText)
            {
                REPORT.hasPassed = true;
            }
            else
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, TITLE, DESCRIPTION, SOLUTION, false, URL);
            }
        }
    }
}

#endif