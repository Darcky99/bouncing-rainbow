#if UNITY_EDITOR

/*  DESCRIPTION =================================================================
 *  This test checks if any of UI components are outside their Canvas
 *  ============================================================================= */

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_Test_UI_2 : PS_SubTest
    {
        private List<PS_ObjectCategory> CANVAS_CATEGORIES;
        private List<PS_Object> AllCanvases;
        private List<PS_Object> affectedCanvases;

        public PS_Test_UI_2(List<PS_Object> AllCanvases)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.UI;
            ID = 24;
            TITLE = "Some UI components are outside their Canvases";
            DESCRIPTION = "May lead to visual problems";
            SOLUTION = "Unless you're planning to change their position at runtime, try moving UI components into Canvas";

            this.AllCanvases = AllCanvases;

            CANVAS_CATEGORIES = new List<PS_ObjectCategory>();
            affectedCanvases = new List<PS_Object>();
        }

        public override void Check()
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

            for (int a = 0; a < CANVAS_CATEGORIES.Count; a++)
            {
                PS_ObjectCategory CATEGORY = CANVAS_CATEGORIES[a];

                for (int b = 0; b < CATEGORY.objectsList.Count; b++)
                {
                    GameObject canvasGO = (GameObject)CATEGORY.objectsList[b].obj;

                    float canvasWidth = canvasGO.GetComponent<RectTransform>().rect.width;
                    float canvasHeight = canvasGO.GetComponent<RectTransform>().rect.height;

                    for (int c = 0; c < canvasGO.transform.childCount; c++)
                    {
                        GameObject canvasChild = canvasGO.transform.GetChild(c).gameObject;

                        if (!InsideCanvas(canvasChild, canvasWidth, canvasHeight))
                        {
                            affectedCanvases.Add(new PS_Object(
                                    canvasChild, PS_Object.TYPE.GAMEOBJECT
                                ));
                        }
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

        /// <summary>
        /// Checks whether given UI component is inside of its Canvas
        /// </summary>
        /// <param name="comp">GameObject UI to check</param>
        /// <param name="canvasWidth">Width of Canvas select UI is in</param>
        /// <param name="canvasHeight">Height of Canvas select UI is in</param>
        /// <returns></returns>
        private bool InsideCanvas(GameObject comp, float canvasWidth, float canvasHeight)
        {
            RectTransform rectTrans = comp.GetComponent<RectTransform>();

            float right = -rectTrans.offsetMax.x + rectTrans.anchorMin.x * canvasWidth;
            float left = rectTrans.offsetMin.x + rectTrans.anchorMin.x * canvasWidth;
            float top = -rectTrans.offsetMax.y + rectTrans.anchorMin.y * canvasHeight;
            float bottom = rectTrans.offsetMin.y + rectTrans.anchorMin.y * canvasHeight;

            if (
                right < 0 ||
                left < 0 ||
                top < 0 ||
                bottom < 0
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

#endif