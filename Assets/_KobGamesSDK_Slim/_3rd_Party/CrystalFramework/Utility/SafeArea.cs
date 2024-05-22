using System;
using KobGamesSDKSlim;
using KobGamesSDKSlim.GameManagerV1;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Crystal
{
    public enum eSafeAreaVersion
    {
        Version1,
        Version2
    }

    public enum eSafeAreaType
    {
        None,
        OnlyTop,
        OnlyBottom,
        TopAndBottom
    }

#if UNITY_EDITOR

    [InitializeOnLoad]
    public static class PlayModeNotifier
    {
        static PlayModeNotifier()
        {
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        static void ModeChanged(PlayModeStateChange i_stateChange)
        {
            if (i_stateChange == PlayModeStateChange.ExitingPlayMode)
                SafeArea.Sim = SafeArea.SimDevice.None;
        }
    }
#endif

    /// <summary>
    /// Safe area implementation for notched mobile devices. Usage:
    ///  (1) Add this component to the top level of any GUI panel. 
    ///  (2) If the panel uses a full screen background image, then create an immediate child and put the component on that instead, with all other elements childed below it.
    ///      This will allow the background image to stretch to the full extents of the screen behind the notch, which looks nicer.
    ///  (3) For other cases that use a mixture of full horizontal and vertical background stripes, use the Conform X & Y controls on separate elements as needed.
    /// </summary>
    [ExecuteInEditMode]
    public class SafeArea : MonoBehaviour
    {
        [InfoBox("You are using a deprecated version. Consider using Version 2 instead.", InfoMessageType.Warning, nameof(m_CorrectVersionWarningBool))]
        public eSafeAreaVersion Version = eSafeAreaVersion.Version1;

        private bool m_CorrectVersionWarningBool => Version != eSafeAreaVersion.Version2;

        /// <summary>
        /// Simulation device that uses safe area due to a physical notch or software home bar. For use in Editor only.
        /// </summary>
        public enum SimDevice
        {
            /// <summary>
            /// Don't use a simulated safe area - GUI will be full screen as normal.
            /// </summary>
            None,
            /// <summary>
            /// Simulate the iPhone X and Xs (identical safe areas).
            /// </summary>
            iPhoneX,
            /// <summary>
            /// Simulate the iPhone Xs Max and XR (identical safe areas).
            /// </summary>
            iPhoneXsMax
        }

        /// <summary>
        /// Simulation mode for use in editor only. This can be edited at runtime to toggle between different safe areas.
        /// </summary>
        public static SimDevice Sim = SimDevice.None;

        /// <summary>
        /// Normalised safe areas for iPhone X with Home indicator (ratios are identical to iPhone Xs). Absolute values:
        ///  PortraitU x=0, y=102, w=1125, h=2202 on full extents w=1125, h=2436;
        ///  PortraitD x=0, y=102, w=1125, h=2202 on full extents w=1125, h=2436 (not supported, remains in Portrait Up);
        ///  LandscapeL x=132, y=63, w=2172, h=1062 on full extents w=2436, h=1125;
        ///  LandscapeR x=132, y=63, w=2172, h=1062 on full extents w=2436, h=1125.
        ///  Aspect Ratio: ~19.5:9.
        /// </summary>
        private Rect[] NSA_IPhoneX
        {
            get
            {
                switch (Version)
                {
                    case eSafeAreaVersion.Version1:
                        return new Rect[]
                        {
                            new Rect (0f, 102f / 2436f, 1f, 2202f / 2436f),  // Portrait
                            new Rect (132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)  // Landscape
                        };
                    case eSafeAreaVersion.Version2:
                        return new Rect[]
                        {
                            new Rect (0f, 157 / 2436f, 1f, 2202f / 2436f),  // Portrait
                            new Rect (132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)  // Landscape
                        };
                    default:
                        break;
                }
                Debug.LogError("Didn't find correct Safe Area version.");
                return new Rect[0];
            }
        }

        /// <summary>
        /// Normalised safe areas for iPhone Xs Max with Home indicator (ratios are identical to iPhone XR). Absolute values:
        ///  PortraitU x=0, y=102, w=1242, h=2454 on full extents w=1242, h=2688;
        ///  PortraitD x=0, y=102, w=1242, h=2454 on full extents w=1242, h=2688 (not supported, remains in Portrait Up);
        ///  LandscapeL x=132, y=63, w=2424, h=1179 on full extents w=2688, h=1242;
        ///  LandscapeR x=132, y=63, w=2424, h=1179 on full extents w=2688, h=1242.
        ///  Aspect Ratio: ~19.5:9.
        /// </summary>
        private Rect[] NSA_IphoneXSMax
        {
            get
            {
                switch (Version)
                {
                    case eSafeAreaVersion.Version1:
                        return new Rect[]
                        {
                            new Rect (0f, 102f / 2688f, 1f, 2454f / 2688f),  // Portrait
                            new Rect (132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f)  // Landscape
                        };
                    case eSafeAreaVersion.Version2:
                        return new Rect[]
                        {
                            new Rect (0f, 157 / 2688f, 1f, 2454f / 2688f),  // Portrait
                            new Rect (132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f)  // Landscape
                        };
                    default:
                        break;
                }
                Debug.LogError("Didn't find correct Safe Area version.");
                return new Rect[0];
            }
        }

        [System.Serializable]
        private class LandscapeModeData
        {
            public bool IsWork = true;
            public bool ConformX = true;
            public bool ConformY = false;
            //public bool OnlyLeft = true;
        }

        RectTransform Panel;
        Rect LastSafeArea = new Rect(0, 0, 0, 0);
        [SerializeField] bool ConformX = false;  // Conform to screen safe area on X-axis (default true, disable to ignore)
        [SerializeField] bool ConformY = true;  // Conform to screen safe area on Y-axis (default true, disable to ignore)

        //For syincing with old versions
        [SerializeField, HideInInspector] bool OnlyTop = false;

        [SerializeField, OnValueChanged(nameof(onSafeAreaTypeChanged))] private eSafeAreaType m_SafeAreaType = eSafeAreaType.TopAndBottom;

        //Bottom Banner
        [SerializeField, OnValueChanged(nameof(onBannerVisibilityChanged))] private bool m_IsBannerOffset = false;

        [SerializeField] private LandscapeModeData m_LandscapeMode;

        [SerializeField, HideInInspector] private Canvas m_RootCanvas;
        //[SerializeField, HideInInspector] private RectTransform m_Root;

        private void OnValidate()
        {
            //For syncing with old projects
            if (OnlyTop)
            {
                m_SafeAreaType = eSafeAreaType.OnlyTop;

                OnlyTop = false;
            }

            //Parent canvas for the banner logic
            m_RootCanvas = transform.GetComponentInParent<Canvas>();
            //m_Root = transform.GetComponentInParent<RectTransform>();
        }

        private void Awake()
        {
            Panel = GetComponent<RectTransform>();

            if (Panel == null)
            {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                Destroy(gameObject);
            }

            Refresh();

            if (Version == eSafeAreaVersion.Version1)
                Debug.LogWarning("Current Safe Area version is deprecated. Be sure if this is the correct version for this project.", gameObject);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            onScreenChanged(Screen.width, Screen.height);
        }

        private void OnEnable()
        {
            GameManagerBase.OnScreenChanged += onScreenChanged;

            AdsManager.OnBannerShown += onBannerVisibilityChanged;
            AdsManager.OnBannerHidden += onBannerVisibilityChanged;

            onBannerVisibilityChanged();
        }

        private void OnDisable()
        {
            GameManagerBase.OnScreenChanged -= onScreenChanged;

            AdsManager.OnBannerShown -= onBannerVisibilityChanged;
            AdsManager.OnBannerHidden -= onBannerVisibilityChanged;
        }

        private void onSafeAreaTypeChanged()
        {
            //We need to reset LastSafeArea, otherwise the safeArea won't be updated
            LastSafeArea = new Rect();
        }

        void Update()
        {
#if UNITY_EDITOR
            Refresh();
#endif
        }

        void Refresh()
        {
            Rect safeArea = GetSafeArea();

            if (safeArea != LastSafeArea)
            {
                ApplySafeArea(safeArea);

                //To update the banner offset when we change the screen in Editor
                //We need to wait for a bit, others the banner hieght won't be calculated correctly
                Invoke(nameof(onBannerVisibilityChanged), 0.1f);
            }
        }

        Rect GetSafeArea()
        {
            Rect safeArea = Screen.safeArea;

            if (Application.isEditor && Sim != SimDevice.None)
            {
                Rect nsa = new Rect(0, 0, Screen.width, Screen.height);

                switch (Sim)
                {
                    case SimDevice.iPhoneX:
                        if (Screen.height > Screen.width)  // Portrait
                            nsa = NSA_IPhoneX[0];
                        else  // Landscape
                            nsa = NSA_IPhoneX[1];
                        break;
                    case SimDevice.iPhoneXsMax:
                        if (Screen.height > Screen.width)  // Portrait
                            nsa = NSA_IphoneXSMax[0];
                        else  // Landscape
                            nsa = NSA_IphoneXSMax[1];
                        break;
                    default:
                        break;
                }

                safeArea = new Rect(Screen.width * nsa.x, Screen.height * nsa.y, Screen.width * nsa.width, Screen.height * nsa.height);
            }

            return safeArea;
        }

        void ApplySafeArea(Rect r)
        {
            if (r.width > r.height &&
                m_LandscapeMode.IsWork)
            {
                // Ignore x-axis?
                if (!m_LandscapeMode.ConformY)
                {
                    r.y = 0;
                    r.height = Screen.height;
                }

                // Ignore y-axis?
                if (!m_LandscapeMode.ConformX)
                {
                    r.x = 0;
                    r.width = Screen.width;
                }
            }
            else
            {
                LastSafeArea = r;

                if (ConformY)
                {
                    switch (m_SafeAreaType)
                    {
                        case eSafeAreaType.None:
                            r.yMin = 0;
                            r.yMax = Screen.height;
                            break;
                        case eSafeAreaType.OnlyTop:
                            r.yMin = 0;
                            break;
                        case eSafeAreaType.OnlyBottom:
                            r.yMax = Screen.height;
                            break;
                        case eSafeAreaType.TopAndBottom:
                            break;
                    }
                }

                // Ignore x-axis?
                if (!ConformX)
                {
                    r.x = 0;
                    r.width = Screen.width;
                }

                // Ignore y-axis?
                if (!ConformY)
                {
                    r.y = 0;
                    r.height = Screen.height;
                }
            }

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            Panel.anchorMin = anchorMin;
            Panel.anchorMax = anchorMax;

            //Debug.LogFormat ("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            //    name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
        }


        private static Vector2Int IphoneX = new Vector2Int(1125, 2436);
        private static Vector2Int IphoneXLand = new Vector2Int(2436, 1125);
        private static Vector2Int IphoneXsMax = new Vector2Int(1242, 2688);
        private static Vector2Int IphoneXsMaxLand = new Vector2Int(2688, 1242);
        private static Vector2Int IphoneXR = new Vector2Int(828, 1792);
        private static Vector2Int IphoneXRLand = new Vector2Int(1792, 828);

        private void onScreenChanged(int i_Width, int i_Height)
        {
            var screenRes = new Vector2Int(i_Width, i_Height);

            if (screenRes == IphoneX || screenRes == IphoneXLand)
                SafeArea.Sim = SafeArea.SimDevice.iPhoneX;
            else if (screenRes == IphoneXR || screenRes == IphoneXsMax || screenRes == IphoneXsMaxLand || screenRes == IphoneXRLand)
                SafeArea.Sim = SafeArea.SimDevice.iPhoneXsMax;
            else
                SafeArea.Sim = SafeArea.SimDevice.None;
        }

        public string CurrentSimulateDevice { get { return "Simulate Device: " + Crystal.SafeArea.Sim.ToString(); } }

        [Button(Name = "$CurrentSimulateDevice")]
        public void SimulateDevice()
        {
            Crystal.SafeArea.SimDevice[] devices = (Crystal.SafeArea.SimDevice[])Enum.GetValues(typeof(Crystal.SafeArea.SimDevice));
            int currentIndex = -1;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i] == Crystal.SafeArea.Sim)
                {
                    currentIndex = i;
                    break;
                }
            }
            currentIndex++;
            if (currentIndex >= devices.Length)
            {
                currentIndex = 0;
            }
            Crystal.SafeArea.Sim = devices[currentIndex];
            Debug.Log("SafeArea: " + Crystal.SafeArea.Sim);
        }

        #region Banner
        private void onBannerVisibilityChanged() => adjustBannerArea(AdsManager.Instance.IsBannerVisible);

        private void adjustBannerArea(bool i_IsBannerVisible)
        {
            if (!Application.isPlaying)
                return;

            if (m_IsBannerOffset)
            {
                if (i_IsBannerVisible)
                {
                    float height = MaxSdkUtils.GetAdaptiveBannerHeight();
                    float screenDensity = MaxSdkUtils.GetScreenDensity();
                    height *= screenDensity;
#if UNITY_EDITOR
                    height = 168;
#endif
                    //Panel.offsetMin = new Vector2(Panel.offsetMin.x, m_Root.rect.height * (height / Screen.height));
                    Panel.offsetMin = new Vector2(Panel.offsetMin.x, height / m_RootCanvas.scaleFactor);
                }
                else
                {
                    Panel.offsetMin = new Vector2(Panel.offsetMin.x, 0);
                }
            }
            else
            {
                Panel.offsetMin = new Vector2(Panel.offsetMin.x, 0);
            }
        }
        #endregion

    }
}
