using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using KobGamesSDKSlim.Animation;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using KobGamesSDKSlim.Collectable;
using Object = UnityEngine.Object;

namespace KobGamesSDKSlim
{
    public abstract class GameConfigBase : SingletonScriptableObject<GameConfig>
    {
#if ENABLE_BEPIC || ENABLE_KOBIC
        [PropertyOrder(0)] public Bepic.CashOutData CashOut = new Bepic.CashOutData();
#endif

        [FoldoutGroup("IOS ATT", true)]
        [FoldoutGroup("IOS ATT"), OnValueChanged(nameof(OnATTChanged))]
        [PropertyOrder(0)] public bool IsIOSATT = false;
        [FoldoutGroup("IOS ATT"), OnValueChanged(nameof(OnATTChanged)), DelayedProperty]
        [PropertyOrder(0), ShowIf(nameof(IsIOSATT))] public string ATTPrivacyPolicy = "http://change.com";
        [FoldoutGroup("IOS ATT"), OnValueChanged(nameof(OnATTChanged))]
        [PropertyOrder(0), ShowIf(nameof(IsIOSATT))] public bool MAXControlsLocalizations = false;

        [OnInspectorInit]
        public void OnInspectorInit()
        {
#if UNITY_EDITOR
            UTUDLocalizationSettings localizationAsset = Resources.Load<UTUDLocalizationSettings>("UTUDLocalizationSettings");
            if (localizationAsset != null && localizationAsset.UseCustomLocalization != IsIOSATT)
            {
                UnityEditor.EditorApplication.delayCall = () => OnATTChanged();
            }
#endif
        }

        public void OnATTChanged()
        {
#if UNITY_EDITOR && ENABLE_ADS && ENABLE_MAX
            AppLovinSettings.Instance.ConsentFlowEnabled = IsIOSATT;
            AppLovinSettings.Instance.ConsentFlowPrivacyPolicyUrl = IsIOSATT ? ATTPrivacyPolicy : string.Empty;
            AppLovinSettings.Instance.UserTrackingUsageLocalizationEnabled = MAXControlsLocalizations;

            if (IsIOSATT)
            {
                AppLovinSettings.Instance.UserTrackingUsageDescriptionEn = "Device info will be used for delivering more relevant content";

                if (AppLovinSettings.Instance.UserTrackingUsageLocalizationEnabled)
                {
                    AppLovinSettings.Instance.UserTrackingUsageDescriptionZhHans = "设备会被用来为你提供更具相关性的内容";
                    AppLovinSettings.Instance.UserTrackingUsageDescriptionFr = "Les informations de l'appareil serviront à fournir des contenus plus pertinents";
                    AppLovinSettings.Instance.UserTrackingUsageDescriptionDe = "Die Geräteinformation wird verwendet, um auf dich zugeschnittene Inhalte zu liefern.";
                    AppLovinSettings.Instance.UserTrackingUsageDescriptionJa = "デバイス情報は関連性の高いコンテンツを表示するために使用されます";
                    AppLovinSettings.Instance.UserTrackingUsageDescriptionKo = "기기 정보는 보다 관련성이 높은 콘텐츠를 제공하는 데 사용됩니다";
                    AppLovinSettings.Instance.UserTrackingUsageDescriptionEs = "Los datos del dispositivo se utilizarán para ofrecer contenido más relevante ";
                }
            }

            UTUDLocalizationSettings localizationAsset = Resources.Load<UTUDLocalizationSettings>("UTUDLocalizationSettings");
            localizationAsset.UseCustomLocalization = IsIOSATT;

            UnityEditor.EditorUtility.SetDirty(AppLovinSettings.Instance);
            UnityEditor.EditorUtility.SetDirty(localizationAsset);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        [PropertyOrder(100)] public AdsMediation AdsMediation = new AdsMediation();
        [PropertyOrder(100)] public ResolutionsData ResolutionsData = new ResolutionsData();
    }

    [Serializable]
    public abstract class DebugVariablesEditorBase
    {
        [SerializeField] private bool m_DebugMode = false;
        public bool DebugMode
        {
            get
            {
#if UNITY_EDITOR
                return m_DebugMode;
#else
                if (Debug.isDebugBuild)
                    return m_DebugMode;
                else
                    return false;
#endif
            }
        }

        [Title("Gameplay")]
        [SerializeField, ShowIf(nameof(DebugMode))] private bool m_GodMode = false;
        public bool GodMode { get { return DebugMode && m_GodMode; } }

        [SerializeField, ShowIf(nameof(DebugMode)), OnValueChanged(nameof(OnEmulateLowFPSChange))] private bool m_EmulateLowFPS = false;
        public bool EmulateLowFPS { get { return DebugMode && m_EmulateLowFPS; } }
        [ShowIf(nameof(EmulateLowFPS))]
        public Vector2Int LowFPSValues;

        //TODO - we really need a debug Ads so we can test Ads on mobile when they are not available


        [Title("Misc")]
        //[SerializeField, ShowIf("DebugMode")] private bool m_ShowIphoneXDiagram = false;
        //public bool ShowIphoneXDiagram { get { return DebugMode && m_ShowIphoneXDiagram; } }
        //[ShowIf("ShowIphoneXDiagram")]
        [InlineButton(nameof(SetRefs), "Set Ref")]
        //public ResourcesObject IphoneXDiagramPrefab;
        public GameObject IphoneXDiagramPrefab;

        // [SerializeField, ShowIf(nameof(DebugMode))] private bool m_ShowAdsDebugButtons = false;
        public bool ShowAdsDebugButtons { get { return DebugMode; } }
        [ShowIf(nameof(ShowAdsDebugButtons)), InlineButton(nameof(SetRefs), "Set Ref")]
        //public ResourcesObject GameLoopButtonsPrefab;
        public GameObject AdsDebugButtonsPrefab;
        
        [SerializeField, ShowIf(nameof(DebugMode))] private bool m_ShowFPSCounter = false;
        public bool ShowFPSCounter { get { return DebugMode && m_ShowFPSCounter; } }
        [ShowIf(nameof(ShowFPSCounter)), InlineButton(nameof(SetRefs), "Set Ref")]
        //public ResourcesObject FPSCounterPrefab;
        public GameObject FPSCounterPrefab;


        [SerializeField, ShowIf(nameof(DebugMode))] private bool m_ShowGameLoopButtons = false;
        public bool ShowGameLoopButtons { get { return DebugMode && m_ShowGameLoopButtons; } set => m_ShowGameLoopButtons = value; }
        [ShowIf(nameof(ShowGameLoopButtons)), InlineButton(nameof(SetRefs), "Set Ref")]
        //public ResourcesObject GameLoopButtonsPrefab;
        public GameObject GameLoopButtonsPrefab;
        
        

        [SerializeField, ShowIf(nameof(DebugMode))] private bool m_ShowMediationTester = false;
        public                                              bool ShowMediationTester { get { return DebugMode && m_ShowMediationTester; } set => m_ShowMediationTester = value; }
        [ShowIf(nameof(ShowMediationTester)), InlineButton(nameof(SetRefs), "Set Ref")]
        //public ResourcesObject GameLoopButtonsPrefab;
        public GameObject MediationTesterPrefab;
        
        private void OnEmulateLowFPSChange()
        {
            if (!EmulateLowFPS)
                GameManager.Instance.SetDefaultFPS();
        }

