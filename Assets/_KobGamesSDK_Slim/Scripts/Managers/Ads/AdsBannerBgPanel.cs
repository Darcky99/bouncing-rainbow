using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    public class AdsBannerBgPanel : MonoBehaviour
    {
        public delegate void StateUpdatedListener(AdsBannerBgPanel i_BannerBgPanel);
        public static event StateUpdatedListener StateUpdatedListeners = delegate { };

        public Image BannerBgPanelImage;

        [SerializeField]
        private RectTransform m_RectTransform;

        [SerializeField]
        private Canvas m_AdsCanvas;

        private Canvas m_MainCanvasCache;
        [ShowInInspector]
        private Canvas m_MainCanvas
        {
            get
            {
                if (m_MainCanvasCache == null)
                {
                    foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
                    {
                        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                        {
                            m_MainCanvasCache = canvas;
                            break;
                        }
                    }

                    foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
                    {
                        m_MainCanvasCache = canvas;
                        break;
                    }
                }

                if (m_MainCanvasCache == null)
                {
                    Debug.LogError($"{nameof(AdsBannerBgPanel)}: Can't find main Canvas");
                }

                return m_MainCanvasCache;
            }
        }

        private int m_OldWidth;
        private int m_Oldheight;
        private bool m_IsBannerShown = false;

        private Vector2 m_SizeDeltaDummy = Vector2.zero;
        private float m_DPI;
        private int m_ScreenHeight;

        [ShowInInspector]
        public float MainHeight
        {
            get
            {
                return GetHeight(m_MainCanvas);
            }
        }

        private Camera m_MainCamera;
        
        
        private void OnValidate()
        {
            if (BannerBgPanelImage == null) BannerBgPanelImage = this.GetComponentInChildren<Image>();
            if (m_RectTransform == null) m_RectTransform = BannerBgPanelImage?.GetComponent<RectTransform>();
            if (m_AdsCanvas == null) m_AdsCanvas = this.GetComponentInParent<Canvas>();
        }

        public float GetHeight()
        {
            return GetHeight(m_MainCanvas);
        }

        public float GetHeight(Canvas i_MainCanvas)
        {
            if (i_MainCanvas == null || m_AdsCanvas == null || m_RectTransform == null || !gameObject.activeInHierarchy)
            {
                return 0;
            }

            RectTransform mainRect = i_MainCanvas.GetComponent<RectTransform>();
            RectTransform adsRect = m_AdsCanvas.GetComponent<RectTransform>();
            return mainRect.sizeDelta.y / adsRect.sizeDelta.y * m_RectTransform.sizeDelta.y;
        }

        private void Awake()
        {
            BannerBgPanelImage.enabled = false;
            //m_RectTransform?.gameObject.SetActive(false);

#if ENABLE_ADS && (ENABLE_IRONSOURCE || ENABLE_SUPERERA || ENABLE_BEPIC)
            if (AdsManager.Instance.ShowDummyBgPanel || (Application.isEditor && AdsManager.Instance.ShowDummyBgPanelEditor))
            {
                AdsManager.BannerEventsGlobal.ShownEvent += OnBannerShown;
                AdsManager.BannerEventsGlobal.HiddenEvent += OnBannerHidden;

                RemoteSettingsManager.OnFirebaseRemotConfigUpdated += onFirebaseRemotConfigUpdated;
            }
#endif
        }

        private void OnEnable()
        {
            UpdateBannerBgPanelImage();
        }

        private void OnDestroy()
        {
#if ENABLE_ADS && (ENABLE_IRONSOURCE || ENABLE_SUPERERA || ENABLE_BEPIC)
            AdsManager.BannerEventsGlobal.ShownEvent -= OnBannerShown;
            AdsManager.BannerEventsGlobal.HiddenEvent -= OnBannerHidden;
#endif
            RemoteSettingsManager.OnFirebaseRemotConfigUpdated -= onFirebaseRemotConfigUpdated;
        }

        private void onFirebaseRemotConfigUpdated(bool i_Success)
        {
            UpdateBannerBgPanelImage();
        }

        public void UpdateBannerBgPanelImage()
        {
            //if (BannerBgPanelImage != null)
            //{
            //    BannerBgPanelImage.color = GameConfiguration.Instance.GraphicsData.ColorsData.GetColorSchemeBuilding().AdsBanner;
            //}
        }

        private void Update()
        {
            if (m_MainCamera == null)
                m_MainCamera = Camera.main;
            else
            {
                if (m_OldWidth != m_MainCamera.pixelWidth || m_Oldheight != m_MainCamera.pixelHeight)
                {
                    m_OldWidth  = m_MainCamera.pixelWidth;
                    m_Oldheight = m_MainCamera.pixelHeight;
                    if (m_IsBannerShown)
                    {
                        OnBannerShown("");
                    }
                }
            }
        }

        [Button(parameterBtnStyle: ButtonStyle.FoldoutButton)]
        public void OnBannerShown(string i_PlacementId)
        {
            Debug.Log($"{nameof(AdsBannerBgPanel)}: {Utils.GetFuncName()}");

            m_IsBannerShown = true;
            //m_RectTransform?.gameObject.SetActive(true);
            BannerBgPanelImage.enabled = true;

            updateRectTransform();
            StateUpdatedListeners(this);
        }

        [Button(parameterBtnStyle: ButtonStyle.FoldoutButton)]
        public void OnBannerHidden(string i_PlacementId)
        {
            Debug.Log($"{nameof(AdsBannerBgPanel)}: {Utils.GetFuncName()}");

            m_IsBannerShown = false;
            //m_RectTransform?.gameObject.SetActive(false);
            BannerBgPanelImage.enabled = false;
            StateUpdatedListeners(this);
        }


        private void updateRectTransform()
        {
            if (m_RectTransform != null)
            {
                m_SizeDeltaDummy.Set(m_RectTransform.sizeDelta.x, getBannerSize());
                m_RectTransform.sizeDelta = m_SizeDeltaDummy;
            }
        }

        private int getBannerSize()
        {
            m_DPI = Screen.dpi;
            m_ScreenHeight = Screen.height;

            //#if ENABLE_ADS && ENABLE_IRONSOURCE
            //            ResolutionsData.ResolutionData resolutionData = GameConfig.Instance.ResolutionsData.GetResolutionData();
            //            if (resolutionData != null)
            //            {
            //                m_DPI = resolutionData.DPI;
            //            }

            //            m_ScreenHeight = Camera.main.pixelHeight;
            //            //Debug.LogError($"DPI: {m_DPI} Height: {Camera.main.pixelHeight} Width: {Camera.main.pixelWidth}");
            //#endif

#if UNITY_EDITOR
            return 168;
#endif
            if (m_ScreenHeight <= 720 * (m_DPI / 160))
            {
                if (m_ScreenHeight > 400 * (m_DPI / 160))
                {
                    return Mathf.RoundToInt(50 * (m_DPI / 160));
                }
                else if (m_ScreenHeight <= 400 * (m_DPI / 160))
                {
                    return Mathf.RoundToInt(32 * (m_DPI / 160));
                }
            }
            else
            {
                return Mathf.RoundToInt(90 * (m_DPI / 160));
            }

            return Mathf.RoundToInt(90 * (m_DPI / 160));
        }
    }
}
