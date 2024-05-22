using UnityEngine;

namespace KobGamesSDKSlim
{
    public class RewardVideoEvents
    {
        public delegate void RewardVideoOpen(string i_Placement);
        public delegate void RewardVideoClose(string i_Placement, bool i_IsRVSuccess);
        public delegate void RewardVideoSuccess(string i_Placement);
        public delegate void RewardVideoFail(string i_Placement, IAdNetworkError i_AdNetworkError);

        public string PlacementId = Constants.k_None;

        public event RewardVideoSuccess SuccessEvent;
        public event RewardVideoOpen OpenEvent;
        public event RewardVideoClose CloseEvent;
        public event RewardVideoFail FailEvent;

        public void OnSuccess() { SuccessEvent?.Invoke(PlacementId); }
        public void OnOpened() { OpenEvent?.Invoke(PlacementId); }
        public void OnFailed(IAdNetworkError i_AdNetworkError) { FailEvent?.Invoke(PlacementId, i_AdNetworkError); }
        public void OnClosed(bool i_IsRVSuccess) { CloseEvent?.Invoke(PlacementId, i_IsRVSuccess); }

        public void ResetCallbacks(RewardVideoEvents i_GlobalEvents, RewardVideoSuccess i_RewardVideoSuccess = null, RewardVideoOpen i_RewardVideoOpen = null, RewardVideoClose i_RewardVideoClose = null, RewardVideoFail i_RewardVideoFail = null, string i_PlacementId = Constants.k_None)
        {
            InjectCallBacks(i_GlobalEvents, false);

            SuccessEvent += i_RewardVideoSuccess;
            OpenEvent += i_RewardVideoOpen;
            CloseEvent += i_RewardVideoClose;
            FailEvent += i_RewardVideoFail;

            if (i_PlacementId != Constants.k_None) PlacementId = i_PlacementId;
        }

        public void InjectCallBacks(RewardVideoEvents i_RewardVideoCallbacks, bool i_Append = true)
        {
            if (i_Append)
            {
                SuccessEvent += i_RewardVideoCallbacks.SuccessEvent;
                OpenEvent += i_RewardVideoCallbacks.OpenEvent;
                CloseEvent += i_RewardVideoCallbacks.CloseEvent;
                FailEvent += i_RewardVideoCallbacks.FailEvent;
            }
            else
            {
                SuccessEvent = i_RewardVideoCallbacks.SuccessEvent;
                OpenEvent = i_RewardVideoCallbacks.OpenEvent;
                CloseEvent = i_RewardVideoCallbacks.CloseEvent;
                FailEvent = i_RewardVideoCallbacks.FailEvent;
            }
        }
    }
}