        public void SetRefs()
        {
#if UNITY_EDITOR
            UnityEngine.GameObject FPSCounterFetched            = Utils.GetAsset<GameObject>("t:prefab FPSCounterPrefab",     "Assets");
            UnityEngine.GameObject IphoneXDiagramFetched        = Utils.GetAsset<GameObject>("t:prefab IphoneXDiagram",       "Assets");
            UnityEngine.GameObject GameLoopButtonsPrefabFetched = Utils.GetAsset<GameObject>("t:prefab GameLoopDebugButtons", "Assets");
            UnityEngine.GameObject MediationTesterFetched       = Utils.GetAsset<GameObject>("t:prefab MediationTester",      "Assets");
            UnityEngine.GameObject AdsDebugButtonsFetched       = Utils.GetAsset<GameObject>("t:prefab AdsDebugButtons",      "Assets");

            //so we can mark it dirty only when a prefab changed
            //bool differentPrefabs = FPSCounterPrefab.Prefab != FPSCounterFetched || IphoneXDiagramPrefab.Prefab != IphoneXDiagramFetched || GameLoopButtonsPrefab.Prefab != GameLoopButtonsPrefabFetched;
            bool differentPrefabs = FPSCounterPrefab      != FPSCounterFetched            || 
                                    IphoneXDiagramPrefab  != IphoneXDiagramFetched        || 
                                    GameLoopButtonsPrefab != GameLoopButtonsPrefabFetched ||
                                    MediationTesterPrefab != MediationTesterFetched       ||
                                    AdsDebugButtonsPrefab != AdsDebugButtonsFetched;


            //FPSCounterPrefab.Prefab = FPSCounterFetched;
            //IphoneXDiagramPrefab.Prefab = IphoneXDiagramFetched;
            //GameLoopButtonsPrefab.Prefab = GameLoopButtonsPrefabFetched;

            FPSCounterPrefab      = FPSCounterFetched;
            IphoneXDiagramPrefab  = IphoneXDiagramFetched;
            GameLoopButtonsPrefab = GameLoopButtonsPrefabFetched;
            MediationTesterPrefab = MediationTesterFetched;
            AdsDebugButtonsPrefab = AdsDebugButtonsFetched;

            if (differentPrefabs)
            {
                UnityEditor.EditorUtility.SetDirty(GameConfig.Instance);

                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
#endif
        }

        public void InitOnGameManagerAwakeEvent()
        {
#if UNITY_EDITOR
            SetRefs();
#endif

            //When in Editor, always create so we can easily hide/show with GameConfig as asked by Koby
#if UNITY_EDITOR
            //Utils.InstantiateResourcesObject(GameConfig.Instance.Debug.FPSCounterPrefab);
            //Utils.InstantiateResourcesObject(GameConfig.Instance.Debug.GameLoopButtonsPrefab);

            Object.Instantiate(GameConfig.Instance.Debug.FPSCounterPrefab);
            Object.Instantiate(GameConfig.Instance.Debug.GameLoopButtonsPrefab);
            Object.Instantiate(GameConfig.Instance.Debug.AdsDebugButtonsPrefab);

#else
            if (ShowFPSCounter)
            {
                //Utils.InstantiateResourcesObject(GameConfig.Instance.Debug.FPSCounterPrefab);
                Object.Instantiate(GameConfig.Instance.Debug.FPSCounterPrefab);
            }

            if (ShowGameLoopButtons)
            {
                //Utils.InstantiateResourcesObject(GameConfig.Instance.Debug.GameLoopButtonsPrefab);
                Object.Instantiate(GameConfig.Instance.Debug.GameLoopButtonsPrefab);
            }
            
            if (ShowMediationTester)
            {
                Object.Instantiate(GameConfig.Instance.Debug.MediationTesterPrefab);
            }
            
            if (ShowAdsDebugButtons)
            {
                Object.Instantiate(GameConfig.Instance.Debug.AdsDebugButtonsPrefab);
            }
        {  
  
    }
#endif

#if UNITY_EDITOR
            //Utils.InstantiateResourcesObject(GameConfig.Instance.Debug.IphoneXDiagramPrefab);
            GameObject.Instantiate(GameConfig.Instance.Debug.IphoneXDiagramPrefab);
#endif

            if (!GameConfig.Instance.Menus.ExtendedButtonCanDoHaptics || !GameConfig.Instance.Menus.ExtendedButtonCanDoScaleAnim)
                Debug.LogWarning("New projects should have Button Scale and Haptics turned on. Check GameConfig.Menus. It's turned off by default for legacy purposes.");
        }
    }

    [Serializable]
    public abstract class InputVariablesEditorBase
    {
        public Vector2 DragSensitivity = new Vector2(1, 1);

        public bool IsUseJoystick;

        [ShowIf(nameof(IsUseJoystick))] public JoystickData Joystick;

        [Serializable]
        public class JoystickData
        {
            public bool IsShowVisuals = false;

            public bool IsStatic = false;

            public bool IsResetDirection = false;

            public float Radius = 120;
            public float HandleRadiusMultiplier = .25f;
        }

        public bool CalculateMultiTouch = false;
    }
    

    [Serializable]
    public abstract class LevelsVariablesEditorBase
    {
    }

    [Serializable]
    public abstract class GamePlayVariablesEditorBase
    {
    }

    [Serializable]
    public abstract class HUDVariablesEditorBase
    {
        public bool IsUseCollectables = false;

        [HideIf(nameof(IsUseCollectables))] public EarnObjectData EarnObject;

        [ShowIf(nameof(IsUseCollectables)), OnValueChanged(nameof(onValueChanged), true)] public TypeCollectableDataDictionary CollectableData;

        [ShowIf(nameof(IsUseCollectables))] public AnimationData AnimationData;

        [Obsolete, Serializable]
        public class EarnObjectData
        {
            public EarnObjectUIAnimData DefaultEarnObjectUIAnimData => EarnObjectUIAnimData.ContainsKey(eCoinSenderType.Default)
                ? EarnObjectUIAnimData[eCoinSenderType.Default] : null;

            public eCoinSenderTypeEarnObjectUIAnimDataDictionary EarnObjectUIAnimData;

