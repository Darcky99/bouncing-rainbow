using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class SDKVersionInfo
    {
        public string Name;
        public string Version;
        [InlineButton(nameof(Open))]
        public string URL;

        public void Open()
        {
            Application.OpenURL(URL);
        }
    }
}
