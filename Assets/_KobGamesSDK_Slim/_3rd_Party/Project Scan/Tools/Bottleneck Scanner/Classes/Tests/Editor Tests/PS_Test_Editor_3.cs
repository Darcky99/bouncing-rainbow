#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This subtest checks whether Occlusion culling is up-to-date or hasn't been baked at all
    /// </summary>
    public class PS_Test_Editor_3 : PS_SubTest
    {
        public PS_Test_Editor_3()
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.EDITOR;
            ID = 8;
            TITLE = "Occlusion Culling baking data is outdated";
            DESCRIPTION = "Outdated occlusion data may cause visual artifacts, most notably flickering and disappearing meshes";
            SOLUTION = "Re-bake Occlusion Culling by going to \"Window > Occlusion Culling\"";
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            if (!PS_Utils.IsProject2D)
            {
                if (!StaticOcclusionCulling.isRunning)
                {
                    string occlussionPath = Directory.GetParent(Application.dataPath) + "/Library/Occlusion";

                    if (Directory.Exists(occlussionPath))
                    {
                        var directory = new DirectoryInfo(occlussionPath);
                        FileInfo latestFile = (from f in directory.GetFiles()
                                               orderby f.LastWriteTime descending
                                               select f).First();

                        double acceptableAge = 1.0f;

                        if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_ENABLED)
                        {
                            acceptableAge = PS_Utils.GetData_BottleneckSettings().BENCHMARK_EDITOR_occlussionBakeDataAge;
                        }

                        if (acceptableAge > 0)
                        {
                            if ((DateTime.Now - latestFile.LastWriteTime).Hours >= acceptableAge)
                            {
                                REPORT.hasPassed = false;
                                DESCRIPTION += "\nLast time occlusion culling data was baked on " + latestFile.LastWriteTime.ToString();
                                REPORT.Populate(ID, TITLE, DESCRIPTION, SOLUTION, false);
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
                        REPORT.hasPassed = false;
                        TITLE = "Occlusion Culling hasn't been baked at all!";
                        DESCRIPTION = "Occlusion Culling is a great feature in Unity which can potentially improve runtime performance of your game!";
                        SOLUTION = "Bake Occlusion Culling by going to \"Windows > Occlusion Culling\"";
                        URL = "https://docs.unity3d.com/Manual/OcclusionCulling.html";
                        REPORT.Populate(ID, TITLE, DESCRIPTION, SOLUTION, false);
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