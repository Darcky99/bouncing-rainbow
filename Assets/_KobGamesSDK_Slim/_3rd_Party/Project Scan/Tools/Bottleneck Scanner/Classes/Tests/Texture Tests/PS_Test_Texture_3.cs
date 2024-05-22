#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This class checks if textures which rely on Trilinear filtering have "Generate Mip-Maps" option enabled
    /// </summary>
    public class PS_Test_Texture_3 : PS_SubTest
    {
        private List<PS_Object> allTextures, targetTextures;
        private List<TextureImporter> textureImporters;

        public PS_Test_Texture_3(List<TextureImporter> textureImporters, List<PS_Object> textureAssets)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.TEXTURE;
            ID = 19;
            TITLE = "Some Textures of Trilinear filtering have Mip-Maps disabled";
            DESCRIPTION = "There are some Textures that have Trilinear filtering enabled, but have \"Generate Mip-Maps\" disabled. Trilinear filtering heavily relies on usage of MipMaps";
            SOLUTION = "You can either enable Mip-Maps for Textures with Trilinear filtering or change filtering mode entirely (Bilinear is preferred)";

            targetTextures = new List<PS_Object>();
            this.textureImporters = textureImporters;
            allTextures = textureAssets;
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            for (int i = 0; i < textureImporters.Count; i++)
            {
                PS_Utils.CallProgressBar("Assessing Textures", allTextures[i].objName, textureImporters.Count, i);

                if (textureImporters[i].filterMode == FilterMode.Trilinear)
                {
                    if (!textureImporters[i].mipmapEnabled)
                    {
                        targetTextures.Add(allTextures[i]);
                    }
                }
            }

            if (targetTextures.Count > 0)
            {
                DESCRIPTION = string.Format("There are {0} textures that have Trilinear filtering enabled, but have \"Generate Mip-Maps\" disabled. Trilinear filtering heavily relies on usage of MipMaps and is useless with it disabled.", targetTextures.Count);
                REPORT.Populate(ID, targetTextures, TITLE, DESCRIPTION, SOLUTION, false);
                REPORT.hasPassed = false;
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif