#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  This test checks if Lighting resolution isn't set to "Use Quality Settings"
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_Lighting_3 : PS_SubTest
    {
        private List<PS_ObjectCategory> LIGHT_CATEGORIES;
        private List<PS_Object> AllLights;
        private List<PS_Object> targetObjects;

        public PS_Test_Lighting_3(List<PS_Object> AllLights)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.LIGHTING;
            ID = 11;
            TITLE = "Inconsistent Shadow Resolution in some Light Sources";
            DESCRIPTION = "Inconsistent shadow quality may lead to performance hits on some consumer machines as well as visual inconsistencies due to varying shadow quality";
            SOLUTION = "Change the light's Shadow Resolution to \"Use Quality Settings\"";

            this.AllLights = AllLights;
            LIGHT_CATEGORIES = new List<PS_ObjectCategory>();
            targetObjects = new List<PS_Object>();
        }

        public override void Check()
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

                    if (gObj.GetComponent<Light>().shadowResolution != UnityEngine.Rendering.LightShadowResolution.FromQualitySettings)
                    {
                        targetObjects.Add(CATEGORY.objectsList[j]);
                    }
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