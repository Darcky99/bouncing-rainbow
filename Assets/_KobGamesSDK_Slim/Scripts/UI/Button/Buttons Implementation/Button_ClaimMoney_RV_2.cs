using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using KobGamesSDKSlim.UI;
using System;
using KobGamesSDKSlim.Collectable;
using TMPro;

namespace KobGamesSDKSlim
{
    public class Button_ClaimMoney_RV_2 : MonoBehaviour
    {
        [Serializable]
        private struct MoneyToReceiveData
        {
            [SerializeField] private float m_Angle;
            [SerializeField] private int m_Multiplier;

            public float Angle => m_Angle;
            public int Multiplier => m_Multiplier;
        }
        
        private int m_MoneyToReceive = 90;
        private int m_MoneyMultiplier = 2;
        private int m_MoneyToReceivedRV => m_MoneyToReceive * m_MoneyMultiplier;


        [SerializeField] private MoneyToReceiveData[] m_MultiplierData;

        private int        m_LastMultiplierPick = 0;
        private Quaternion m_LastPickedAngle;

        [SerializeField, ReadOnly] private ExtendedButton_RV m_ClaimRV;
        [SerializeField, ReadOnly] private CollectableSender m_MultipliedCoinSender;
        [SerializeField, ReadOnly] private CoinSender        m_MultipliedCoinSenderLegacy;

        [SerializeField, ReadOnly] private RectTransform m_RewardArrowTransform;
        [SerializeField, ReadOnly] private Rotator       m_RewardArrowRotator;

        [SerializeField, ReadOnly] private Scaler m_CoinGroupScaler;

        [SerializeField] private bool m_ResetEventsOnDisable = true;

        private Action<eRewardVideoCallResult> m_OnRewardVideoCallback;
        private Action m_OnClaimSuccess;
        private Action m_OnClaimFailed;
        private Action m_OnClaimCompleted;


        protected MenuVariablesEditor m_MenuVars => GameConfig.Instance.Menus;


        [Button]
        private void SetRefs()
        {
            m_ClaimRV                    = transform.GetComponentInChildren<ExtendedButton_RV>();
            m_MultipliedCoinSender       = m_ClaimRV.GetComponent<CollectableSender>();
            m_MultipliedCoinSenderLegacy = m_ClaimRV.GetComponent<CoinSender>();

            m_RewardArrowTransform = transform.FindDeepChild<RectTransform>("Reward Arrow");
            m_RewardArrowRotator   = m_RewardArrowTransform.GetComponent<Rotator>();

            m_CoinGroupScaler = transform.FindDeepChild<Scaler>("Coin Group Reward");
        }

        private void OnEnable()
        {
            m_LastMultiplierPick = 0;
            m_MoneyMultiplier = 2;

            m_RewardArrowRotator.Duration = m_MenuVars.RV_NeedleRotationDuration;
            m_RewardArrowRotator.Ease = m_MenuVars.RV_NeedleRotationEase;

            m_ClaimRV.Setup(RewardVideoButtonCallback, ClaimRewardSuccess, ClaimRewardFailed, RewardVideoPlacementIDs.LevelCompleted_ClaimRV, OnClaimInteractivityChanged);

            if (m_ClaimRV.interactable)
                m_RewardArrowRotator.StartAnimation();

            UpdateMoneyToReceivedRV();
        }

        private void OnDisable()
        {
            if (m_ResetEventsOnDisable)
                ResetEvents();
        }

        public void Set(int i_CoinAmount, Action<eRewardVideoCallResult> i_RewardVideoCallResult, Action i_OnClaimSuccess, Action i_OnClaimFailed, Action i_OnClaimCompleted)
        {
            SetCoins(i_CoinAmount);
            SetEvents(i_RewardVideoCallResult, i_OnClaimSuccess, i_OnClaimFailed, i_OnClaimCompleted);
        }

        private void SetCoins(int i_Value)
        {
            m_MoneyToReceive = i_Value;
            UpdateMoneyToReceivedRV();
        }

        #region Events
        private void SetEvents(Action<eRewardVideoCallResult> i_RewardVideoCallResult, Action i_OnClaimSuccess, Action i_OnClaimFailed, Action i_OnClaimCompleted)
        {
            m_OnRewardVideoCallback += i_RewardVideoCallResult;
            m_OnClaimSuccess += i_OnClaimSuccess;
            m_OnClaimFailed += i_OnClaimFailed;
            m_OnClaimCompleted += i_OnClaimCompleted;
        }

        public void ResetEvents()
        {
            m_OnRewardVideoCallback = null;
            m_OnClaimSuccess = m_OnClaimFailed = m_OnClaimCompleted = null;
        }
        #endregion

        private void Update()
        {
            if ((m_ClaimRV.interactable && m_ClaimRV.DisableVisualOnClick) || !m_ClaimRV.DisableVisualOnClick)
            {
                CalculateMoneyToReceiveRV(true);
            }
        }

        private void CalculateMoneyToReceiveRV(bool i_Animate)
        {
            float angleToEvaluate = m_RewardArrowTransform.localEulerAngles.z;
            if (angleToEvaluate > 180)
                angleToEvaluate = -(360 - angleToEvaluate);

            for (int i = 0; i < m_MultiplierData.Length - 1; i++)
            {
                if (angleToEvaluate.IsBetweenInclusive(m_MultiplierData[i + 1].Angle, m_MultiplierData[i].Angle))
                {
                    if (m_LastMultiplierPick != i)
                    {
                        m_LastMultiplierPick = i;
                        m_MoneyMultiplier    = m_MultiplierData[i].Multiplier;
                        UpdateMoneyToReceivedRV();
                        m_CoinGroupScaler.StartAnimation();
                    }

                    break;
                }
            }
        }

        private void UpdateMoneyToReceivedRV()
        {
            if(GameConfig.Instance.HUD.IsUseCollectables)
            {
                m_MultipliedCoinSender.Set(m_MoneyToReceivedRV);
            }
            else
            {
                m_MultipliedCoinSenderLegacy.SetCoins(m_MoneyToReceivedRV);
            }
        }

        private void RewardVideoButtonCallback(eRewardVideoCallResult i_Result)
        {
            if (i_Result == eRewardVideoCallResult.Success)
            {
                m_RewardArrowRotator.PauseAnimation();
                m_LastPickedAngle = m_RewardArrowTransform.rotation;
            }

            m_OnRewardVideoCallback.InvokeSafe(i_Result);
        }

        private void ClaimRewardSuccess()
        {
            m_RewardArrowRotator.PauseAnimation();
            m_RewardArrowTransform.rotation = m_LastPickedAngle;


            CalculateMoneyToReceiveRV(false);

            if (GameConfig.Instance.HUD.IsUseCollectables)
            {
                m_MultipliedCoinSender.Send(m_OnClaimCompleted);
            }
            else
            {
                m_MultipliedCoinSenderLegacy.SendCoins(m_OnClaimCompleted);
            }

            m_OnClaimSuccess.InvokeSafe();
        }

        private void ClaimRewardFailed()
        {
            if (m_ClaimRV.interactable)
                m_RewardArrowRotator.ContinueAnimation();
            else
            {
                m_LastMultiplierPick = 0;
                m_MoneyMultiplier = 2;
                UpdateMoneyToReceivedRV();
                m_RewardArrowRotator.ResetValues();
            }

            m_OnClaimFailed.InvokeSafe();
        }

        private void OnClaimInteractivityChanged(bool i_Value, eButtonInteractivityChangeReason i_Reason)
        {
            if (i_Value)
            {
                m_RewardArrowRotator.RestartAnimation();
            }
            else
            {
                m_RewardArrowRotator.PauseAnimation();
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < m_MultiplierData.Length; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(m_RewardArrowTransform.transform.position, Quaternion.Euler(0, 0, m_MultiplierData[i].Angle) * Vector3.up * 1000);
            }
        }
    }
}