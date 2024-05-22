using UnityEngine;

namespace KobGamesSDKSlim
{
    public class InterstitialEvents
    {
        public delegate void InterstitialOpen(string i_Placement);
        public delegate void InterstitialClose(string i_Placement);
        public delegate void InterstitialShown(string i_Placement);
        public delegate void InterstitialShownFail(string i_Placement, IAdNetworkError i_AdNetworkError);

        public string PlacementId = Constants.k_None;

        public event InterstitialOpen OpenEvent;
        public event InterstitialClose CloseEvent;
        public event InterstitialShown ShownEvent;
        public event InterstitialShownFail ShownFailEvent;

        public void OnOpened() { OpenEvent?.Invoke(PlacementId); }
        public void OnClosed() { CloseEvent?.Invoke(PlacementId); }
        public void OnShownSuccess() { ShownEvent?.Invoke(PlacementId); }
        public void OnShownFailed(IAdNetworkError i_AdNetworkError) { ShownFailEvent?.Invoke(PlacementId, i_AdNetworkError); }


        public void ResetCallbacks(InterstitialEvents i_GlobalEvents, InterstitialShown i_InterstitialShown = null, InterstitialOpen i_InterstitialOpen = null, InterstitialClose i_InterstitialClose = null, InterstitialShownFail i_InterstitialShownFail = null, string i_PlacementId = Constants.k_None)
        {
            InjectCallBacks(i_GlobalEvents, false);

            OpenEvent += i_InterstitialOpen;
            CloseEvent += i_InterstitialClose;
            ShownEvent += i_InterstitialShown;
            ShownFailEvent += i_InterstitialShownFail;

            if (i_PlacementId != Constants.k_None) PlacementId = i_PlacementId;
        }

        public void InjectCallBacks(InterstitialEvents i_InterstitialEventsCallbacks, bool i_Append = true)
        {
            if (i_Append)
            {
                OpenEvent += i_InterstitialEventsCallbacks.OpenEvent;
                CloseEvent += i_InterstitialEventsCallbacks.CloseEvent;
                ShownEvent += i_InterstitialEventsCallbacks.ShownEvent;
                ShownFailEvent += i_InterstitialEventsCallbacks.ShownFailEvent;
            }
            else
            {
                OpenEvent = i_InterstitialEventsCallbacks.OpenEvent;
                CloseEvent = i_InterstitialEventsCallbacks.CloseEvent;
                ShownEvent = i_InterstitialEventsCallbacks.ShownEvent;
                ShownFailEvent = i_InterstitialEventsCallbacks.ShownFailEvent;
            }
        }
    }
}