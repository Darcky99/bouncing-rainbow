using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections;

namespace KobGamesSDKSlim.Collectable
{
    public class CollectableUpdater : CollectableContainer
    {
        [Title("Parameters")]
        [SerializeField] private eCollectableType m_CollectableType;
        [SerializeField] private eCollectableReceiveAnimType m_ReceiverType = eCollectableReceiveAnimType.Default;
        [SerializeField] private bool m_IsOverrideAnimData;
        [SerializeField, ShowIf(nameof(m_IsOverrideAnimData))] private AnimationData.ReceiveAnimData m_OverrideAnimData;

        public eCollectableType CollectableType => m_CollectableType;

        private AnimationData m_AnimationData => GameConfig.Instance.HUD.AnimationData;

        private AnimationData.ReceiveAnimDataDictionary m_AnimDataDictionary => m_AnimationData.ReceiveAnim;
        private AnimationData.ReceiveAnimData m_AnimData => m_AnimDataDictionary.ContainsKey(m_ReceiverType)
            ? m_AnimDataDictionary[m_ReceiverType]
            : m_AnimDataDictionary[eCollectableReceiveAnimType.Default];


        protected override void OnEnable()
        {
            base.OnEnable();

            StorageManager.OnCollectableAmountChanged += collectableValueChanged;

            var collectableValue = StorageManager.Instance.GetCollectable(CollectableType);

            SetCollectableValue(collectableValue, false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StorageManager.OnCollectableAmountChanged -= collectableValueChanged;
        }

        private Coroutine m_RecieveCoroutine;
        private void collectableValueChanged(eCollectableType i_CollectableType, int i_EarnAmount, int i_ResultAmount)
        {
            if (i_CollectableType == m_CollectableType)
            {
                SetCollectableValue(i_ResultAmount, m_AnimData.IsPunchIconScale);

                switch (m_AnimData.ShowMode)
                {
                    case AnimationData.ReceiveAnimData.eShowMode.OnIncreaseAmount:
                        if (i_EarnAmount < 0) i_EarnAmount = 0;
                        break;
                    case AnimationData.ReceiveAnimData.eShowMode.OnDecreaseAmount:
                        if (i_EarnAmount > 0) i_EarnAmount = 0; 
                        break;
                    case AnimationData.ReceiveAnimData.eShowMode.Both:
                        break;
                    case AnimationData.ReceiveAnimData.eShowMode.None:
                        i_EarnAmount = 0;
                        break;
                    default:
                        break;
                }

                if(i_EarnAmount > 0)
                {
                    if (m_RecieveCoroutine.HasValue()) StopCoroutine(m_RecieveCoroutine);

                    m_RecieveCoroutine = StartCoroutine(receiveCoroutine(m_MoveTarget, m_IsOverrideAnimData ? m_OverrideAnimData : m_AnimData, Mathf.Abs(i_EarnAmount), i_EarnAmount > 0));
                }
            }
        }

        private IEnumerator receiveCoroutine(RectTransform i_SpawnRectTransform,
            AnimationData.ReceiveAnimData i_AnimData, int i_ReceiveAmount, bool i_IsIncrease)
        {
            AnimationData.ReceiveAnimData.BaseAnimData animData = null;
            switch (i_AnimData.AnimMode)
            {
                case AnimationData.ReceiveAnimData.eAnimMode.Burst:
                    animData = i_IsIncrease ? i_AnimData.IncreaseAnimMode : i_AnimData.DecreaseAnimMode;
                    break;
                default:
                    break;
            }

            var sendAmount = Mathf.Clamp(i_ReceiveAmount / animData.ItemCost, 0, animData.MaxAmount);

            for (int i = 0; i < sendAmount; i++)
            {
                var collectable = PoolManager.Instance.Dequeue(ePoolType.CollectableUI).GetComponent<CollectableUI>();

                collectable.transform.SetParent(i_SpawnRectTransform);
                collectable.Initialize(i_SpawnRectTransform, m_CollectableType);
                collectable.Receive(m_AnimData.AnimMode, animData);

                var delay = animData.DelayBetween * animData.DelayCurve.Evaluate(((float)i / sendAmount));

                if (delay != 0)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }
}