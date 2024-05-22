#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// For running subtests of Audio category
    /// </summary>
    public class PS_Test_Audio : PS_Test
    {
        private List<AudioImporter> AllAudioImporters;
        private List<PS_Object> AllAudio;

        public PS_Test_Audio(List<PS_Object> AllGameObjects)
        {
            TESTS = new PS_SubTest[3];

            TestCategory = PS_Result.CATEGORY.AUDIO;

            RetrieveData();

            TESTS[0] = new PS_Test_Audio_1(AllAudioImporters, AllAudio);
            TESTS[1] = new PS_Test_Audio_2(AllAudioImporters, AllAudio);
            TESTS[2] = new PS_Test_Audio_3(AllGameObjects);
        }

        public override void RetrieveData()
        {
            int count = PS_Utils.ALL_FILE_PATHS.Length;

            AllAudio = new List<PS_Object>();
            AllAudioImporters = new List<AudioImporter>();

            string[] audioFormats = { "mp3", "ogg", "aiff", "wav", "mod", "it", "sm3" };

            for (int i = 0; i < count; i++)
            {
                string fileExtension = Path.GetExtension(PS_Utils.ALL_FILE_PATHS[i]).Replace(".", "");

                if (audioFormats.Contains(fileExtension))
                {
                    string audioPath = "Assets" + PS_Utils.ALL_FILE_PATHS[i].Substring(Application.dataPath.Length);

                    AudioImporter newImporter = AssetImporter.GetAtPath(audioPath) as AudioImporter;

                    if (newImporter != null)
                    {
                        PS_Utils.CallProgressBar("Gathering Audio", audioPath, i, count);

                        AllAudioImporters.Add(newImporter);

                        AllAudio.Add(new PS_Object( 
                            AssetDatabase.LoadAssetAtPath(audioPath, typeof(AudioClip)), PS_Object.TYPE.ASSET
                            ));
                    }
                }
            }
        }

        public override void Run()
        {
            if (AllAudio.Count > 0)
            {
                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Executing Audio Tests", i, TESTS.Length);
                    TESTS[i].Check();
                }

                for (int i = 0; i < TESTS.Length; i++)
                {
                    PS_Utils.CallProgressBar("Compiling Audio Results", i, TESTS.Length);
                    RESULTS.Add(TESTS[i].REPORT);
                }
            }
        }
    }
}

#endif