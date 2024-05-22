using UnityEngine;

namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "GameEditorConstants")]
    public class GameEditorConstants : SingletonScriptableObject<GameEditorConstants>
    {
        public string FolderFastlanePath = "/Users/kobyle/Dropbox/Mac Files/FastlaneFiles/";
        public string FolderFastlaneKobyProfile = "fastlaneKoby";
        public string FolderFastlaneYakovProfile = "fastlaneYakov";
    }
}