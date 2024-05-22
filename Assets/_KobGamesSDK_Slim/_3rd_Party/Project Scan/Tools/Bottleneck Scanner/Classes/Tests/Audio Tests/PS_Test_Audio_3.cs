#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This class checks if there are more than one audio listeners in the scene
    /// </summary>
    public class PS_Test_Audio_3 : PS_SubTest
    {
        private List<PS_ObjectCategory> AUDIO_CATEGORIES;
        private List<PS_Object> AllGameObjects, targetObjects;

        public PS_Test_Audio_3(List<PS_Object> AllGameObjects)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.AUDIO;
            ID = 2;
            TITLE = "Multiple Audio Listeners in the scene";
            DESCRIPTION = "Unity supports only one Audio Listener per Scene";
            SOLUTION = "Remove or disable one of the Audio Listeners";

            this.AllGameObjects = AllGameObjects;

            targetObjects = new List<PS_Object>();
            AUDIO_CATEGORIES = new List<PS_ObjectCategory>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            for (int i = 0; i < AllGameObjects.Count; i++)
            {
                GameObject psGO = (GameObject)AllGameObjects[i].obj;

                PS_Utils.CallProgressBar("Assessing Audio Clips", psGO.name, AllGameObjects.Count, i);

                if (psGO.GetComponent<AudioListener>())
                {
                    int resultIndex = AUDIO_CATEGORIES.FindIndex(x => x.categoryScene == psGO.scene);

                    if (resultIndex >= 0)
                    {
                        AUDIO_CATEGORIES[resultIndex].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                    else
                    {
                        AUDIO_CATEGORIES.Add(new PS_ObjectCategory(psGO.scene));
                        AUDIO_CATEGORIES[AUDIO_CATEGORIES.Count - 1].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                }
            }

            for (int i = 0; i < AUDIO_CATEGORIES.Count; i++)
            {
                PS_ObjectCategory CATEGORY = AUDIO_CATEGORIES[i];

                if (CATEGORY.objectsList.Count > 1)
                {
                    targetObjects.AddRange(CATEGORY.objectsList);
                }
            }

            if (targetObjects.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, targetObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif