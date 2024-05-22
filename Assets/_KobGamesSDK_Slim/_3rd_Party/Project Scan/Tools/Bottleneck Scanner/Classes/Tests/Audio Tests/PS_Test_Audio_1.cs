#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Checks if there are any short Audio Clips that use a PCM Compression
    /// </summary>
    public class PS_Test_Audio_1 : PS_SubTest
    {
        private List<AudioImporter> AllAudioImporters;
        private List<PS_Object> targetObjects, AllAudioClips;

        public PS_Test_Audio_1(List<AudioImporter> AllAudioImporters, List<PS_Object> AllAudioClips)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.AUDIO;
            ID = 0;
            TITLE = "Short Audio Clips Compression Settings";
            DESCRIPTION = "Short Clips which are frequently used will not benefit from decompression and may even cause delays when playing sound effects for the first time.";
            SOLUTION = "Ensure that you're using the PCM Compression Format";

            targetObjects = new List<PS_Object>();

            this.AllAudioImporters = AllAudioImporters;
            this.AllAudioClips = AllAudioClips;
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            for (int i = 0; i < AllAudioClips.Count; i++)
            {
                PS_Utils.CallProgressBar("Assessing Audio Clips", AllAudioClips[i].objName, AllAudioClips.Count, i);

                if (((AudioClip)(AllAudioClips[i].obj)).length <= 2)                //check if clip is short
                {
                    if (AllAudioImporters[i].GetOverrideSampleSettings("Standalone").compressionFormat != AudioCompressionFormat.PCM)  //check if it's not mono
                    {
                        targetObjects.Add(AllAudioClips[i]);
                    }
                }
            }

            if (targetObjects.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, targetObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else if (targetObjects.Count == 0)
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif