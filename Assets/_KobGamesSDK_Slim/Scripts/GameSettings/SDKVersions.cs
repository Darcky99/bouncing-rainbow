using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class SDKVersions : MonoBehaviour
    {        
        public string PluginsVersion = "0.1";

        [TableList]
        public List<SDKVersionInfo> SDKVersionsInfo = new List<SDKVersionInfo>();
    }
}
