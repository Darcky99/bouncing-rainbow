using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "RateUsSchema", menuName = "RateUsSchema")]
    public class RateUsPopupSchema : ScriptableObject
    {
        public Sprite PanelBg;
        public Sprite TitleBg;
        public Color TitleTextColor;
        public Sprite StarOn;
        public Sprite StarOff;
        public Sprite YesButtonBg;
        public Color YesButtonTextColor;
    }
}