using DG.Tweening;
using KobGamesSDKSlim.Animation;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim.Collectable
{
    [System.Serializable, CreateAssetMenu(menuName = "KobGames/Collectables Data", fileName = "Collectables Data"), InlineEditor]
    public class AnimationData : ScriptableObject
    {
        public SendAnimDataDictionary SendAnim;
        public ReceiveAnimDataDictionary ReceiveAnim;
        public EarnMessageAnimDataDictionary EarnMessageAnim;

        public HapticsData Haptics;

        [System.Serializable] public class SendAnimDataDictionary : UnitySerializedDictionary<eCollectableSendAnimType, SendAnimData> { }
        [System.Serializable] public class ReceiveAnimDataDictionary : UnitySerializedDictionary<eCollectableReceiveAnimType, ReceiveAnimData> { }
        [System.Serializable] public class EarnMessageAnimDataDictionary : UnitySerializedDictionary<eCollectableEarnMessageAnimType, EarnMessageAnimData> { }

        [System.Serializable]
        public class SendAnimData
        {
            public eAnimMode AnimMode;

            [ShowIf(nameof(isLinearAnimMode)), LabelText("Parameters")] public LinearAnimData LinearAnimMode;
            [ShowIf(nameof(isBurstAnimMode)), LabelText("Parameters")] public BurstAnimData BurstAnimMode;

            private bool isLinearAnimMode => AnimMode == eAnimMode.Linear;
            private bool isBurstAnimMode => AnimMode == eAnimMode.Burst;

            [System.Serializable]
            public class LinearAnimData : BaseAnimData
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
            public class BurstAnimData : BaseAnimData
            {
                public float BurstRadius;
                public Vector2 BurstOffset;
                [Range(0f, 1f)] public float RadiusThickness;

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
            public class BaseAnimData
            {
                public bool IsSpecificAmount = true;
                [ShowIf(nameof(IsSpecificAmount))] public int SendAmount;
                [HideIf(nameof(IsSpecificAmount))] public int ItemCost;
                [HideIf(nameof(IsSpecificAmount))] public int MaxAmount;

                public float DelayBetween;
                public AnimationCurve DelayCurve;
            }

            public enum eAnimMode
            {
                Linear,
                Burst
            }
        }

        [System.Serializable]
        public class ReceiveAnimData
        {
            public eAnimMode AnimMode;

            public eShowMode ShowMode;

            public bool IsPunchIconScale = true;

            [ShowIf(nameof(isShowIncrease)), LabelText("Increase Data")] public BurstAnimData IncreaseAnimMode;
            [ShowIf(nameof(isShowDecrease)), LabelText("Decrease Data")] public BurstAnimData DecreaseAnimMode;

            private bool isBurstAnimMode => AnimMode == eAnimMode.Burst;
            private bool isShowDecrease => isBurstAnimMode && (ShowMode == eShowMode.Both || ShowMode == eShowMode.OnDecreaseAmount);
            private bool isShowIncrease => isBurstAnimMode && (ShowMode == eShowMode.Both || ShowMode == eShowMode.OnIncreaseAmount);


            [System.Serializable]
            public class BurstAnimData : BaseAnimData
            {
                public float BurstRadius;
                public Vector2 BurstOffset;
                [Range(0f, 1f)] public float RadiusThickness;

                public bool IsLimitAngle;
                [ShowIf(nameof(IsLimitAngle))] public float AngleLimitMin;
                [ShowIf(nameof(IsLimitAngle))] public float AngleLimitMax;

                [Header("Move")]
                public float MoveDuration;
                public Ease MoveEase;

                [Header("Rotate")]
                public bool IsRotate;
                [ShowIf(nameof(IsRotate))] public float RotateOffsetMax;
                [ShowIf(nameof(IsRotate))] public Ease RotateEase;

                [Header("Scale")]
                public bool IsScale;
                [ShowIf(nameof(IsScale)), Range(0f, 1f)] public float FinalScale;
                [ShowIf(nameof(IsScale))] public Ease ScaleEase;

                [Header("Fade")]
                public bool IsFade;
                [ShowIf(nameof(IsFade))] public float FinalFadeValue;
                [ShowIf(nameof(IsFade))] public Ease FadeEase;
            }

            [System.Serializable]
            public class BaseAnimData
            {
                public int ItemCost;
                public int MaxAmount = 15;

                public float DelayBetween;
                public AnimationCurve DelayCurve;
            }

            public enum eAnimMode
            {
                Burst
            }

            public enum eShowMode
            {
                OnIncreaseAmount,
                OnDecreaseAmount,
                Both,
                None
            }
        }

        [System.Serializable]
        public class EarnMessageAnimData
        {
            public eAnimMode AnimMode;

            [ShowIf(nameof(isSimpleText)), LabelText("Parameters")] public SimpleTextAnimData SimpleTextAnimMode;

            private bool isSimpleText => AnimMode == eAnimMode.SimpleText;

            public float SizeMultiplier = 1f;

            public bool IsSpawnParticles;

            [ShowIf(nameof(IsSpawnParticles))] public ParticlesData Particles;

            [System.Serializable]
            public class SimpleTextAnimData : BaseAnimData
            {
                public bool IsShowPanel;
                [HideIf(nameof(IsShowPanel))] public bool IsShowText;
                [ShowIf(nameof(IsShowPanel))] public Color PanelColor;

                [FoldoutGroup("Phase 0 - Start")] public float StartScale;
                [FoldoutGroup("Phase 0 - Start")] public float StartFade;
                [FoldoutGroup("Phase 0 - Start")] public float StartOffset;

                [FoldoutGroup("Phase 1 - Appear")] public float AppearDuration;
                [FoldoutGroup("Phase 1 - Appear")] public float AppearScale;
                [FoldoutGroup("Phase 1 - Appear")] public float AppearFade;
                [FoldoutGroup("Phase 1 - Appear")] public float AppearOffset;
                [FoldoutGroup("Phase 1 - Appear")] public Ease AppearEase;

                [FoldoutGroup("Phase 2 - Move")] public float MoveDuration;
                [FoldoutGroup("Phase 2 - Move")] public float MoveScale;
                [FoldoutGroup("Phase 2 - Move")] public float MoveFade;
                [FoldoutGroup("Phase 2 - Move")] public float MoveOffset;
                [FoldoutGroup("Phase 2 - Move")] public Ease MoveEase;

                [FoldoutGroup("Phase 3 - Disappear")] public float DisappearDuration;
                [FoldoutGroup("Phase 3 - Disappear")] public float DisappearScale;
                [FoldoutGroup("Phase 3 - Disappear")] public float DisappearFade;
                [FoldoutGroup("Phase 3 - Disappear")] public float DisappearOffset;
                [FoldoutGroup("Phase 3 - Disappear")] public Ease DisappearEase;
            }

            [System.Serializable]
            public class BaseAnimData
            {

            }

            [System.Serializable]
            public class ParticlesData
            {
                public float ItemCost;
                public float MaxAmount;

                public Vector2 ParticlesLifetime;
                public Vector2 ParticlesSize;
                public Vector2 Speed;
                public float SpawnConeAngle;
                public float SpawnConeRadius;

                public Vector3 SpawnParticlesOffset;
                public float SpawnDelay;
            }

            public enum eAnimMode
            {
                SimpleText
            }
        }

        [System.Serializable]
        public class HapticsData
        {
            public bool IsPlayHapticsOnReceive = true;
            [ShowIf(nameof(IsPlayHapticsOnReceive))] public HapticTypes ReceivedHaptics = HapticTypes.Selection;
            [ShowIf(nameof(IsPlayHapticsOnReceive))] public HapticTypes CompleteHaptics = HapticTypes.MediumImpact;
            [ShowIf(nameof(IsPlayHapticsOnReceive))] public float HapticsCooldown = 0.05f;
        }
    }
}