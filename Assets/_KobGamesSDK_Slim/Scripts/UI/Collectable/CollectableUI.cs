using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace KobGamesSDKSlim.Collectable
{
    public class CollectableUI : MonoBehaviour
    {
        [Title("Refs")]
        [SerializeField, ReadOnly] private RectTransform m_RectTransform;
        [SerializeField, ReadOnly] private Image m_Image;

        [Title("Info")]
        [ReadOnly, ShowInInspector] private eCollectableType m_CollectableType;

        public virtual eCollectableType CollectableType => m_CollectableType;

        public int Value { get; set; }

        private System.Action<CollectableUI> m_MoveCompleteCallback;

        private Sequence m_AnimSequence;

        protected virtual void OnEnable() {}

        protected virtual void OnDisable() {}

        #region Initialize
        public void Initialize(RectTransform i_OriginalTransform, eCollectableType i_Type)
        {
            m_AnimSequence?.Kill();
            m_RectTransform.DOKill();
            m_Image.DOKill();

            transform.position = i_OriginalTransform.position;
            //m_RectTransform.sizeDelta = i_OriginalTransform.sizeDelta;
            m_RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, i_OriginalTransform.rect.width);
            m_RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, i_OriginalTransform.rect.height);
            transform.localScale = i_OriginalTransform.localScale;

            //Resets values
            m_RectTransform.localRotation = Quaternion.identity;
            m_RectTransform.localScale = Vector3.one;
            m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 1f);

            setType(i_Type);
        }

        private void setType(eCollectableType i_Type)
        {
            m_CollectableType = i_Type;

            m_Image.sprite = GameConfig.Instance.HUD.CollectableData[m_CollectableType].CollectableSprite;
        }
        #endregion

        #region Send
        public void Send(RectTransform i_TargetTransform, AnimationData.SendAnimData.eAnimMode i_AnimMode, AnimationData.SendAnimData.BaseAnimData i_AnimData, 
            System.Action<CollectableUI> i_CompleteMoveCallback)
        {
            m_MoveCompleteCallback = i_CompleteMoveCallback;

            switch (i_AnimMode)
            {
                case AnimationData.SendAnimData.eAnimMode.Linear:
                    sendLinear(i_TargetTransform, i_AnimData as AnimationData.SendAnimData.LinearAnimData);
                    break;
                case AnimationData.SendAnimData.eAnimMode.Burst:
                    sendBurst(i_TargetTransform, i_AnimData as AnimationData.SendAnimData.BurstAnimData);
                    break;
                default:
                    break;
            }
        }

        private void sendLinear(RectTransform i_TargetTransform, AnimationData.SendAnimData.LinearAnimData i_AnimData)
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
                .OnComplete(OnSendComplete);
        }

        private void sendBurst(RectTransform i_TargetTransform, AnimationData.SendAnimData.BurstAnimData i_AnimData)
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
                .OnComplete(OnSendComplete);
        }

        protected virtual void OnSendComplete() 
        {
            StorageManager.Instance.SetCollectable(CollectableType, StorageManager.Instance.GetCollectable(CollectableType) + Value);

            PoolManager.Instance.Queue(ePoolType.CollectableUI, gameObject);

            m_MoveCompleteCallback?.Invoke(this);
        }
        #endregion

        #region Recieve 
        public void Receive(AnimationData.ReceiveAnimData.eAnimMode i_AnimMode, AnimationData.ReceiveAnimData.BaseAnimData i_AnimData)
        {
            switch (i_AnimMode)
            {
                case AnimationData.ReceiveAnimData.eAnimMode.Burst:
                    receiveBurst(i_AnimData as AnimationData.ReceiveAnimData.BurstAnimData);
                    break;
                default:
                    break;
            }
        }

        private void receiveBurst(AnimationData.ReceiveAnimData.BurstAnimData i_AnimData)
        {
            m_AnimSequence?.Kill();

            m_AnimSequence = DOTween.Sequence();

            float angle = !i_AnimData.IsLimitAngle ? UnityEngine.Random.Range(0, 360f) : UnityEngine.Random.Range(i_AnimData.AngleLimitMin, i_AnimData.AngleLimitMax);
            float radius = UnityEngine.Random.Range(i_AnimData.BurstRadius * (1f - i_AnimData.RadiusThickness), i_AnimData.BurstRadius);

            m_AnimSequence
                .AppendCallback(() =>
                {
                    if (i_AnimData.IsScale)
                    {
                        m_RectTransform.DOScale(Vector2.one * i_AnimData.FinalScale, i_AnimData.MoveDuration)
                            .SetEase(i_AnimData.MoveEase);
                    }
                })
                .AppendCallback(() =>
                {
                    if (i_AnimData.IsFade)
                    {
                        m_Image.DOFade(i_AnimData.FinalFadeValue, i_AnimData.MoveDuration)
                                                .SetEase(i_AnimData.FadeEase);
                    }
                })
                .AppendCallback(() =>
                {
                    if (i_AnimData.IsRotate)
                    {
                        m_Image.rectTransform.DORotate(Vector3.forward * UnityEngine.Random.Range(-i_AnimData.RotateOffsetMax, i_AnimData.RotateOffsetMax), i_AnimData.MoveDuration)
                                                .SetRelative()
                                                .SetEase(i_AnimData.FadeEase);
                    }
                })
                .Append(m_RectTransform.DOAnchorPos((Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.right * radius) + i_AnimData.BurstOffset, i_AnimData.MoveDuration)
                    .SetEase(i_AnimData.MoveEase)
                    .SetRelative(true));

            m_AnimSequence
                .OnComplete(OnReceiveComplete);
        }

        protected virtual void OnReceiveComplete()
        {
            PoolManager.Instance.Queue(ePoolType.CollectableUI, gameObject);
        }
        #endregion

        #region Editor
        [Button]
        protected virtual void SetRefs()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_Image = GetComponent<Image>();
        }
        #endregion
    }
}