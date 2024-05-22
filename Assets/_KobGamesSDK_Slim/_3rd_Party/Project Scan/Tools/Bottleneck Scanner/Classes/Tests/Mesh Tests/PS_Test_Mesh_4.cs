#if UNITY_EDITOR

using System.Collections.Generic;

/*  DESCRIPTION =================================================================
 *  This class does a test
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Mesh_4 : PS_SubTest
    {
        private List<PS_Object> AllModels;

        public PS_Test_Mesh_4(List<PS_Object> AllModels)
        {
            this.AllModels = AllModels;

            REPORT.ResultCategory = PS_Result.CATEGORY.MESH;
            ID = 34;
            TITLE = "3D Models found in a 2D Project";
            DESCRIPTION = string.Format("Based on your Project Type settings, it has been determined that the project is entirely 2D\nThere were {0} meshes found inside your Project. Because the project is 2D, you're likely to not be using 3D models, which do nothing but add up additional project space.", AllModels.Count);
            SOLUTION = "You could either get rid of all your 3D models, or ";
        }

        public override void Check()
        {
            if (PS_Utils.IsProject2D)
            {
                if (AllModels.Count > 0)
                {
                    REPORT.hasPassed = false;
                    REPORT.Populate(ID, AllModels, TITLE, DESCRIPTION, SOLUTION, false);
                }
                else
                {
                    REPORT.hasPassed = true;
                }
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif