#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs Mesh subtests
    /// </summary>
    public class PS_Test_Mesh : PS_Test
    {
        private List<PS_Object> AllModels;
        private List<ModelImporter> AllModelImporters;

        public PS_Test_Mesh()
        {
            TESTS = new PS_SubTest[4];

            TestCategory = PS_Result.CATEGORY.MESH;

            RetrieveData();

            TESTS[0] = new PS_Test_Mesh_1(AllModels, AllModelImporters);
            TESTS[1] = new PS_Test_Mesh_2(AllModels, AllModelImporters);
            TESTS[2] = new PS_Test_Mesh_3(AllModels);
            TESTS[3] = new PS_Test_Mesh_4(AllModels);
        }

        public override void RetrieveData()
        {
            string[] meshFormats = new string[] { "fbx", "dae", "3ds", "dxf", "obj", "skp" };

            AllModels = new List<PS_Object>();
            AllModelImporters = new List<ModelImporter>();

            int count = PS_Utils.ALL_FILE_PATHS.Length;

            for (int i = 0; i < count; i++)
            {
                string fileExtension = Path.GetExtension(PS_Utils.ALL_FILE_PATHS[i]).Replace(".", "");

                if (meshFormats.Contains(fileExtension))
                {
                    string modelPath = "Assets" + PS_Utils.ALL_FILE_PATHS[i].Substring(Application.dataPath.Length);

                    ModelImporter newImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;

                    if (newImporter != null)
                    {
                        Mesh newMesh = (Mesh)AssetDatabase.LoadAssetAtPath(modelPath, typeof(Mesh));

                        if (newMesh != null)
                        {
                            PS_Utils.CallProgressBar("Gathering Mesh", modelPath, i, count);

                            AllModelImporters.Add(newImporter);

                            AllModels.Add(new PS_Object(
                                newMesh, PS_Object.TYPE.ASSET
                                ));
                        }
                    }
                }
            }
        }

        public override void Run()
        {
            if (AllModels.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing Mesh Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling Mesh Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif