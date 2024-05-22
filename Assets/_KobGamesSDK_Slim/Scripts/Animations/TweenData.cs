using DG.Tweening;
using System;

//TODO - better namespace?
namespace KobGamesSDKSlim.Animation
{
    [Serializable]
    public struct TweenData
    {
        public float Delay;
        public float Duration;
        public Ease Ease;

        public int LoopCount;

        public TweenData(float i_Delay, float i_Duration, Ease i_Ease, int i_LoopCount)
        {
            Delay = i_Delay;
            Duration = i_Duration;
            Ease = i_Ease;
            LoopCount = i_LoopCount;
        }
    }

    [Serializable]
    public struct TweenDataInOut
    {
        public float DurationIn;
        public float DurationOut;

        public Ease EaseIn;
        public Ease EaseOut;
    }
}