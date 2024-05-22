#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

/*  DESCRIPTION =================================================================
 *  This class tests if some of the meshes have hit the benchmark's polygon limit
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Mesh_1 : PS_SubTest
    {
        private List<PS_Object> AllModels;
        private List<PS_Object> targetModels;
        private List<ModelImporter> AllModelImporters;

        public PS_Test_Mesh_1(List<PS_Object> AllModels, List<ModelImporter> AllModelImporters)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.MESH;
            ID = 12;
            TITLE = "Model's mesh is not optimized";
            DESCRIPTION = "Optimized Mesh Performance is recommended as it reorders vertices and indices to maximize post-vertex-shader cache hit rate, resulting in a better GPU performance";
            SOLUTION = "Ensure that \"Optimize Mesh\" is enabled in model's Import Settings";

            this.AllModels = AllModels;
            targetModels = new List<PS_Object>();
            this.AllModelImporters = AllModelImporters;
        }

        public override void Check()
        {
            for (int i = 0; i < AllModels.Count; i++)
            {
                if (!AllModelImporters[i].optimizeMesh)
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