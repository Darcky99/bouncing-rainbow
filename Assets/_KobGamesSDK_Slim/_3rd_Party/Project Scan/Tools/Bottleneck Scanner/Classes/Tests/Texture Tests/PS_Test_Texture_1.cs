#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This class tests whether textures use Trilinear filtering sparingly
    /// </summary>
    [System.Serializable]
    public class PS_Test_Texture_1 : PS_SubTest
    {
        private List<PS_Object> allTextures;
        private List<PS_Object> targetTextures;
        private List<TextureImporter> textureImporters;

        public PS_Test_Texture_1(List<TextureImporter> textureImporters, List<PS_Object> textureAssets)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.TEXTURE;
            ID = 17;
            TITLE = "There are too many textures that use Trilinear rendering";
            DESCRIPTION = "Trilinear rendering relies on blending Mip-maps, which in turn takes more rendering power";
            SOLUTION = "Use Trilinear rendering sparingly. Bilinear is a great alternative";

            allTextures = textureAssets;
            this.textureImporters = textureImporters;
            targetTextures = new List<PS_Object>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            int TrilinearTextures = 0;
            int BilinearTextures = 0;
            int PointTextures = 0;

            for (int i = 0; i < textureImporters.Count; i++)
            {
                PS_Utils.CallProgressBar("Assessing Textures", allTextures[i].objName, textureImporters.Count, i);

                switch (textureImporters[i].filterMode)
                {
                    case FilterMode.Point:
                        PointTextures += 1;
                        break;

                    case FilterMode.Bilinear:
                        BilinearTextures += 1;
                        break;

                    case FilterMode.Trilinear:
                        TrilinearTextures += 1;
                        targetTextures.Add(allTextures[i]);
                        break;
                }
            }

            if (textureImporters.Count > 0)
            {
                if (TrilinearTextures > 0)
                {
                    if (TrilinearTextures >= BilinearTextures)
                    {
                        DESCRIPTION = "There are " + TrilinearTextures + " textures that use a Trilinear Filter. Trilinear Filters use Mip-Map blending, which takes additional computation time.";
                        SOLUTION = "Use Trilinear Filters sparingly and use Bilinear Filtering instead (unless you absolutely have to).";

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
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif