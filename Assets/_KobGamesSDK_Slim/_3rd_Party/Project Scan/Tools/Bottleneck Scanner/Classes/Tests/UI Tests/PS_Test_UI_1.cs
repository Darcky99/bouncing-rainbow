#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

/*  DESCRIPTION =================================================================
 *  This class tests whether there's a single Canvas and has a lot of UI Elements
 *  ============================================================================= */

namespace HardCodeLab.Utils.ProjectScan
{
    [System.Serializable]
    public class PS_Test_UI_1 : PS_SubTest
    {
        private List<PS_ObjectCategory> CANVAS_CATEGORIES;
        public List<PS_Object> AllCanvases;
        private List<PS_Object> affectedCanvases;

        public PS_Test_UI_1(List<PS_Object> AllCanvases)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.UI;
            ID = 23;
            TITLE = "Too many UI elements in a single Canvas GameObject";
            DESCRIPTION = "Having a single Canvas that handles tons of UI GameObjects makes the hierarchy cluttered, making it hard to manage";
            SOLUTION = "Split up one large Canvas into smaller Canvases with relevant UI elements inside. E.g, a Canvas dedicated to Main Menu and a Canvas dedicated for Game-play elements";

            this.AllCanvases = AllCanvases;

            CANVAS_CATEGORIES = new List<PS_ObjectCategory>();
            affectedCanvases = new List<PS_Object>();
        }

        public override void Check()
        {
            if (PS_Utils.GetData_BottleneckSettings().BENCHMARK_ENABLED && PS_Utils.GetData_BottleneckSettings().BENCHMARK_UI_maxObjectsPerCanvas > 0)
            {
                for (int i = 0; i < AllCanvases.Count; i++)
                {
                    GameObject psGO = (GameObject)AllCanvases[i].obj;

                    int resultIndex = CANVAS_CATEGORIES.FindIndex(x => x.categoryScene == psGO.scene);

                    if (resultIndex >= 0)
                    {
                        CANVAS_CATEGORIES[resultIndex].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                    else
                    {
                        CANVAS_CATEGORIES.Add(new PS_ObjectCategory(psGO.scene));
                        CANVAS_CATEGORIES[CANVAS_CATEGORIES.Count - 1].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                    }
                }

                for (int i = 0; i < CANVAS_CATEGORIES.Count; i++)
                {
                    PS_ObjectCategory CATEGORY = CANVAS_CATEGORIES[i];

                    for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                    {
                        GameObject canvasGO = (GameObject)CATEGORY.objectsList[j].obj;

                        if (canvasGO.transform.childCount >= PS_Utils.GetData_BottleneckSettings().BENCHMARK_UI_maxObjectsPerCanvas)
                        {
                            affectedCanvases.Add(CATEGORY.objectsList[j]);
                        }
                    }
                }

                if (affectedCanvases.Count > 0)
                {
                    REPORT.hasPassed = false;
                    REPORT.Populate(ID, affectedCanvases, TITLE, DESCRIPTION, SOLUTION);
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