using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Linq;

namespace KobGamesSDKSlim.MenuManagerV1
{
	public class MenuScreenBase : MonoBehaviour
    {
        [SerializeField] protected bool m_HideBackground = false;
        [SerializeField] protected bool DefaultAnimateOnOpen = false;
        [SerializeField] protected bool DefaultAnimateOnClose = false;

        private bool m_AnimateOnOpen;
        private bool m_AnimateOnClose;

        [SerializeField] private bool m_HideOnReset = true;

        [SerializeField, ReadOnly] protected CanvasGroup m_CanvasGroup;
        [SerializeField, ReadOnly] private Button[] m_AllUnityButtons = new Button[0];
        [SerializeField, ReadOnly] private bool[] m_AllUnityButtonsOriginalInteractivity = new bool[0];
        [SerializeField, ReadOnly] private ExtendedButton[] m_AllExtendedButtons = new ExtendedButton[0];

        [SerializeField, ReadOnly] protected Image m_Background;

        protected Tween m_FadeTween;

        protected MenuVariablesEditor m_MenuVars => GameConfig.Instance.Menus;

        [Button]
        protected virtual void SetRefs()
        {
            m_CanvasGroup        = transform.FindDeepChild<CanvasGroup>("Canvas Group");
            //Keeping the legacy name which was mostly used for background
            m_Background         = transform.FindDeepChild<Image>("Screen");
            m_AllUnityButtons    = GetComponentsInChildren<Button>(true);
            m_AllExtendedButtons = GetComponentsInChildren<ExtendedButton>(true);

            m_AllUnityButtons = m_AllUnityButtons.Except(m_AllExtendedButtons).ToArray();

            m_AllUnityButtonsOriginalInteractivity = new bool[m_AllUnityButtons.Length];
            for (int i = 0; i < m_AllUnityButtons.Length; i++)
            {
                m_AllUnityButtonsOriginalInteractivity[i] = m_AllUnityButtons[i].interactable;
            }
        }

        protected virtual void OnValidate()
        {
            SetRefs();
        }

        #region Unity Loop
        protected virtual void Awake()
        {
            if (m_Background != null && m_HideBackground)
                m_Background.color = new Color(m_Background.color.r, m_Background.color.g, m_Background.color.b, 0);
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update() { }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }
        #endregion

        public virtual void Reset()
        {
            //Debug.LogError("Reset " + gameObject.name);
            gameObject.SetActive(!m_HideOnReset);
            m_CanvasGroup.alpha = m_HideOnReset ? 0 : 1;
        }

        #region Open
        public virtual void Open()
        {
            Open(DefaultAnimateOnOpen);
        }
        public virtual void Open(bool i_Animate)
        {
            //Debug.LogError("Open " + gameObject.name);
            m_AnimateOnOpen = i_Animate;
            OnScreenOpenStart();

            ResetUnityButtonsToOriginalInteractivity();
        }


        private void OnScreenOpenStart()
        {
            MenuManager.Instance.OnMenu_ScreenOpenStart(this);

            OpenAnim();
        }

        protected virtual void OpenAnim()
        {
            gameObject.SetActive(true);
            m_CanvasGroup.alpha = 0;

            m_FadeTween?.Kill();
            m_FadeTween = m_CanvasGroup.DOFade(1, m_AnimateOnOpen ? m_MenuVars.ScreenFadeInDuration : 0)
            .OnComplete(OnScreenOpenEnd);

            //OnScreenOpenEnd();
        }

        protected virtual void OnScreenOpenEnd()
		{
			MenuManager.Instance.OnMenu_ScreenOpenEnd(this);

		}
        #endregion

        #region Close
        public virtual void Close()
		{
            Close(DefaultAnimateOnClose);
        }

        public virtual void Close(bool i_Animate)
        {
            //Debug.LogError("Close " + gameObject.name);
            m_AnimateOnClose = i_Animate;
            OnScreenCloseStart();
        }

        private void OnScreenCloseStart()
        {
            MenuManager.Instance.OnMenu_ScreenCloseStart(this);

            ResetButtonsState();


            CloseAnim();
        }

        protected virtual void CloseAnim()
        {
            //gameObject.SetActive(false);

            m_FadeTween?.Kill();
            m_FadeTween = m_CanvasGroup.DOFade(0, m_AnimateOnClose ? m_MenuVars.ScreenFadeOutDuration : 0)
            .OnComplete(OnScreenCloseEnd);

            //OnScreenCloseEnd();
        }

        protected virtual void OnScreenCloseEnd()
		{
			MenuManager.Instance.OnMenu_ScreenCloseEnd(this);
            gameObject.SetActive(false);
		}
        #endregion

        #region Buttons
        protected void SetButtonsInteractivityOn()
        {
            SetButtonsInteractivity(true);
        }

        protected void SetButtonsInteractivityOff(bool i_TurnOnWithTimer = false)
        {
            SetButtonsInteractivity(false);
            if (i_TurnOnWithTimer)
                Invoke(nameof(SetButtonsInteractivityOn), m_MenuVars.RV_ButtonBlockDuration);
        }

        /// <summary>
        /// Quickly set interactivity for all buttons on the Screen
        /// </summary>
        /// <param name="i_Value">Interactivity value - True or False</param>
        private void SetButtonsInteractivity(bool i_Value)
        {
            for (int i = 0; i < m_AllExtendedButtons.Length; i++)
            {
                m_AllExtendedButtons[i].SetInteractable(i_Value);
            }

            for (int i = 0; i < m_AllUnityButtons.Length; i++)
            {
                m_AllUnityButtons[i].interactable = i_Value;
            }
        }

        

        /// <summary>
        /// Removes events and interactivity from all Extended Buttons
        /// Note - This should be used when we don't want to give more access to buttons. Turning interactivity on after won't work since events were removed.
        /// </summary>
        protected virtual void ResetButtonsState()
        {
            for (int i = 0; i < m_AllExtendedButtons.Length; i++)
            {
                m_AllExtendedButtons[i].ResetState();
            }

            for (int i = 0; i < m_AllUnityButtons.Length; i++)
            {
                m_AllUnityButtons[i].interactable = false;
            }
        }

        protected virtual void ResetUnityButtonsToOriginalInteractivity()
        {
            for (int i = 0; i < m_AllUnityButtons.Length; i++)
            {
                m_AllUnityButtons[i].interactable = m_AllUnityButtonsOriginalInteractivity[i];
            }
        }
        #endregion

        #region Debug
        [Button, BoxGroup("Debug")]
		public virtual void OpenDebug()
		{
            if (Application.isPlaying)
            {
                Open();
            }
		}
		[Button, BoxGroup("Debug")]
		public virtual void CloseDebug()
		{
            if (Application.isPlaying)
            {
                Close();
            }
		}
		#endregion
    }
}
