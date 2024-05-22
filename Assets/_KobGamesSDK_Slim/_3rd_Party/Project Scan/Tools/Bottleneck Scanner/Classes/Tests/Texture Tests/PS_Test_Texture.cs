#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs Texture tests
    /// </summary>
    public class PS_Test_Texture : PS_Test
    {
        private List<PS_Object> AllTextures;
        private List<TextureImporter> AllTextureImporters;

        public PS_Test_Texture()
        {
            TESTS = new PS_SubTest[6];

            TestCategory = PS_Result.CATEGORY.TEXTURE;

            RetrieveData();

            TESTS[0] = new PS_Test_Texture_1(AllTextureImporters, AllTextures);
            TESTS[1] = new PS_Test_Texture_2(AllTextureImporters, AllTextures);
            TESTS[2] = new PS_Test_Texture_3(AllTextureImporters, AllTextures);
            TESTS[3] = new PS_Test_Texture_4(AllTextureImporters, AllTextures);
            TESTS[4] = new PS_Test_Texture_5(AllTextureImporters, AllTextures);
            TESTS[5] = new PS_Test_Texture_6(AllTextures);
        }

        public override void RetrieveData()
        {
            AllTextures = new List<PS_Object>();
            AllTextureImporters = new List<TextureImporter>();

            string[] textureFormats = new string[] { "psd", "jpg", "png", "gif", "bmp", "tga", "tiff", "iff", "pict", "dds" };
            int count = PS_Utils.ALL_FILE_PATHS.Length;

            for (int i = 0; i < count; i++)
            {
                string fileExtension = Path.GetExtension(PS_Utils.ALL_FILE_PATHS[i]).Replace(".", "");

                if (textureFormats.Contains(fileExtension))
                {
                    string texturePath = "Assets" + PS_Utils.ALL_FILE_PATHS[i].Substring(Application.dataPath.Length);

                    TextureImporter newImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

                    if (newImporter != null)
                    {
                        PS_Utils.CallProgressBar("Gathering Textures", texturePath, i, count);

                        AllTextureImporters.Add(newImporter);

                        AllTextures.Add(new PS_Object(
                            AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)), PS_Object.TYPE.ASSET
                            ));
                    }
                }
            }
        }

        public override void Run()
        {
            if (AllTextures.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing Texture Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling Texture Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif