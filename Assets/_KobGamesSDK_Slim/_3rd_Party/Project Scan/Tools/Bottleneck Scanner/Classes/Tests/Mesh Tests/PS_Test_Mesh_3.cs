#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  This class does a test
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Mesh_3 : PS_SubTest
    {
        public List<PS_Object> AllModels;
        public List<PS_Object> targetModels;

        public PS_Test_Mesh_3(List<PS_Object> AllModels)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.MESH;
            ID = 26;
            TITLE = "Some meshes have abnormally high number of polygons";
            DESCRIPTION = "Some of models' number polygons is above the average number of all meshes in the project folder. Apart from performance hits, this may also lead to different levels of detail due to varying polygon count. This may result in inconsistent visuals in the game.";
            SOLUTION = "You can either reduce the number of polygons in meshes or break up the large model into smaller separate pieces";

            this.AllModels = AllModels;
            targetModels = new List<PS_Object>();
        }

        public override void Check()
        {
            int totalPolygonsN = 0;

            for (int i = 0; i < AllModels.Count; i++)
            {
                Mesh model = (Mesh)AllModels[i].obj;
                totalPolygonsN += model.triangles.Length / 3;
            }

            int averagePolygonsN = totalPolygonsN / AllModels.Count;

            for (int i = 0; i < AllModels.Count; i++)
            {
                Mesh model = (Mesh)AllModels[i].obj;
                int modelPolygonN = model.triangles.Length / 3;

                if (modelPolygonN > averagePolygonsN)
                {
                    targetModels.Add(AllModels[i]);
                }
            }

            if (targetModels.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, targetModels, TITLE, DESCRIPTION, SOLUTION, false);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif