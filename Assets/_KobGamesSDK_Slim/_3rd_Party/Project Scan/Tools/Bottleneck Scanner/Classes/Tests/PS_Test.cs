#if UNITY_EDITOR

using System.Collections.Generic;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used to store, manage and execute subtests of relevant category
    /// </summary>
    [System.Serializable]
    public abstract class PS_Test
    {
        /// <summary>
        /// Stores all relevant subtests
        /// </summary>
        protected PS_SubTest[] TESTS;

        /// <summary>
        /// Stores all REPORTS collected from subtests
        /// </summary>
        public List<PS_Result> RESULTS;

        /// <summary>
        /// Specifies a Category this Test belongs to
        /// </summary>
        protected PS_Result.CATEGORY TestCategory;

        /// <summary>
        /// Constructor that initializes RESULTS List
        /// </summary>
        public PS_Test()
        {
            RESULTS = new List<PS_Result>();
        }

        /// <summary>
        /// Retrieves necessary data such as directories, gameobjects etc. that are crucial for subtests
        /// </summary>
        public abstract void RetrieveData();

        /// <summary>
        /// Runs all subtests
        /// </summary>
        public abstract void Run();
    }
}

#endif