using System;
using Crosstales.OnlineCheck;
using UnityEngine;
using KobGamesSDKSlim.Animation;
using KobGamesSDKSlim.MenuManagerV1;

namespace KobGamesSDKSlim
{
    public class ExtendedButton_RV : ExtendedButton_FadeAnim
    {
        public bool DisableButtonAfterAdShown = true;
        public bool DisableVisualOnClick      = true;
        public bool OpenConfirmationScreen = false;
        
        private bool m_IsShowingAd                   = false;
        private bool m_IsAutoInteractivityEventSetup = false;
        
        

        protected override void OnEnableImpl()
        {
            OnlineCheck.Instance.OnOnlineStatusChange += onOnlineStatusChange;
        }


        protected override void OnDisableImpl()
        {
            OnlineCheck.Instance.OnOnlineStatusChange -= onOnlineStatusChange;
        }

        /// <summary>
        /// Setting up this button will automatically apply and switch interactivity based on the Reward Video availability.
        /// Note - button automatically unregisters the events on disable so no need to do that yourself.
        /// </summary>
        /// <param name="i_RewardVideoCallResult">Return Success or appropriate Error when trying to call the Reward Ad</param>
        /// <param name="i_CloseWithSuccessCallback">Triggered when closing the Reward Video. Player will receive the reward</param>
        /// <param name="i_CloseWithFailCallback">Triggered when closing the Reward Video. Player will not receive the reward</param>
        /// <param name="i_PlacementName">Used for Analytics. ID should be unique</param>
        public void Setup(Action<eRewardVideoCallResult> i_RewardVideoCallResult    = null,
                          Action                         i_CloseWithSuccessCallback = null,
                          Action                         i_CloseWithFailCallback    = null,
                          string                         i_PlacementID              = "None",
                          InteractivityChangedCallback   i_OnInteractivityChanged   = null)
        {
            if (m_IsSetup)
            {
                Debug.LogError("Button already Setup. Skiping!!!");
                return;
            }


            setup(true, i_RewardVideoCallResult, i_CloseWithSuccessCallback, i_CloseWithFailCallback, i_PlacementID, i_OnInteractivityChanged, OpenConfirmationScreen);
        }

        /// <summary>
        /// Setting up this button will automatically apply and switch interactivity based on the Reward Video availability.
        /// It will setup a fade animation according to TweenData
        /// Note - button automatically unregisters the events on disable so no need to do that yourself.
        /// </summary>
        /// <param name="i_TweenData"></param>
        /// <param name="i_RewardVideoCallResult"></param>
        /// <param name="i_CloseWithSuccessCallback"></param>
        /// <param name="i_CloseWithFailCallback"></param>
        /// <param name="i_PlacementID"></param>
        public void Setup(TweenData                      i_TweenData,
                          Action<eRewardVideoCallResult> i_RewardVideoCallResult    = null,
                          Action                         i_CloseWithSuccessCallback = null,
                          Action                         i_CloseWithFailCallback    = null,
                          string                         i_PlacementID              = "None",
                          InteractivityChangedCallback   i_OnInteractivityChanged   = null) 
            => Setup(i_TweenData, i_RewardVideoCallResult, i_CloseWithSuccessCallback, i_CloseWithFailCallback, i_PlacementID, i_OnInteractivityChanged, OpenConfirmationScreen);

        /// <summary>
        /// Setting up this button will automatically apply and switch interactivity based on the Reward Video availability.
        /// It will setup a fade animation according to TweenData
        /// Note - button automatically unregisters the events on disable so no need to do that yourself.
        /// </summary>
        /// <param name="i_TweenData"></param>
        /// <param name="i_RewardVideoCallResult"></param>
        /// <param name="i_CloseWithSuccessCallback"></param>
        /// <param name="i_CloseWithFailCallback"></param>
        /// <param name="i_PlacementID"></param>
        public void Setup(TweenData                      i_TweenData,
                          Action<eRewardVideoCallResult> i_RewardVideoCallResult    = null,
                          Action                         i_CloseWithSuccessCallback = null,
                          Action                         i_CloseWithFailCallback    = null,
                          string                         i_PlacementID              = "None",
                          InteractivityChangedCallback   i_OnInteractivityChanged   = null,
                          bool                           i_OpenRVConfirmationScreen = false)
        {
            if (m_IsSetup)
            {
                Debug.LogError("Button already Setup. Skiping!!!");
                return;
            }

            base.Setup(i_TweenData);
            setup(false, i_RewardVideoCallResult, i_CloseWithSuccessCallback, i_CloseWithFailCallback, i_PlacementID, i_OnInteractivityChanged, i_OpenRVConfirmationScreen);
        }


    private void setup(bool i_SetOnRewardVideoAvailabilityChange,
            Action<eRewardVideoCallResult> i_RewardVideoCallResult = null,
                                    Action i_CloseWithSuccessCallback = null,
                                    Action i_CloseWithFailCallback = null,
                                    string i_PlacementID = "None",
                                    InteractivityChangedCallback i_OnInteractivityChanged = null,
                                    bool i_OpenRVConfirmationScreen = false)
        {
            m_IsSetup = true;
            m_IsShowingAd = false;

            //This is necessary in case we are setting up a fade button
            if (i_SetOnRewardVideoAvailabilityChange)
            {
                setAutoInteractivityEvent(true);

                setInteractable(AdsManager.Instance.IsRewardVideoLoaded, eButtonInteractivityChangeReason.RV_Availability);
            }

            onClick.AddListener(() =>
                                {
                                    if (i_OpenRVConfirmationScreen)
                                    {
                                        MenuManager.Instance.GetMenuScreen<Screen_RVConfirmation>().Open(onShow, i_CloseWithFailCallback);
                                        i_RewardVideoCallResult.InvokeSafe(eRewardVideoCallResult.OpenedRVConfirmationScreen);
                                    }
                                    else
                                        onShow();
                                    
                                    void onShow()
                                    {
                                        eRewardVideoCallResult result = AdsManager.Instance.ShowRewardVideo(() =>
                                                                                                            {
                                                                                                                m_IsShowingAd = false;
                                                                                                                i_CloseWithSuccessCallback.InvokeSafe();
                                                                                                                if (DisableButtonAfterAdShown) setAutoInteractivity(false, eButtonInteractivityChangeReason.Manual);
                                                                                                                //ResetState();
                                                                                                            }, () =>
                                                                                                               {
                                                                                                                   m_IsShowingAd = false;
                                                                                                                   i_CloseWithFailCallback.InvokeSafe();
                                                                                                                   setInteractable(AdsManager.Instance.IsRewardVideoLoaded, eButtonInteractivityChangeReason.RV_Availability);
                                                                                                               }, i_PlacementID.ToString());


                                        m_IsShowingAd = result == eRewardVideoCallResult.Success;
                                        i_RewardVideoCallResult.InvokeSafe(result);
                                        setInteractable(false);
                                    }
                                });

            OnInteractivityChanged += i_OnInteractivityChanged;
        }

        protected override void OnFadeAnimDelayCallback()
        {
            setAutoInteractivityEvent(true);
            setInteractable(AdsManager.Instance.IsRewardVideoLoaded, eButtonInteractivityChangeReason.RV_Availability);
        }

        

        

        public override void ResetState()
        {
            setAutoInteractivityEvent(false);

            base.ResetState();
        }


        protected virtual void onRewardVideoAvailabilityChange(bool i_Value) => setInteractable(i_Value, eButtonInteractivityChangeReason.RV_Availability);

        private void onOnlineStatusChange(bool i_IsConnected)
        {
            if(!i_IsConnected)
                setInteractable(false, eButtonInteractivityChangeReason.RV_Availability);
        }

        //Overriding original func so people won't manually set interactivity and always rely on Ads availability
        private void setInteractable(bool i_Value)
        {
            setInteractable(i_Value, eButtonInteractivityChangeReason.Manual);
        }
        private void setInteractable(bool i_Value, eButtonInteractivityChangeReason i_Reason)
        {
            base.SetInteractable(i_Value, i_Reason);

            if (i_Reason == eButtonInteractivityChangeReason.Manual || m_IsShowingAd)
            {
                if (!DisableVisualOnClick)
                    SetAssetsState(eButtonState.Normal);
            }
        }

        /// <summary>
        /// Set Interactable is more of a suggestion. If AdsManager doesn't have any ads available it will keep off until it does
        /// </summary>
        /// <param name="i_Value"></param>
        public override void SetInteractable(bool i_Value)
        {
            setAutoInteractivity(i_Value, eButtonInteractivityChangeReason.RV_Availability);
        }
        /// <summary>
        /// Set Interactable is more of a suggestion. If AdsManager doesn't have any ads available it will keep off until it does
        /// </summary>
        /// <param name="i_Value"></param>
        /// <param name="i_Reason">Manual will keep it visually enabled even after it is set to Disabled. Rv_Availability will match visual and interactivity</param>
        public override void SetInteractable(bool i_Value, eButtonInteractivityChangeReason i_Reason)
        {
            setAutoInteractivity(i_Value, i_Reason);
        }
        //_______________________________________________________________________________________________________


        private  void setAutoInteractivity(bool i_Value, eButtonInteractivityChangeReason i_Reason)
        {
            setAutoInteractivityEvent(i_Value);

            setInteractable(i_Value ? AdsManager.Instance.IsRewardVideoLoaded : false, i_Reason);
        }

        private void setAutoInteractivityEvent(bool i_Value)
        {
            if (m_IsAutoInteractivityEventSetup == i_Value)
                return;

            m_IsAutoInteractivityEventSetup = i_Value;

            if (i_Value)
                AdsManager.OnRewardVideoAvailabilityChange += onRewardVideoAvailabilityChange;
            else
                AdsManager.OnRewardVideoAvailabilityChange -= onRewardVideoAvailabilityChange;
        }
    }
}
