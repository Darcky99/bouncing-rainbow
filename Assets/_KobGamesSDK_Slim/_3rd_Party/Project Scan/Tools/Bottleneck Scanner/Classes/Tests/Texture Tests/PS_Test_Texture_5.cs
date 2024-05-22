#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

/*  DESCRIPTION =================================================================
 *  This class checks whether Mip-Maps are enabled whilst the game is in 2D mode
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Texture_5 : PS_SubTest
    {
        private List<PS_Object> allTextures, targetTextures;
        private List<TextureImporter> textureImporters;

        public PS_Test_Texture_5(List<TextureImporter> textureImporters, List<PS_Object> textureAssets)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.TEXTURE;
            ID = 21;
            TITLE = "Sprites with Mip-Maps enabled";
            DESCRIPTION = "There are some textures of \"Sprite\" type with Mip-Maps enabled. Because 2D games and UI usually do not have a change in depth (thanks to orthographic camera mode), Mip-Maps not only useless, but also result in higher project and build size.";
            SOLUTION = "Make sure that \"Generate Mip Map\" option is unchecked";

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

                if (textureImporters[i].textureType == TextureImporterType.Sprite && textureImporters[i].mipmapEnabled)
                {
                    targetTextures.Add(allTextures[i]);
                }
            }

            if (targetTextures.Count > 0)
            {
                DESCRIPTION = string.Format("There are {0} textures of \"Sprite\" type with Mip-Maps enabled. Because 2D games and UI usually do not have a change in depth (thanks to orthographic camera mode), Mip-Maps are not necessary.", targetTextures.Count);
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