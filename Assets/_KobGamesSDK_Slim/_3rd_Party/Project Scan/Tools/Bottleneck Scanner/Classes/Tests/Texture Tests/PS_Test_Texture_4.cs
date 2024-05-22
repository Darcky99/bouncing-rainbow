#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This class checks for Textures which have no compression of any sort
    /// </summary>
    public class PS_Test_Texture_4 : PS_SubTest
    {
        private List<PS_Object> allTextures, targetTextures;
        private List<TextureImporter> textureImporters;

        public PS_Test_Texture_4(List<TextureImporter> textureImporters, List<PS_Object> textureAssets)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.TEXTURE;
            ID = 20;
            TITLE = "Uncompressed textures detected";
            DESCRIPTION = "There are some textures that do not use compression of any sort. Unless the texture is small, runtime performance may worsen along with an increased build size.";
            SOLUTION = "Change compression setting to at least \"Normal Quality\"";

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

                if (textureImporters[i].textureCompression == TextureImporterCompression.Uncompressed)
                {
                    targetTextures.Add(allTextures[i]);
                }
            }

            if (targetTextures.Count > 0)
            {
                DESCRIPTION = string.Format("There are {0} textures that do not use compression of any sort. Unless the texture is small, runtime performance may worsen along with an increased build size.", targetTextures.Count);
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