#if UNITY_EDITOR

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used to run minor tests. Ran by classes that derive from PS_Test
    /// </summary>
    public abstract class PS_SubTest
    {
        /// <summary>
        /// Report generated after SubTest has been run
        /// </summary>
        public PS_Result REPORT;

        /// <summary>
        /// Unique ID of a SubTest. Used for remembering dismissed reports
        /// </summary>
        protected int ID;

        /// <summary>
        /// Brief problem which has been found by a SubTest
        /// </summary>
        protected string TITLE;

        /// <summary>
        /// More detailed description regarding the problem and how does it impact the project as a whole
        /// </summary>
        protected string DESCRIPTION;

        /// <summary>
        /// Solution to solving the problem
        /// </summary>
        protected string SOLUTION;

        /// <summary>
        /// An external link to an external web article, normally used to provide more information regarding the problem (Optional)
        /// </summary>
        protected string URL;

        /// <summary>
        /// Runs subtest
        /// </summary>
        public abstract void Check();
    }
}

#endif