            [Serializable] public class eCoinSenderTypeEarnObjectUIAnimDataDictionary : UnitySerializedDictionary<eCoinSenderType, EarnObjectUIAnimData> { }

            public bool IsPlayHapticsOnReceive = true;
            [ShowIf(nameof(IsPlayHapticsOnReceive))] public HapticTypes ReceivedHaptics = HapticTypes.Selection;
            [ShowIf(nameof(IsPlayHapticsOnReceive))] public HapticTypes EarningCompleteHaptics = HapticTypes.MediumImpact;
            [ShowIf(nameof(IsPlayHapticsOnReceive))] public float HapticsCooldown = 0.3f;
        }

        [Serializable] public class TypeCollectableDataDictionary : UnitySerializedDictionary<eCollectableType, CollectableData> { }

        private void onValueChanged()
        {
            foreach (var item in CollectableData)
            {
                item.Value.Type = item.Key;
            }
        }
    }

    public enum eButtonScaleAnim
    {
        ScaleInOut,
        Punch
    }

    [Serializable]
    public abstract class MenuVariablesEditorBase
    {
        public bool UseScreenSettings = false;
        public bool OpenLevelFailedScreenAfterReviveScreen = false;

        [Title("Buttons")]
        [Tooltip("Idle time until show No Thanks Buttons")]
        public float TimeToShowNoThanks = 2;
        [Tooltip("No thanks Fade In Duration")]
        public float ShowNoThanksDuration = 1;
        public Ease ShowNoThanksEase = Ease.InOutQuad;

        [Header("RV")]
        public float RV_NeedleRotationDuration = 1.3f;
        public Ease RV_NeedleRotationEase = Ease.InSine;
        public float RV_ButtonBlockDuration = 3;


        [Header("Extended Button")]
        public bool ExtendedButtonCanDoScaleAnim = false;
        public bool ExtendedButtonCanDoHaptics = false;

        public eButtonScaleAnim ExtendedButtonScaleAnimType = eButtonScaleAnim.Punch;
        public float ExtendedButtonPunchScale = .2f;
        public float ExtendedButtonPunchScaleDuration = .25f;
        public int ExtendedButtonPunchScaleVibrato = 7;
        public float ExtendedButtonPunchScaleElasticity = .66f;
        public Ease ExtendedButtonPunchScaleEaseIn = Ease.OutBack;
        public Ease ExtendedButtonPunchScaleEaseOut = Ease.OutCubic;


        [Title("Screen - Revive")]
        public float TimeToAllowRevive = 10;

        public GDPRData GDPR;
    }

    [Serializable]
    public abstract class PlayerVariablesEditorBase
    {
    }

    [Flags]
    public enum eCameraZoomType : byte
    {
        Dolly = 1 << 0,
        Ortho = 1 << 1,
        Perspective = 1 << 2
    }
    [Serializable]
    public class CameraZoomData
    {
        public float Sensitivity = 1;
        public Vector2 Limits;
        public CameraZoomData(float m_Sensitivity, Vector2 m_Limits)
        {
            Sensitivity = m_Sensitivity;
            Limits = m_Limits;
        }
    }
    [Serializable]
    public abstract class CameraVariablesEditorBase
    {
        private bool m_IsMultiTouchDisabled => !GameConfig.Instance.Input.CalculateMultiTouch;
        [EnumToggleButtons, InfoBox("Input -> CalculateMultiTouch is Disabled", InfoMessageType.Warning, VisibleIf = nameof(m_IsMultiTouchDisabled))]
        public eCameraZoomType ZoomType;

        [ShowIf("@(this.ZoomType & eCameraZoomType.Dolly) == eCameraZoomType.Dolly")]
        public CameraZoomData ZoomDolly = new CameraZoomData(1, new Vector2(-5, 5));
        [ShowIf("@(this.ZoomType.HasFlag(eCameraZoomType.Ortho))")]
        public CameraZoomData ZoomOrtho = new CameraZoomData(1, new Vector2(2, 5));
        [ShowIf("@(this.ZoomType & eCameraZoomType.Perspective) == eCameraZoomType.Perspective")]
        public CameraZoomData ZoomPerspective = new CameraZoomData(20, new Vector2(25, 90));
    }

    [Serializable]
    public abstract class HapticsVariablesEditorBase
    {
    }


    [Serializable]
    public class AdsMediation
    {
        [InfoBox("* Dummy Ads will only work on Development Build")]
        [HorizontalGroup("2"), HideLabel, DisplayAsString, PropertySpace(SpaceBefore = 0, SpaceAfter = -15)]
        public string dummyText = string.Empty;

