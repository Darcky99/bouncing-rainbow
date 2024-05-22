using KobGamesSDKSlim;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim.Collectable
{
    public class UniversalCollectableSender : MonoBehaviour
    {
        private AnimationData m_AnimationData => GameConfig.Instance.HUD.AnimationData;

        [Title("Refs")]
        [SerializeField, ReadOnly] private RectTransform m_RectTransform;

        private System.Action m_SendCompleteCallback;

        private int m_CollectableAmount;
        private int m_ReceivedCollectableAmount;
        private int m_SentCollectableAmount;

        #region Send
        public void Send(eCollectableType i_CollectableType, int i_CollectableAmount, eCollectableSendAnimType i_AnimType, Vector2 i_ScreenPosition, System.Action i_CompleteCallback = null)
        {
            m_CollectableAmount = i_CollectableAmount;
            m_SendCompleteCallback = i_CompleteCallback;

            var target = CollectableManager.Instance.DefaultCollectableTarget(i_CollectableType).MoveTarget;
            var animData = m_AnimationData.SendAnim.ContainsKey(i_AnimType)
                ? m_AnimationData.SendAnim[i_AnimType]
                : m_AnimationData.SendAnim[eCollectableSendAnimType.Default];

            StartCoroutine(sendsCoroutine(i_CollectableType, i_CollectableAmount, i_AnimType, i_ScreenPosition, target, animData));
        }

        

        private IEnumerator sendsCoroutine(eCollectableType i_CollectableType, int i_CollectableAmount, eCollectableSendAnimType i_AnimType, Vector2 i_ScreenPosition, RectTransform i_TargetRectTransform,
            AnimationData.SendAnimData i_AnimData)
        {
            AnimationData.SendAnimData.BaseAnimData animData = null;
            switch (i_AnimData.AnimMode)
            {
                case AnimationData.SendAnimData.eAnimMode.Linear:
                    animData = i_AnimData.LinearAnimMode;
                    break;
                case AnimationData.SendAnimData.eAnimMode.Burst:
                    animData = i_AnimData.BurstAnimMode;
                    break;
                default:
                    break;
            }

            m_RectTransform.anchoredPosition = i_ScreenPosition / CollectableManager.Instance.HUDCanvas.scaleFactor;

            var sendAmount = animData.IsSpecificAmount ? animData.SendAmount : Mathf.Min(m_CollectableAmount / animData.ItemCost, animData.MaxAmount);

            var collectableValue = 0;
            var collectableValueRest = 0;

            if (sendAmount == 0)
            {
                if (!animData.IsSpecificAmount)
                {
                    sendAmount = 1;

                    collectableValue = m_CollectableAmount;
                    collectableValueRest = 0;
                }
            }
            else
            {
                collectableValue = m_CollectableAmount / sendAmount;
                collectableValueRest = m_CollectableAmount % sendAmount;
            }

            m_SentCollectableAmount = 0;
            m_ReceivedCollectableAmount = 0;

            for (int i = 0; i < sendAmount; i++)
            {
                var collectable = PoolManager.Instance.Dequeue(ePoolType.CollectableUI).GetComponent<CollectableUI>();

                collectable.transform.SetParent(i_TargetRectTransform);
                collectable.Value = collectableValue + ((i == (sendAmount - 1)) ? collectableValueRest : 0);
                collectable.Initialize(m_RectTransform, i_CollectableType);
                collectable.Send(i_TargetRectTransform, i_AnimData.AnimMode, animData, collectableMoveComplete);

                var delay = animData.DelayBetween * animData.DelayCurve.Evaluate(((float)i / sendAmount));

                m_SentCollectableAmount += collectable.Value;

                if (delay != 0)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        private void collectableMoveComplete(CollectableUI i_SentCollectable)
        {
            m_ReceivedCollectableAmount += i_SentCollectable.Value;

            if (m_ReceivedCollectableAmount == m_CollectableAmount)
            {
                m_SendCompleteCallback?.Invoke();

                if (m_AnimationData.Haptics.IsPlayHapticsOnReceive) playHaptics(m_AnimationData.Haptics.CompleteHaptics, m_AnimationData.Haptics.HapticsCooldown);
            }
            else
            {
                if (m_AnimationData.Haptics.IsPlayHapticsOnReceive) playHaptics(m_AnimationData.Haptics.ReceivedHaptics, m_AnimationData.Haptics.HapticsCooldown);
            }
        }
        #endregion

        #region Haptics
        private bool m_IsHapticsCooldown;

        private void playHaptics(HapticTypes i_HapticsType, float i_HapticsCooldown)
        {
            if (m_IsHapticsCooldown) return;
            m_IsHapticsCooldown = true;

            Managers.Instance.HapticManager.Haptic(i_HapticsType);

            Invoke(nameof(resetHapticsCooldown), i_HapticsCooldown);

        }

        private void resetHapticsCooldown()
        {
            m_IsHapticsCooldown = false;
        }
        #endregion

        #region Editor
        [Button]
        private void SetRefs()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }
        #endregion
    }
}
