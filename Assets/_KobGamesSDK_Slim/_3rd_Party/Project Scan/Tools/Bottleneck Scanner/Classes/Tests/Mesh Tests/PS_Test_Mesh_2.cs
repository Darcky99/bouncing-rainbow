#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  This class does a test
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Mesh_2 : PS_SubTest
    {
        public List<PS_Object> AllModels;
        public List<PS_Object> targetModels;
        public List<ModelImporter> AllModelImporters;

        public PS_Test_Mesh_2(List<PS_Object> AllModels, List<ModelImporter> AllModelImporters)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.MESH;
            ID = 25;
            TITLE = "Too many polygons";
            DESCRIPTION = "There's a high polygon count on some of your meshes in your Assets Folder. Unless you're targeting high-end machines, it's advised to minimize the polygon count for the sake of performance.\n";
            SOLUTION = "Try reducing the polygon count using an external modeling program";

            this.AllModels = AllModels;
            targetModels = new List<PS_Object>();
            this.AllModelImporters = AllModelImporters;
        }

        public override void Check()
        {
            if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_ENABLED)
            {
                if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_MESH_perMeshPolygonLimit > 0)
                {
                    for (int i = 0; i < AllModels.Count; i++)
                    {
                        Mesh model = (Mesh)AllModels[i].obj;

                        int modelPolygonN = model.triangles.Length / 3;

                        if (modelPolygonN > PS_Utils.GetData_BottleneckSettings().BENCHMARK_MESH_perMeshPolygonLimit)
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