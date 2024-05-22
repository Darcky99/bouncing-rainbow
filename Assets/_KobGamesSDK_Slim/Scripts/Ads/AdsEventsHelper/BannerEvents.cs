using UnityEngine;

namespace KobGamesSDKSlim
{
    public class BannerEvents
    {
        public delegate void BannerDestroyed(string i_Placement);
        public delegate void BannerShown(string i_Placement);
        public delegate void BannerHidden(string i_Placement);
        public delegate void BannerFailed(string i_Placement, IAdNetworkError i_AdNetworkError);

        public string PlacementId = Constants.k_None;

        public event BannerDestroyed DestroyedEvent;
        public event BannerShown ShownEvent;
        public event BannerHidden HiddenEvent;
        public event BannerFailed FailedEvent;

        public void OnDestroyed() { DestroyedEvent?.Invoke(PlacementId); }
        public void OnShown() { ShownEvent?.Invoke(PlacementId); }
        public void OnHidden() { HiddenEvent?.Invoke(PlacementId); }
        public void OnFailed(IAdNetworkError i_AdNetworkError) { FailedEvent?.Invoke(PlacementId, i_AdNetworkError); }

        public void ResetCallbacks(BannerEvents i_GlobalEvents, BannerShown i_ShownEvent = null, BannerHidden i_HiddenEvent = null, BannerFailed i_FailedEvent = null, BannerDestroyed i_BannerDestroyed = null, string i_PlacementId = Constants.k_None)
        {
            InjectCallBacks(i_GlobalEvents, false);

            ShownEvent += i_ShownEvent;
            HiddenEvent += i_HiddenEvent;
            FailedEvent += i_FailedEvent;
            DestroyedEvent += i_BannerDestroyed;

            if (i_PlacementId != Constants.k_None) PlacementId = i_PlacementId;
        }

        public void InjectCallBacks(BannerEvents i_BannerEvents, bool i_Append = true)
        {
            if (i_Append)
            {
                ShownEvent += i_BannerEvents.ShownEvent;
                HiddenEvent += i_BannerEvents.HiddenEvent;
                FailedEvent += i_BannerEvents.FailedEvent;
                DestroyedEvent += i_BannerEvents.DestroyedEvent;
            }
            else
            {
                ShownEvent = i_BannerEvents.ShownEvent;
                HiddenEvent = i_BannerEvents.HiddenEvent;
                FailedEvent = i_BannerEvents.FailedEvent;
                DestroyedEvent = i_BannerEvents.DestroyedEvent;
            }
        }
    }

    public class IAdNetworkError
    {
        private string description;
        private int code;

        public int getErrorCode()
        {
            return code;
        }

        public string getDescription()
        {
            return description;
        }

        public int getCode()
        {
            return code;
        }

        public IAdNetworkError(string errorCode, string errorDescription)
        {
            int.TryParse(errorCode, out code);

            description = errorDescription;
        }

        public IAdNetworkError(int errorCode, string errorDescription)
        {
            code = errorCode;
            description = errorDescription;
        }

        public override string ToString()
        {
            return code + " : " + description;
        }
    }
}