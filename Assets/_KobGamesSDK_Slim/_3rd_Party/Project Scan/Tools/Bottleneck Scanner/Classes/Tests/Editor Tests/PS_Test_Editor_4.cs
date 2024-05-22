#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This test checks if there are any empty folders inside Unity Project
    /// </summary>
    public class PS_Test_Editor_4 : PS_SubTest
    {
        private List<PS_Object> targetFolders;

        public PS_Test_Editor_4()
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 27;
            TITLE = "Empty folder in Unity Project";
            DESCRIPTION = "There are some directories in your project folder that contain neither no files nor other directories. Essentially, it stores nothing. This creates unnecessary clutter, thus making it harder to navigate around the project becomes more complex";
            SOLUTION = "Delete the folder, unless it's needed in the future";

            targetFolders = new List<PS_Object>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            string[] fileDirectories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories).Where(x => !x.Contains("/Project Scan/") && !x.Contains(@"\Project Scan\")).ToArray();

            for (int i = 0; i < fileDirectories.Length; i++)
            {
                string tDir = fileDirectories[i];

                if (Directory.GetFiles(tDir).Length == 0 && Directory.GetDirectories(tDir).Length == 0)
                {
                    targetFolders.Add(new PS_Object(tDir));
                }
            }

            if (targetFolders.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, targetFolders, TITLE, DESCRIPTION, SOLUTION, false);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif