using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using KobGamesSDKSlim.Animation;
using System.Collections;
using MoreMountains.NiceVibrations;

namespace KobGamesSDKSlim.UI
{
    public class CoinSender : MonoBehaviour
    {
        private HUDVariablesEditorBase.EarnObjectData m_EarnParameters => GameConfig.Instance.HUD.EarnObject;

        [Title("Refs")]
        [SerializeField, ReadOnly] private TextMeshProUGUI m_CoinText;
        [SerializeField, ReadOnly] private RectTransform m_CoinIcon;

        [Title("Parameters")]
        [SerializeField] private bool m_IsUseTextFormat;
        [SerializeField] private bool m_IsOverrideTarget;
        [SerializeField, ShowIf(nameof(m_IsOverrideTarget))] private CoinUpdater m_CoinsTarget;

        [SerializeField] private eCoinSenderType m_CoinSenderType;
        [SerializeField, ShowIf(nameof(m_IsOverrideAnimData))] private EarnObjectUIAnimData m_EarnObjectUIAnimData;

        [SerializeField] private bool m_IsDecreaseSendAmount;

        private bool m_IsOverrideAnimData => m_CoinSenderType == eCoinSenderType.Override;

        private int m_CoinsValue;
        private int m_ReceivedCoinsValue;
        private int m_SentCoinsValue;

        private EarnCoinUI m_EarnCoinUIDummy;

        private bool m_IsHapticsCooldown;

        private System.Action m_SendCompleteCallback;

        private string m_TextFormat;

        public void SetCoins(int i_CoinsAmount)
        {
            m_CoinsValue = i_CoinsAmount;

            updateCoinValueText(m_CoinsValue);
        }

        private void updateCoinValueText(int i_CoinValue)
        {
            if(m_IsUseTextFormat)
            {
                if (m_TextFormat == null)
                {
                    m_TextFormat = m_CoinText.text;
                }

                m_CoinText.SetText(string.Format(m_TextFormat, i_CoinValue));
            }
            else
            {
                m_CoinText.SetText(i_CoinValue.ToString());
            }
        }

        public void SendCoins(System.Action i_CompleteCallback = null)
        {
            EarnObjectUIAnimData earnAnimData = null;

            if (m_CoinSenderType == eCoinSenderType.Override)
            {
                earnAnimData = m_EarnObjectUIAnimData;
            }
            else
            {
                earnAnimData = GameConfig.Instance.HUD.EarnObject.EarnObjectUIAnimData.ContainsKey(m_CoinSenderType)
                    ? GameConfig.Instance.HUD.EarnObject.EarnObjectUIAnimData[m_CoinSenderType]
                    : GameConfig.Instance.HUD.EarnObject.EarnObjectUIAnimData[eCoinSenderType.Default];
            }

            var target = (m_IsOverrideTarget && m_CoinsTarget != null) ? m_CoinsTarget.CoinMoveTarget : HUDManager.Instance.DefaultCoinsTarget;

            m_SendCompleteCallback = i_CompleteCallback;

            StartCoroutine(sendCoinsCoroutine(m_CoinIcon, target, earnAnimData));
        }

        private IEnumerator sendCoinsCoroutine(RectTransform i_SpawnRectTransform, RectTransform i_TargetRectTransform,
            EarnObjectUIAnimData i_EarnAnimData)
        {
            EarnObjectUIAnimData.AnimData animData = null;
            switch (i_EarnAnimData.SpawnMode)
            {
                case eEarnObjectSpawnMode.Linear:
                    animData = i_EarnAnimData.LinearMode;
                    break;
                case eEarnObjectSpawnMode.Burst:
                    animData = i_EarnAnimData.BurstMode;
                    break;
                default:
                    break;
            }

            var coinCost = m_CoinsValue / animData.CoinsNumber;
            var coinCostRest = m_CoinsValue % animData.CoinsNumber;

            m_SentCoinsValue = 0;
            m_ReceivedCoinsValue = 0;

            for (int i = 0; i < animData.CoinsNumber; i++)
            {
                m_EarnCoinUIDummy = PoolManager.Instance.Dequeue(ePoolType.CoinUI).GetComponent<EarnCoinUI>();

                m_EarnCoinUIDummy.transform.SetParent(i_TargetRectTransform);
                m_EarnCoinUIDummy.SetCoinCost(coinCost + ((i == (animData.CoinsNumber - 1)) ? coinCostRest : 0));
                m_EarnCoinUIDummy.Animate(i_SpawnRectTransform, i_TargetRectTransform, i_EarnAnimData.SpawnMode, animData, coinMoveComplete);

                var delay = animData.DelayBetweenCoins * animData.DelayBetweenCoinsCurve.Evaluate(((float)i / animData.CoinsNumber));

                m_SentCoinsValue += m_EarnCoinUIDummy.CoinCost;
                if (m_IsDecreaseSendAmount)
                {
                    updateCoinValueText(m_CoinsValue - m_SentCoinsValue);
                }

                yield return new WaitForSeconds(delay);
            }
        }


        private void coinMoveComplete(EarnObjectUI i_SentCoin)
        {
            m_ReceivedCoinsValue += (i_SentCoin as EarnCoinUI).CoinCost;

            if (m_ReceivedCoinsValue == m_CoinsValue)
            {
                m_SendCompleteCallback?.Invoke();

                if (m_EarnParameters.IsPlayHapticsOnReceive) playHaptics(m_EarnParameters.EarningCompleteHaptics);
            }
            else
            {
                if (m_EarnParameters.IsPlayHapticsOnReceive) playHaptics(m_EarnParameters.ReceivedHaptics);
            }
        }

        private void playHaptics(HapticTypes i_HapticsType)
        {
            if (m_IsHapticsCooldown) return;
            m_IsHapticsCooldown = true;

            Managers.Instance.HapticManager.Haptic(i_HapticsType);

            Invoke(nameof(resetHapticsCooldown), m_EarnParameters.HapticsCooldown);
        }

        private void resetHapticsCooldown()
        {
            m_IsHapticsCooldown = false;
        }

        [Button]
        private void SendCoinsTest()
        {
            SendCoins();
        }

        [Button]
        private void SetRefs()
        {
            m_CoinText = transform.FindDeepChild<TextMeshProUGUI>("Coin Text");
            m_CoinIcon = transform.FindDeepChild<RectTransform>("Coin Icon");
        }
    }
}