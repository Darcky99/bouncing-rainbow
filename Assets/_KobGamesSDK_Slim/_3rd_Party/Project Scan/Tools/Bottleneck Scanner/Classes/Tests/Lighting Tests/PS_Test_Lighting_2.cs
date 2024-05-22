#if UNITY_EDITOR

/*  DESCRIPTION =================================================================
 *  This class checks Dynamic Shadows' settings
 *  ============================================================================= */

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    [System.Serializable]
    public class PS_Test_Lighting_2 : PS_SubTest
    {
        private List<PS_ObjectCategory> LIGHT_CATEGORIES;

        private bool IS_MOBILE = false;

        private List<PS_Object> AllLights;
        private List<PS_Object> targetObjects;

        public PS_Test_Lighting_2(bool IS_MOBILE, List<PS_Object> AllLights)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.LIGHTING;
            ID = 10;
            TITLE = "Real-time Shadows in a Mobile Game";
            DESCRIPTION = "Real-time Shadows may affect the performance of your game due to mobile's limited capabilities";
            SOLUTION = "Change Baking to \"Baked\" in the Light component";

            this.IS_MOBILE = IS_MOBILE;
            this.AllLights = AllLights;
            LIGHT_CATEGORIES = new List<PS_ObjectCategory>();
            targetObjects = new List<PS_Object>();
        }

        public override void Check()
        {
            if (IS_MOBILE)
            {
                for (int i = 0; i < AllLights.Count; i++)
                {
                    GameObject psGO = (GameObject)AllLights[i].obj;

                    int resultIndex = LIGHT_CATEGORIES.FindIndex(x => x.categoryScene == psGO.scene);

                    if (resultIndex >= 0)
                    {
                        LIGHT_CATEGORIES[resultIndex].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                    else
                    {
                        LIGHT_CATEGORIES.Add(new PS_ObjectCategory(psGO.scene));
                        LIGHT_CATEGORIES[LIGHT_CATEGORIES.Count - 1].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                }

                for (int i = 0; i < LIGHT_CATEGORIES.Count; i++)
                {
                    PS_ObjectCategory CATEGORY = LIGHT_CATEGORIES[i];

                    for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                    {
                        GameObject gObj = (GameObject)CATEGORY.objectsList[j].obj;
#if UNITY_5_6_OR_NEWER
                        if (gObj.GetComponent<Light>().lightmapBakeType == LightmapBakeType.Realtime)
                        {
                            targetObjects.Add(CATEGORY.objectsList[j]);
                        }
#endif
#if UNITY_5_5
                        if (gObj.GetComponent<Light>().lightmappingMode == LightmappingMode.Realtime)
                        {
                            targetObjects.Add(CATEGORY.objectsList[j]);
                        }
#endif
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
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif