using UnityEngine;

namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "PluginsVersion")]
    public class PluginsVersion : SingletonScriptableObject<PluginsVersion>
    {
        public string Version = "0.1";
    }
}