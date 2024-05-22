using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim.Animation
{
    public class EarnObjectUI : MonoBehaviour
    {
        private EarnObjectUIAnimData m_DefaultEarnObjectUIAnimData => GameConfig.Instance.HUD.EarnObject.DefaultEarnObjectUIAnimData;

        [SerializeField, ReadOnly] private RectTransform m_RectTransform;

        private System.Action<EarnObjectUI> m_MoveCompleteCallback;

        private Sequence m_AnimSequence;

        protected virtual void OnEnable() {}

        protected virtual void OnDisable() {}

        #region Animate
        public void Animate(RectTransform i_OriginalTransform, RectTransform i_TargetTransform, System.Action<EarnObjectUI> i_CompleteMoveCallback)
        {
            Animate(i_OriginalTransform, i_TargetTransform, m_DefaultEarnObjectUIAnimData, i_CompleteMoveCallback);
        }

        public void Animate(RectTransform i_OriginalTransform, RectTransform i_TargetTransform, eEarnObjectSpawnMode i_Mode, System.Action<EarnObjectUI> i_CompleteMoveCallback)
        {
            m_MoveCompleteCallback = i_CompleteMoveCallback;

            EarnObjectUIAnimData.AnimData animData = null;
            switch (i_Mode)
            {
                case eEarnObjectSpawnMode.Linear:
                    animData = m_DefaultEarnObjectUIAnimData.LinearMode;
                    break;
                case eEarnObjectSpawnMode.Burst:
                    animData = m_DefaultEarnObjectUIAnimData.BurstMode;
                    break;
                default:
                    break;
            }

            Animate(i_OriginalTransform, i_TargetTransform, i_Mode, animData, i_CompleteMoveCallback);
        }

        public void Animate(RectTransform i_OriginalTransform, RectTransform i_TargetTransform, EarnObjectUIAnimData i_AnimData, System.Action<EarnObjectUI> i_CompleteMoveCallback)
        {
            EarnObjectUIAnimData.AnimData animData = null;
            switch (i_AnimData.SpawnMode)
            {
                case eEarnObjectSpawnMode.Linear:
                    animData = m_DefaultEarnObjectUIAnimData.LinearMode;
                    break;
                case eEarnObjectSpawnMode.Burst:
                    animData = m_DefaultEarnObjectUIAnimData.BurstMode;
                    break;
                default:
                    break;
            }

            Animate(i_OriginalTransform, i_TargetTransform, i_AnimData.SpawnMode, animData, i_CompleteMoveCallback);
        }

        public void Animate(RectTransform i_OriginalTransform, RectTransform i_TargetTransform, eEarnObjectSpawnMode i_Mode, EarnObjectUIAnimData.AnimData i_AnimData, 
            System.Action<EarnObjectUI> i_CompleteMoveCallback)
        {
            m_MoveCompleteCallback = i_CompleteMoveCallback;

            copyTarget(i_OriginalTransform);

            switch (i_Mode)
            {
                case eEarnObjectSpawnMode.Linear:
                    animateLinear(i_OriginalTransform, i_TargetTransform, i_AnimData as EarnObjectUIAnimData.LinearAnimData);
                    break;
                case eEarnObjectSpawnMode.Burst:
                    animateBurst(i_OriginalTransform, i_TargetTransform, i_AnimData as EarnObjectUIAnimData.BurstAnimData);
                    break;
                default:
                    break;
            }
        }

        private void animateLinear(RectTransform i_OriginalTransform, RectTransform i_TargetTransform, EarnObjectUIAnimData.LinearAnimData i_AnimData)
        {
            m_AnimSequence?.Kill();

            m_AnimSequence = DOTween.Sequence();

            m_AnimSequence
                .AppendCallback(()=>
                {
                    m_RectTransform.DOSizeDelta(Vector2.one * i_AnimData.InitialSizeDeltaAmount, i_AnimData.InitialSizeDeltaDuration)
                                        .SetEase(i_AnimData.InitialSizeDeltaEase);
                })
                .Append(m_RectTransform.DOAnchorPos(i_AnimData.InitialMoveOffset, i_AnimData.InitialSizeDeltaDuration)
                    .SetEase(i_AnimData.InitialSizeDeltaEase)
                    .SetRelative(true))
                .Append(m_RectTransform.DOSizeDelta(Vector2.one * i_AnimData.FinalSizeDeltaAmount, i_AnimData.FinalSizeDeltaDuration)
                    .SetEase(i_AnimData.FinalSizeDeltaEase))
                .AppendCallback(() =>
                {
                    m_RectTransform.DOSizeDelta(Vector2.one * i_TargetTransform.rect.size, i_AnimData.FinalAnimData.Duration)
                        .SetEase(i_AnimData.FinalAnimData.Ease);
                })
                .Append(m_RectTransform.DOMove(i_TargetTransform.position, i_AnimData.FinalAnimData.Duration)
                    .SetEase(i_AnimData.FinalAnimData.Ease));

            m_AnimSequence
                .OnComplete(OnAnimComplete);
        }

        private void animateBurst(RectTransform i_OriginalTransform, RectTransform i_TargetTransform, EarnObjectUIAnimData.BurstAnimData i_AnimData)
        {
            m_AnimSequence?.Kill();

            m_AnimSequence = DOTween.Sequence();

            float angle = !i_AnimData.IsLimitAngle ? UnityEngine.Random.Range(0, 360f) : UnityEngine.Random.Range(i_AnimData.AngleLimitMin, i_AnimData.AngleLimitMax);
            float radius = UnityEngine.Random.Range(i_AnimData.BurstRadius * (1f - i_AnimData.RadiusThickness), i_AnimData.BurstRadius);

            m_AnimSequence
                .AppendCallback(() =>
                {
                    m_RectTransform.DOSizeDelta(Vector2.one * i_AnimData.BurstSizeDeltaAmount, i_AnimData.BurstDuration)
                        .SetEase(i_AnimData.BurstSizeDeltaEase);
                })
                .Append(m_RectTransform.DOAnchorPos((Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.right * radius) + i_AnimData.BurstOffset, i_AnimData.BurstDuration)
                    .SetEase(i_AnimData.BurstMoveEase)
                    .SetRelative(true))
                .Append(m_RectTransform.DOSizeDelta(Vector2.one * i_AnimData.WaitSizeDeltaAmount, i_AnimData.WaitSizeDeltaDuration)
                    .SetEase(i_AnimData.WaitSizeDeltaEase))
                .AppendCallback(()=>
                {
                    m_RectTransform.DOSizeDelta(Vector2.one * i_TargetTransform.rect.size, i_AnimData.FinalAnimData.Duration)
                        .SetEase(i_AnimData.FinalAnimData.Ease);
                })
                .Append(m_RectTransform.DOMove(i_TargetTransform.position, i_AnimData.FinalAnimData.Duration)
                    .SetEase(i_AnimData.FinalAnimData.Ease));

            m_AnimSequence
                .OnComplete(OnAnimComplete);
        }

        protected virtual void OnAnimComplete() 
        {
            m_MoveCompleteCallback?.Invoke(this);
        }

        private void copyTarget(RectTransform i_OriginalTransform)
        {
            transform.position = i_OriginalTransform.position;
            m_RectTransform.sizeDelta = i_OriginalTransform.sizeDelta;
            transform.localScale = i_OriginalTransform.localScale;
            m_RectTransform.sizeDelta = i_OriginalTransform.sizeDelta;
        }
        #endregion

        #region Editor
        [Button]
        protected virtual void SetRefs()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }
        #endregion
    }
}