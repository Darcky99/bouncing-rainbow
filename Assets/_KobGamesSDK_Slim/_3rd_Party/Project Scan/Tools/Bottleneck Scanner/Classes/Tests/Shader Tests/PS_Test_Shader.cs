#if UNITY_EDITOR

using System;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Runs test related to Shader category
    /// </summary>
    [System.Serializable]
    public class PS_Test_Shader : PS_Test
    {
        public PS_Test_Shader()
        {
            TESTS = new PS_SubTest[1];

            TestCategory = PS_Result.CATEGORY.SHADER;
        }

        public override void RetrieveData()
        {
            throw new NotImplementedException();
        }

        public override void Run()
        {
        }
    }
}

#endif