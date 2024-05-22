#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This class checks whether Fog is enabled while on mobile
    /// </summary>
    [System.Serializable]
    public class PS_Test_Lighting_1 : PS_SubTest
    {
        private bool IS_MOBILE = false;

        public List<PS_Object> targetObjects;

        public PS_Test_Lighting_1(bool IS_MOBILE)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.LIGHTING;
            ID = 9;
            TITLE = "Fog is enabled in Render Settings for a Mobile Platform";
            DESCRIPTION = "Fog enabled may cause performance issues on lower-end devices";
            SOLUTION = "Disable the Fog before building for a mobile game";

            this.IS_MOBILE = IS_MOBILE;
            targetObjects = new List<PS_Object>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
        public override void Check()
        {
            if (IS_MOBILE)
            {
                if (RenderSettings.fog)
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