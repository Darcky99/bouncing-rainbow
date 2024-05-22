using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using KobGamesSDKSlim.Animation;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using System.Collections.Generic;
using System.Linq;

namespace KobGamesSDKSlim
{
    public enum eButtonState
    {
        Normal,
        Disabled
    }

    public enum eButtonInteractivityChangeReason
    {
        Manual,
        Reset,
        RV_Availability
    }

    public enum eButtonScaleSizeType
    {
        Default,
        CustomScale
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class ExtendedButton : Button, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private const string k_MaterialID_Desaturated = "_Desaturate";

        protected bool m_OneTimeSetup = false;
        //TODO - should probably be on a parent class
        private bool _IsSetup = false;
        protected bool m_IsSetup
        {
            get => _IsSetup;
            set { _IsSetup = value; if(value) firstTimeSetup(); }
        }

        private eButtonState m_CurrState = eButtonState.Normal;
        
        //______________________________

        //TODO - set private and create accessors
        //Needs to be public to be accessible from Editor
        public bool CanDoScaleAnim = true;
        public eButtonScaleSizeType ScaleSizeType = eButtonScaleSizeType.Default;
        public Vector3 CustomScale = Vector3.one;

        public bool CanDoHaptics = true;
        public HapticTypes HapticType = HapticTypes.Selection;

        public bool ResetOnDisable = true;

        public Material MaterialOverride;

        private Vector3 m_OriginalScale;

        // LEGACY
        [Obsolete]
        public Action onDown = () => { };
        [Obsolete]
        public Action onUp = () => { };
        // LEGACY

        public Action<PointerEventData> OnDownEvent = (i_PointerEventData) => { };
        public Action<PointerEventData> OnUpEvent = (i_PointerEventData) => { };
        public Action<PointerEventData> OnBeginDragEvent = (i_PointerEventData) => { };
        public Action<PointerEventData> OnDragEvent = (i_PointerEventDataevData) => { };
        public Action<PointerEventData> OnEndDragEvent = (i_PointerEventData) => { };

        public delegate void InteractivityChangedCallback(bool i_Value, eButtonInteractivityChangeReason i_Reason);
        public event InteractivityChangedCallback OnInteractivityChanged = delegate { };


        [SerializeField, ReadOnly] protected ExtendedButtonModuleBehaviour[] m_Modules = new ExtendedButtonModuleBehaviour[0];
        [SerializeField, ReadOnly] protected Transform m_ScalerTransform;

        [SerializeField, ReadOnly] protected CanvasGroup m_CanvasGroup;

        [SerializeField, ReadOnly] protected Image m_OriginalImage;

        [SerializeField, ReadOnly] protected Image[] m_Images;
        [SerializeField, ReadOnly] protected Color[] m_ImagesOriginalColors;

        [SerializeField, ReadOnly] protected Text[] m_Texts;
        [SerializeField, ReadOnly] protected Color[] m_TextsOriginalColors;

        [SerializeField, ReadOnly] protected TextMeshProUGUI[] m_TextsTMP;
        [SerializeField, ReadOnly] protected Color[] m_TextsTMPOriginalColors;
        [SerializeField, ReadOnly] protected Color[] m_TextsTMPOutlineOriginalColors;


        private MenuVariablesEditor MenuVars { get { return GameConfig.Instance.Menus; } }

        private Transform m_DummyTransform;

#if UNITY_EDITOR
        //Needs to be public due to custom Editor
        [Button]
        public virtual void SetRefs()
        {
            m_Modules = GetComponentsInChildren<ExtendedButtonModuleBehaviour>();

            for (int i = 0; i < m_Modules.Length; i++)
            {
                UpdateModuleData(m_Modules[i]);
            }

            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_OriginalImage = GetComponent<Image>();
            m_ScalerTransform = transform.Find("Scaler")?.GetComponent<Transform>();

            //TODO - To simplify?
            m_Images = GetComponentsInChildren<Image>();
            m_Texts = GetComponentsInChildren<Text>();
            m_TextsTMP = GetComponentsInChildren<TextMeshProUGUI>();

            GameObject[] modulesObjects = m_Modules.Select(x => x.gameObject).ToArray();
            List<Image> finalImagesList = new List<Image>();
            for (int i = 0; i < m_Images.Length; i++)
            {
                if (!modulesObjects.Contains(m_Images[i].gameObject))
                    finalImagesList.Add(m_Images[i]);
            }
            List<Text> finalText = new List<Text>();
            for (int i = 0; i < m_Texts.Length; i++)
            {
                if (!modulesObjects.Contains(m_Texts[i].gameObject))
                    finalText.Add(m_Texts[i]);
            }
            List<TextMeshProUGUI> finalTextTMP = new List<TextMeshProUGUI>();
            for (int i = 0; i < m_TextsTMP.Length; i++)
            {
                if (!modulesObjects.Contains(m_TextsTMP[i].gameObject))
                    finalTextTMP.Add(m_TextsTMP[i]);
            }

            m_Images = finalImagesList.ToArray();
            m_Texts = finalText.ToArray();
            m_TextsTMP = finalTextTMP.ToArray();
            //______________________________________________________________________________


            if (!interactable)
            {
                SetInteractable(false);
                UnityEditor.EditorUtility.SetDirty(this);
                return;
            }

            m_ImagesOriginalColors = new Color[m_Images.Length];
            for (int i = 0; i < m_ImagesOriginalColors.Length; i++)
            {
                m_ImagesOriginalColors[i] = m_Images[i].color;
            }

            m_TextsOriginalColors = new Color[m_Texts.Length];
            for (int i = 0; i < m_TextsOriginalColors.Length; i++)
            {
                m_TextsOriginalColors[i] = m_Texts[i].color;
            }

            m_TextsTMPOriginalColors = new Color[m_TextsTMP.Length];
            for (int i = 0; i < m_TextsTMPOriginalColors.Length; i++)
            {
                m_TextsTMPOriginalColors[i] = m_TextsTMP[i].color;
            }

            m_TextsTMPOutlineOriginalColors = new Color[m_TextsTMP.Length];
            for (int i = 0; i < m_TextsTMPOutlineOriginalColors.Length; i++)
            {
                if(m_TextsTMP[i].fontSharedMaterial != null && m_TextsTMP[i].fontSharedMaterial.HasColor(ShaderUtilities.ID_OutlineColor))
                    m_TextsTMPOutlineOriginalColors[i] = m_TextsTMP[i].fontSharedMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif 

        public void UpdateModuleData(ExtendedButtonModuleBehaviour i_Module)
        {
            i_Module.SetData(MaterialOverride, colors); 
        }

        #region Unity Loop

        protected sealed override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            //This is needed because Button funs are being called in the editor for some reason
            if (!Application.isPlaying) return;
#endif
            firstTimeSetup();
        }

        protected sealed override void OnEnable()
        {
            base.OnEnable();

#if UNITY_EDITOR
            //This is needed because Button funs are being called in the editor for some reason
            if (!Application.isPlaying) return;
#endif

            transform.localScale = m_OriginalScale;
            
            OnEnableImpl();
        }
        protected virtual void OnEnableImpl(){}

        protected sealed override void OnDisable()
        {
            base.OnDisable();

#if UNITY_EDITOR
            //This is needed because Button funs are being called in the editor for some reason
            if (!Application.isPlaying) return;
#endif

            //Otherwise it is running while in the editor even if not playing :$
            //only auto reset if is Setup so legacy projects won't break
            if (Application.isPlaying && m_IsSetup && ResetOnDisable)
                ResetState();
            
            OnDisableImpl();
        }
        
        protected virtual void OnDisableImpl() {}


#if UNITY_EDITOR
        protected sealed override void OnValidate()
        {
            base.OnValidate();

            SetRefs();
            
            OnValidateImpl();
        }
        
        protected virtual void OnValidateImpl() {}

#endif
        #endregion


        #region Events
        public override void OnPointerDown(PointerEventData i_PointerEventData)
        {
            base.OnPointerDown(i_PointerEventData);

            DoInputFeedback();

            OnDownEvent.InvokeSafe(i_PointerEventData);
            onDown.Invoke();
        }

        public override void OnPointerUp(PointerEventData i_PointerEventData)
        {
            base.OnPointerUp(i_PointerEventData);

            DoInputFeedback(false);

            OnUpEvent.InvokeSafe(i_PointerEventData);
            onUp.Invoke();
        }

        private void DoInputFeedback(bool i_Down = true)
        {
            m_DummyTransform = m_ScalerTransform != null ? m_ScalerTransform : transform;

            if (interactable && CanDoScaleAnim && MenuVars.ExtendedButtonCanDoScaleAnim)
            {
                m_DummyTransform.DOKill();

                Vector3 finalScale = Vector3.one;

                switch (ScaleSizeType)
                {
                    case eButtonScaleSizeType.Default:     
                        if(MenuVars.ExtendedButtonScaleAnimType == eButtonScaleAnim.Punch)
                            finalScale = Vector3.one * MenuVars.ExtendedButtonPunchScale;   
                        else
                            finalScale = Vector3.one * (1 + MenuVars.ExtendedButtonPunchScale);
                        break;

                    case eButtonScaleSizeType.CustomScale:  
                        finalScale = CustomScale;                                       
                        break;
                    default: break;
                }

                switch (MenuVars.ExtendedButtonScaleAnimType)
                {
                    case eButtonScaleAnim.ScaleInOut:
                        if (i_Down)
                            m_DummyTransform.DOScale(finalScale, MenuVars.ExtendedButtonPunchScaleDuration)
                                .SetEase(MenuVars.ExtendedButtonPunchScaleEaseIn);
                        else
                            m_DummyTransform.DOScale(Vector3.one, MenuVars.ExtendedButtonPunchScaleDuration)
                                .SetEase(MenuVars.ExtendedButtonPunchScaleEaseOut);
                        break;
                    case eButtonScaleAnim.Punch:
                        if (i_Down)
                            m_DummyTransform.DOPunchScale(finalScale, MenuVars.ExtendedButtonPunchScaleDuration, MenuVars.ExtendedButtonPunchScaleVibrato, MenuVars.ExtendedButtonPunchScaleElasticity);
                        else
                            m_DummyTransform.localScale = Vector3.one;

                        break;
                    default:
                        break;
                }
               
            }

            if (interactable && CanDoHaptics && MenuVars.ExtendedButtonCanDoHaptics && i_Down)
                Managers.Instance.HapticManager.Haptic(HapticType);
        }

        public virtual void OnBeginDrag(PointerEventData i_PointerEventData)
        {
            OnBeginDragEvent.InvokeSafe(i_PointerEventData);
        }

        public virtual void OnDrag(PointerEventData i_PointerEventData)
        {
            OnDragEvent.InvokeSafe(i_PointerEventData);
        }

        public virtual void OnEndDrag(PointerEventData i_PointerEventData)
        {
            OnEndDragEvent.InvokeSafe(i_PointerEventData);
        }
        #endregion


        #region Setup
        protected virtual void firstTimeSetup()
        {
            if (m_OneTimeSetup) return;

            m_OriginalScale = transform.localScale;

            //Needed so we can setup the same material for every button and then, at runtime, we split it per button
            if (MaterialOverride != null && m_OriginalImage != null)
            {
                m_OriginalImage.material = MaterialOverride = new Material(MaterialOverride);
            }

            for (int i = 0; i < m_Modules.Length; i++)
            {
                UpdateModuleData(m_Modules[i]);
            }
            //________________________________________

            m_OneTimeSetup = true;
        }

        /// <summary>
        /// When you Setup a button it will automatically Reset its state when disabled
        /// </summary>
        /// <param name="i_OnClick"></param>
        /// <param name="i_OnDownEvent"></param>
        /// <param name="OnUpEvent"></param>
        /// <param name="OnBeginDragEvent"></param>
        /// <param name="OnDragEvent"></param>
        /// <param name="OnEndDragEvent"></param>
        public void Setup(
            UnityAction i_OnClick = null, 
            Action<PointerEventData> i_OnDownEvent = null, 
            Action<PointerEventData> i_OnUpEvent = null, 
            Action<PointerEventData> i_OnBeginDragEvent = null, 
            Action<PointerEventData> i_OnDragEvent = null, 
            Action<PointerEventData> i_OnEndDragEvent = null,
            InteractivityChangedCallback i_OnInteractivityChanged = null)
        {
            if (m_IsSetup)
            {
                Debug.LogError("Button already Setup. Skiping!!!");
                return;
            }

            m_IsSetup = true;
            SetEvents(i_OnClick, i_OnDownEvent, i_OnUpEvent, i_OnBeginDragEvent, i_OnDragEvent, i_OnEndDragEvent, i_OnInteractivityChanged);

            SetInteractable(true);
        }


        protected void SetEvents(UnityAction i_OnClick = null,
            Action<PointerEventData> i_OnDownEvent = null,
            Action<PointerEventData> i_OnUpEvent = null,
            Action<PointerEventData> i_OnBeginDragEvent = null,
            Action<PointerEventData> i_OnDragEvent = null,
            Action<PointerEventData> i_OnEndDragEvent = null,
            InteractivityChangedCallback i_OnInteractivityChanged = null)
        {
            if(i_OnClick != null)
                onClick.AddListener(i_OnClick);
            OnDownEvent = i_OnDownEvent;
            OnUpEvent = i_OnUpEvent;
            OnBeginDragEvent = i_OnBeginDragEvent;
            OnDragEvent = i_OnDragEvent;
            OnEndDragEvent = i_OnEndDragEvent;
            OnInteractivityChanged = i_OnInteractivityChanged;
        }

        /// <summary>
        /// Will remove all events and turn interactivety off
        /// </summary>
        public virtual void ResetState()
        {
            m_IsSetup = false;

            onClick.RemoveAllListeners();
            OnDownEvent = OnUpEvent = OnBeginDragEvent = OnDragEvent = OnEndDragEvent = (i_PointerEventData) => { };

            OnInteractivityChanged = delegate { };
            
            m_DummyTransform = m_ScalerTransform != null ? m_ScalerTransform : transform;
            m_DummyTransform.DOKill();

            m_DummyTransform = m_ScalerTransform != null ? m_ScalerTransform : transform;
            m_DummyTransform.DOKill();

            SetInteractable(false, eButtonInteractivityChangeReason.Reset);
        }
        #endregion


        /// <summary>
        /// Not only switches interactivity status but also adjust the children graphics so they match visually
        /// </summary>
        /// <param name="i_Value"></param>
        public virtual void SetInteractable(bool i_Value)
        {
            SetInteractable(i_Value, eButtonInteractivityChangeReason.Manual);
        }

        /// <summary>
        /// Not only switches interactivity status but also adjust the children graphics so they match visually
        /// </summary>
        /// <param name="i_Value"></param>
        public virtual void SetInteractable(bool i_Value, eButtonInteractivityChangeReason i_Reason)
        {
            //Debug.LogError("Set Interactable " + i_Value, gameObject);

            bool prevValue = interactable;

            interactable = i_Value;

            if(i_Reason != eButtonInteractivityChangeReason.Reset)
                SetAssetsState(i_Value ? eButtonState.Normal : eButtonState.Disabled);

            if (i_Value == prevValue)
                return;

            OnInteractivityChanged?.Invoke(i_Value, i_Reason);
        }


        #region Visuals
        public virtual void SetGraphicsVisibility(bool i_Value)
        {
            //Debug.LogError("Set Alpha", gameObject);
            m_CanvasGroup.alpha = Convert.ToInt32(i_Value);
        }


        /// <summary>
        /// Go through all UI assets like Image and Text and set the appropriate colors
        /// </summary>
        /// <param name="i_LerpValue">0 = Disabled color, 1 = Enabled color</param>
        /// <param name="i_Alpha">Alpha multiplier</param>
        protected virtual void SetAssetsState(eButtonState i_ButtonState)
        {
            //Debug.LogError("Set Assets State " + i_ButtonState);
            for (int i = 0; i < m_Modules.Length; i++)
            {
                m_Modules[i].SetState(i_ButtonState);
            }

            Color multiplyColor = Color.white;
            if(MaterialOverride != null) 
                MaterialOverride.SetFloat(k_MaterialID_Desaturated, 0);

            switch (i_ButtonState)
            {
                case eButtonState.Disabled:
                    multiplyColor = colors.disabledColor;
                    if (MaterialOverride != null)
                        MaterialOverride.SetFloat(k_MaterialID_Desaturated, 1);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < m_Images.Length; i++)
            {
                //In case color changes in realtime
                if (m_CurrState == eButtonState.Normal) m_ImagesOriginalColors[i] = m_Images[i].color;
                
                if (m_Images[i] != m_OriginalImage)
                    m_Images[i].color = m_ImagesOriginalColors[i] * multiplyColor;
            }

            for (int i = 0; i < m_Texts.Length; i++)
            {
                //In case color changes in realtime
                if (m_CurrState == eButtonState.Normal) m_TextsOriginalColors[i] = m_Texts[i].color;

                m_Texts[i].color = m_TextsOriginalColors[i] * multiplyColor;
            }

            for (int i = 0; i < m_TextsTMP.Length; i++)
            {
                //In case color changes in realtime
                if (m_CurrState == eButtonState.Normal)
                {
                    m_TextsTMPOriginalColors[i]        = m_TextsTMP[i].color;
                    m_TextsTMPOutlineOriginalColors[i] = m_TextsTMP[i].fontMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
                }

                m_TextsTMP[i].color = m_TextsTMPOriginalColors[i] * multiplyColor;
                m_TextsTMP[i].fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, m_TextsTMPOutlineOriginalColors[i] * multiplyColor);

                //Debug.LogError(m_TextsTMP[i].name + "       " + m_TextsTMP[i].color);
            }

            m_CurrState = i_ButtonState;
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Enable Assets")]
        private void EnableAssets()
        {
            SetGraphicsVisibility(true);
        }

        [ContextMenu("Disable Assets")]
        private void DisableAssets()
        {
            SetGraphicsVisibility(false);
        }
#endif
        #endregion
    }
}