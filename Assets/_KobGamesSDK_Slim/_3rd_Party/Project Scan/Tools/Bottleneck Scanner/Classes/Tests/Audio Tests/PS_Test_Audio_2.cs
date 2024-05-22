#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This subtest checks if large audio files are set to be "Streamed"
    /// </summary>
    public class PS_Test_Audio_2 : PS_SubTest
    {
        private List<PS_Object> AllAudioClips, targetObjects;
        private List<AudioImporter> AllAudioImporters;

        public PS_Test_Audio_2(List<AudioImporter> AllAudioImporters, List<PS_Object> AllAudioClips)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.AUDIO;
            ID = 1;
            TITLE = "Large Audio files Load Type set to \"Streaming\"";
            DESCRIPTION = "There are {0} long audio clips whose Load Type is set to \"Streaming\". While the audio file itself loads fast, it causes an increase of build size. \"Streaming\" load type is usually used for short audio clips or at least clips that are used often.";
            SOLUTION = "Set the Load Type to \"Decompress on Load\"";

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

                if (((AudioClip)(AllAudioClips[i].obj)).length >= 30)                //check if clip is long
                {
                    if (AllAudioImporters[i].GetOverrideSampleSettings("Standalone").loadType == AudioClipLoadType.Streaming)  //check if it's of "Streaming" type
                    {
                        targetObjects.Add(AllAudioClips[i]);
                    }
                }
            }

            if (targetObjects.Count > 0)
            {
                REPORT.hasPassed = false;
                DESCRIPTION = string.Format("There are {0} long audio clips whose Load Type is set to \"Streaming\". While the audio file itself loads fast, it causes an increase of build size. \"Streaming\" load type is usually used for short audio clips or at least clips that are used often.", targetObjects.Count);
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