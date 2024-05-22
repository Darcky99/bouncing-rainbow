#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  This test checks if there any transparent pixels around the image
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Texture_6 : PS_SubTest
    {
        private List<PS_Object> allTextures;
        private List<PS_Object> targetTextures;

        public PS_Test_Texture_6(List<PS_Object> textureAssets)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.TEXTURE;
            ID = 22;
            TITLE = "Useless transparent pixels around the image";
            DESCRIPTION = "There is a potential to reduce the size of an image by trimming unused transparent pixels";
            SOLUTION = "Get rid of transparent pixels around the image";

            allTextures = textureAssets;
            targetTextures = new List<PS_Object>();
        }

        public override void Check()
        {
            for (int i = 0; i < allTextures.Count; i++)
            {
                PS_Utils.CallProgressBar("Assessing Textures", allTextures[i].objName, allTextures.Count, i);

                var texture = allTextures[i].obj as Texture2D;

                if (texture == null)
                    continue;

                var tmp = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

                Graphics.Blit(texture, tmp);

                var previous = RenderTexture.active;
                RenderTexture.active = tmp;
                var img = new Texture2D(texture.width, texture.height);
                img.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
                img.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);

                int WIDTH = img.width;
                int HEIGHT = img.height;

                bool
                    sideA_Transparent = true,
                    sideB_Transparent = true,
                    sideC_Transparent = true,
                    sideD_Transparent = true;

                //get pixels of top side of an image
                for (int A = 0; A < WIDTH - 1; A++)
                {
                    if (img.GetPixel(A, 0).a > 0)
                    {
                        sideA_Transparent = false;
                        break;
                    }
                }

                //get pixels of right side of an image
                for (int B = 0; B < HEIGHT - 1; B++)
                {
                    if (img.GetPixel(WIDTH - 1, B).a > 0)
                    {
                        sideB_Transparent = false;
                        break;
                    }
                }

                //get pixels of the bottom side of an image
                for (int C = 1; C <= WIDTH - 1; C++)
                {
                    if (img.GetPixel(C, HEIGHT - 1).a > 0)
                    {
                        sideC_Transparent = false;
                        break;
                    }
                }

                //get pixels of the left side of an image
                for (int D = 1; D <= HEIGHT - 1; D++)
                {
                    if (img.GetPixel(0, D).a > 0)
                    {
                        sideD_Transparent = false;
                        break;
                    }
                }

                if (sideA_Transparent || sideB_Transparent || sideC_Transparent || sideD_Transparent)
                {
                    targetTextures.Add(allTextures[i]);
                }
            }


            if (targetTextures.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, targetTextures, TITLE, DESCRIPTION, SOLUTION, false);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif