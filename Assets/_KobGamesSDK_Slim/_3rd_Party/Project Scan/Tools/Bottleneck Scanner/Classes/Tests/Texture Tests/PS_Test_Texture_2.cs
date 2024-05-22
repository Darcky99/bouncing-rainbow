#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This subtest checks whether size of textures isn't too large
    /// </summary>
    public class PS_Test_Texture_2 : PS_SubTest
    {
        private List<PS_Object> allTextures;
        private List<PS_Object> targetTextures;
        private List<TextureImporter> textureImporters;

        public PS_Test_Texture_2(List<TextureImporter> textureImporters, List<PS_Object> textureAssets)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.TEXTURE;
            ID = 18;
            TITLE = "Some Textures' resolution is too large";
            DESCRIPTION = "Large Max Size of textures usually lead to larger build and project size and may also result in longer computation time at runtime";
            SOLUTION = "Consider reducing the size of the maximum texture resolution";

            targetTextures = new List<PS_Object>();
            this.textureImporters = textureImporters;
            allTextures = textureAssets;
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_ENABLED && PS_Utils.GetData_BottleneckSettings().BENCHMARK_TEXTURE_maxTextureResolution > 0)
            {
                for (int i = 0; i < textureImporters.Count; i++)
                {
                    PS_Utils.CallProgressBar("Assessing Textures", allTextures[i].objName, textureImporters.Count, i);

                    if (textureImporters[i].maxTextureSize > PS_Utils.GetData_BottleneckSettings().BENCHMARK_TEXTURE_maxTextureResolution)
                    {
                        UnityEngine.Debug.Log(textureImporters[i].maxTextureSize);
                        targetTextures.Add(allTextures[i]);
                    }
                }

                if (targetTextures.Count > 0)
                {
                    REPORT.Populate(ID, targetTextures, TITLE, DESCRIPTION, SOLUTION, false);
                    REPORT.hasPassed = false;
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