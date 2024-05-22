#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used to store Bottleneck Scan results and dismissed results' IDs
    /// </summary>
    public class PS_Data_BottleneckResults : ScriptableObject
    {
        [Header("Bottleneck Scan Result Data")]
        public PS_BottleneckTest bottleneckResults;                             // Latest PerformanceTest result

        public List<int> dismissedResultsID = new List<int>();                  // IDs of dismissed Bottleneck results
    }
}

#endif