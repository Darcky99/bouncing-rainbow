using UnityEngine;
using UnityEditor;

namespace KobGamesSDKSlim
{
    public class GameConfigEditor : MonoBehaviour
    {
        [MenuItem("KobGamesSDK/Select GameConfig #%t", false, -2)]
        public static void SelectGameConfg()
        {
            Selection.activeObject = GameConfig.Instance;
        }
    }
}