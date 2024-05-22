using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using KobGamesSDKSlim.Animation;
using System.Collections;
using MoreMountains.NiceVibrations;
using System.Linq;

namespace KobGamesSDKSlim.Collectable
{
    public class CollectableSender : MonoBehaviour
    {
        private AnimationData m_AnimationData => GameConfig.Instance.HUD.AnimationData;


        [Title("Refs")]
        [SerializeField, ReadOnly] private RectTransform m_RectTransform;
        [SerializeField, ReadOnly] private TextMeshProUGUI m_CollectableText;
        [SerializeField, ReadOnly] private RectTransform m_CollectableIcon;

        [Title("Parameters")]
        [SerializeField] private eCollectableType m_CollectableType;
        [SerializeField] private eCollectableSendAnimType m_AnimType = eCollectableSendAnimType.Default;

        [SerializeField] private bool m_IsOverrideTarget;
        [SerializeField, ShowIf(nameof(m_IsOverrideTarget))] private CollectableUpdater m_OverrideTarget;

        [SerializeField] private bool m_IsOverrideAnimData;
        [SerializeField, ShowIf(nameof(m_IsOverrideAnimData))] private AnimationData.SendAnimData m_OverrideAnimData;

        [SerializeField] private bool m_IsDecreaseSendAmount;
        [SerializeField] private bool m_IsUseTextFormat;
        [SerializeField] private bool m_IsHideBigNumbers = true;

        protected virtual eCollectableType CollectableType => m_CollectableType;

        public RectTransform RectTransform => m_RectTransform;
        private AnimationData.SendAnimDataDictionary m_AnimDataDictionary => m_AnimationData.SendAnim;
        private AnimationData.SendAnimData m_AnimData => m_AnimDataDictionary.ContainsKey(m_AnimType)
            ? m_AnimDataDictionary[m_AnimType]
            : m_AnimDataDictionary[eCollectableSendAnimType.Default];


        private System.Action m_SendCompleteCallback;

        private int m_CollectableAmount;
        private int m_ReceivedCollectableAmount;
        private int m_SentCollectableAmount;

        private string m_TextFormat;

        #region Set
        public void Set(int i_CollectableAmount)
        {
            m_CollectableAmount = i_CollectableAmount;

            updateCoinValueText(m_CollectableAmount);
        }

        private void updateCoinValueText(int i_CollectableValue)
        {
            if(m_IsUseTextFormat)
            {
                if (m_TextFormat == null)
                {
                    m_TextFormat = m_CollectableText.text;
                }

                m_CollectableText.SetText(string.Format(m_TextFormat,
                    m_IsHideBigNumbers ? (object)hideBigNumber(i_CollectableValue) : i_CollectableValue));
            }
            else
            {
                m_CollectableText.SetText(i_CollectableValue.ToString());
            }
        }

        private string hideBigNumber(int i_Num)
        {
            if (i_Num >= 100000000)
            {
                return (i_Num / 1000000D).ToString("0.#M");
            }
            if (i_Num >= 1000000)
            {
                return (i_Num / 1000000D).ToString("0.##M");
            }
            if (i_Num >= 100000)
            {
                return (i_Num / 1000D).ToString("0.#k");
            }
            if (i_Num >= 10000)
            {
                return (i_Num / 1000D).ToString("0.##k");
            }

            return i_Num.ToString();
        }
        #endregion

        #region Send
        public void Send(System.Action i_CompleteCallback = null)
        {
            Send(m_CollectableIcon, i_CompleteCallback);
        }

        public void Send(RectTransform i_SpawnRectTransform, System.Action i_CompleteCallback = null)
        {
            var target = (m_IsOverrideTarget && m_OverrideTarget != null)
                ? m_OverrideTarget.MoveTarget
                : CollectableManager.Instance.DefaultCollectableTarget(CollectableType).MoveTarget;

            m_SendCompleteCallback = i_CompleteCallback;

            StartCoroutine(sendsCoroutine(i_SpawnRectTransform, target,
                m_IsOverrideAnimData ? m_OverrideAnimData : m_AnimData));
        }

        private IEnumerator sendsCoroutine(RectTransform i_SpawnRectTransform, RectTransform i_TargetRectTransform,
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
                collectable.Initialize(i_SpawnRectTransform, m_CollectableType);
                collectable.Send(i_TargetRectTransform, i_AnimData.AnimMode, animData, collectableMoveComplete);

                var delay = animData.DelayBetween * animData.DelayCurve.Evaluate(((float)i / sendAmount));

                m_SentCollectableAmount += collectable.Value;
                if (m_IsDecreaseSendAmount)
                {
                    updateCoinValueText(m_CollectableAmount - m_SentCollectableAmount);
                }

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

                if (m_AnimationData.Haptics.IsPlayHapticsOnReceive) playHaptics(m_AnimationData.Haptics.CompleteHaptics);
            }
            else
            {
                if (m_AnimationData.Haptics.IsPlayHapticsOnReceive) playHaptics(m_AnimationData.Haptics.ReceivedHaptics);
            }
        }
        #endregion

        #region Haptics
        private bool m_IsHapticsCooldown;

        private void playHaptics(HapticTypes i_HapticsType)
        {
            if (m_IsHapticsCooldown) return;
            m_IsHapticsCooldown = true;

            Managers.Instance.HapticManager.Haptic(i_HapticsType);

            Invoke(nameof(resetHapticsCooldown), m_AnimationData.Haptics.HapticsCooldown);

        }

        private void resetHapticsCooldown()
        {
            m_IsHapticsCooldown = false;
        }
        #endregion

        #region Editor
        [Button]
        private void sendTest()
        {
            if (!Application.isPlaying) return;

            Send();
        }

        [Button]
        private void SetRefs()
        {
            m_CollectableText = transform.FindDeepChild<TextMeshProUGUI>("Collectable Text");
            if (m_CollectableText == null)
            {
                //Legacy
                m_CollectableText = transform.FindDeepChild<TextMeshProUGUI>("Coin Text");
            }

            m_CollectableIcon = transform.FindDeepChild<RectTransform>("Collectable Icon");
            if (m_CollectableIcon == null)
            {
                //Legacy
                m_CollectableIcon = transform.FindDeepChild<RectTransform>("Coin Icon");
            }

            m_RectTransform = GetComponent<RectTransform>();
        }
        #endregion
    }
}