        [SerializeField, HideInInspector]
        private bool m_UseDummyAdsOnDevice = false;

        [HorizontalGroup("1", Width = 100)]
        [VerticalGroup("1/A"), ShowInInspector]
        public bool UseDummyAdsOnDevice => Application.isEditor && m_UseDummyAdsOnDevice || m_UseDummyAdsOnDevice && Debug.isDebugBuild;

        public delegate void UseDummyAdsOnDeviceFromTestSuiteChange(bool i_Value);
        public static event UseDummyAdsOnDeviceFromTestSuiteChange OnUseDummyAdsOnDeviceFromTestSuiteChangeEvent = delegate { };

        public void SetForceUseDummyAdsOnDevice(bool i_Value)
        {
            m_UseDummyAdsOnDevice = i_Value;

            OnUseDummyAdsOnDeviceFromTestSuiteChangeEvent.Invoke(m_UseDummyAdsOnDevice);
        }

        [Button, VerticalGroup("1/B")]
        public void ToggleForceUseDummyAdsOnDevice()
        {
            m_UseDummyAdsOnDevice = !m_UseDummyAdsOnDevice;

            OnUseDummyAdsOnDeviceFromTestSuiteChangeEvent.Invoke(m_UseDummyAdsOnDevice);
        }

        // [Tooltip("Should we show the confirmation pop up by default? If false, pop up might still appear if it is forced in the RV call")]
        // public bool RVNeedsConfirmation = false;
        
        [ShowInInspector]
        public bool IsAdsEnabled
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsAdsEnabled.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsAdsEnabled.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [ShowInInspector]
        public bool IsBannerEnabled
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsBannerEnabled.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsBannerEnabled.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [ShowInInspector]
        public bool IsInterstitialEnabled
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsInterstitialEnabled.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsInterstitialEnabled.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [ShowInInspector]
        public bool IsRewardVideosEnabled
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsRewardVideosEnabled.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.IsRewardVideosEnabled.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [ShowInInspector]
        public int InterstitialMinLevel
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialMinLevel.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialMinLevel.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [ShowInInspector]
        public int RewardVideoCoolDown
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.RewardVideoCoolDown.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.RewardVideoCoolDown.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [ShowInInspector]
        public int InterstitialCoolDown
        {
            get { return Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialCoolDown.Value; }
            set
            {
                Managers.Instance.RemoteSettings.FirebaseRemoteConfig.InterstitialCoolDown.SetToDefaultValue(value, true);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Managers.Instance.RemoteSettings);
#endif
            }
        }

        [OnInspectorDispose]
        public void TryApplyManagersPrefab()
        {
#if UNITY_EDITOR
            //Debug.LogError("Lost Focus");
            Managers.Instance.ProcessValidateManagers();
#endif
        }

        [Button]
        public void SelectRemoteConfig()
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = Managers.Instance.RemoteSettings.gameObject;
#endif
        }
    }

    [Serializable]
    public class ResolutionsData
    {
        [SerializeField, ListDrawerSettings(ListElementLabelName = nameof(ResolutionData.NameLabel))]
        private List<ResolutionData> m_ResolutionDatas;

        public bool IsDebugEnabled = true;

        public ResolutionData GetResolutionData()
        {
            return GetResolutionData(Camera.main.pixelWidth, Camera.main.pixelHeight);
        }

        public ResolutionData GetResolutionData(int i_Width, int i_Height)
        {
            ResolutionData result = null;
            foreach (ResolutionData resolutionData in m_ResolutionDatas)
            {
                if ((i_Width == resolutionData.Width && i_Height == resolutionData.Height) ||
                    (i_Width == resolutionData.Height && i_Height == resolutionData.Width))
                {
                    result = resolutionData;
                    break;
                }
            }

            if (IsDebugEnabled && GameConfig.Instance.Debug.DebugMode)
            {
                if (result == null)
                {
                    Debug.LogErrorFormat("ResolutionData for {0} {1} not found. pls add this info to gameConfigV2 (ctrl+shift+t) \n NOTE: this info needs only for Editor mode", Camera.main.pixelWidth, Camera.main.pixelHeight);
                }
                else
                {
                    Debug.Log("ResolutionData = " + result);
                }
            }

            return result;
        }

        [Serializable]
        public class ResolutionData
        {
            public string Name;
            public int Width;
            public int Height;
            public float DPI;

            public string NameLabel { get { return "" + Width + ", " + Height + ", " + DPI; } }

            public override string ToString()
            {
                return Name + " " + NameLabel;
            }
        }
    }
}