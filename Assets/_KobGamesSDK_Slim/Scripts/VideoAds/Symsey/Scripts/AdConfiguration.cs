
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Video;


namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "CrossPromoData", menuName = "Ad Configuration")]
    public class AdConfiguration : ScriptableObject
    {
        public bool IOS;
        [ShowIf(nameof(IOS), true)]
        public string IOSAppID;

        public bool Android;
        [ShowIf(nameof(Android), true)]
        public string AndroidAppID;

        public VideoClip VideoClip;


    }

}