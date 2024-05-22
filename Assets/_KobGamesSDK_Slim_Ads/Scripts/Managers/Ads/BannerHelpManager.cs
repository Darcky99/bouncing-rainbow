using UnityEngine;

namespace KobGamesSDKSlim
{
    public class BannerHelpManager : Singleton<BannerHelpManager>
    {
        private bool m_IsHaveBanner = false;
        private void OnEnable()
        {
#if ENABLE_ADS
            AdsManager.OnBannerShown += bannerShowCallback;
            AdsManager.OnBannerHidden += bannerHiddenCallback;
#endif
        }

        public override void OnDisable()
        {
            base.OnDisable();
#if ENABLE_ADS
            AdsManager.OnBannerShown -= bannerShowCallback;
            AdsManager.OnBannerHidden -= bannerHiddenCallback;
#endif
        }

        private void bannerShowCallback()
        {
            m_IsHaveBanner = true;
        }
        private void bannerHiddenCallback()
        {
            m_IsHaveBanner = false;
        }

        public float GetBannerSize(RectTransform i_Canvas)
        {
            if (m_IsHaveBanner)
            {
                int bannerSize = getBannerSize();
                return ((float)bannerSize) / i_Canvas.localScale.y;
                //return 100;
            }
            return 0;
        }

        private int getBannerSize()
        {
            float DPI = Screen.dpi;
            int screenHeight = Screen.height;
#if UNITY_EDITOR
            return 168;
#endif
            if (screenHeight <= 720 * (DPI / 160))
            {
                if (screenHeight > 400 * (DPI / 160))
                {
                    return Mathf.RoundToInt(50 * (DPI / 160));
                }
                else if (screenHeight <= 400 * (DPI / 160))
                {
                    return Mathf.RoundToInt(32 * (DPI / 160));
                }
            }
            else
            {
                return Mathf.RoundToInt(90 * (DPI / 160));
            }

            return Mathf.RoundToInt(90 * (DPI / 160));
        }
    }
}