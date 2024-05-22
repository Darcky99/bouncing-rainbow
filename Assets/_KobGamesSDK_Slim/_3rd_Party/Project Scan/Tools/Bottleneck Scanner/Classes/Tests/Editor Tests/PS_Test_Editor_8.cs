#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This subtest checks whether game icon is default or not
    /// </summary>
    public class PS_Test_Editor_8 : PS_SubTest
    {
        public PS_Test_Editor_8()
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 31;
            TITLE = "Default Icon is a stock Unity logo";
            DESCRIPTION = "The game is missing its own icon, as a result the application compiles with a default Unity logo. This creates an unprofessional representation of the final product for consumers";
            SOLUTION = "Change the Default Icon by going to \"Edit > Project Settings > Player\"";
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);

            bool nullIcon = false;

            for (int i = 0; i < icons.Length; i++)
            {
                nullIcon = (icons[i] == null);

                if (nullIcon)
                {
                    break;
                }
            }

            if (nullIcon)
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