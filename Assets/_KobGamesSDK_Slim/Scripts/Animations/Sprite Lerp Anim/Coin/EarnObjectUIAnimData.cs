using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim.Animation
{
    [System.Serializable]
    public class EarnObjectUIAnimData
    {
        public eEarnObjectSpawnMode SpawnMode;

        [ShowIf(nameof(isLinearMode)), LabelText("Parameters")] public LinearAnimData LinearMode;
        [ShowIf(nameof(isBurstCircularMode)), LabelText("Parameters")] public BurstAnimData BurstMode;
        
        private bool isLinearMode => SpawnMode == eEarnObjectSpawnMode.Linear;
        private bool isBurstCircularMode => SpawnMode == eEarnObjectSpawnMode.Burst;

        [System.Serializable]
        public class LinearAnimData : AnimData
        {
            [Header("Phase 1 - Scale Up and Move")]
            public float InitialSizeDeltaAmount;
            public float InitialSizeDeltaDuration;
            public Ease InitialSizeDeltaEase;
            public Vector3 InitialMoveOffset;
            public Ease InitialMoveEase;

            [Header("Phase 2 - Scale Down")]
            public float FinalSizeDeltaAmount;
            public float FinalSizeDeltaDuration;
            public Ease FinalSizeDeltaEase;

            [Header("Phase 3 - Animate to Final Pos")]
            public TweenData FinalAnimData;

        }

        [System.Serializable]
        public class BurstAnimData : AnimData
        {
            public float BurstRadius;
            public Vector2 BurstOffset;
            [Range(0f,1f)] public float RadiusThickness;

            public bool IsLimitAngle;
            [ShowIf(nameof(IsLimitAngle))] public float AngleLimitMin;
            [ShowIf(nameof(IsLimitAngle))] public float AngleLimitMax;

            [Header("Phase 1 - Scale Up and move")]
            public float BurstDuration;
            public float BurstSizeDeltaAmount;
            public Ease BurstMoveEase;
            public Ease BurstSizeDeltaEase;

            [Header("Phase 2 - Wait and scale down")]
            public float WaitSizeDeltaAmount;
            public Ease WaitSizeDeltaEase;
            public float WaitSizeDeltaDuration;

            [Header("Phase 3 - Animate to Final Pos")]
            public TweenData FinalAnimData;
        }

        [System.Serializable]
        public class AnimData
        {
            public int CoinsNumber;

            public float DelayBetweenCoins;
            public AnimationCurve DelayBetweenCoinsCurve;
        }
    }

    public enum eEarnObjectSpawnMode
    {
        Linear,
        Burst
    }
}