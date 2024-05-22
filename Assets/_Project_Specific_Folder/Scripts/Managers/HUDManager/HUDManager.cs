using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;
using KobGamesSDKSlim.Animation;
using KobGamesSDKSlim.UI;
using DG.Tweening;
using KobGamesSDKSlim.Collectable;

namespace KobGamesSDKSlim
{
    //TODO - Eventually we should upgrade this to HUDManagerBase
    public class HUDManager : Singleton<HUDManager>
    {
        [SerializeField, ReadOnly] private ExtendedButton m_InputButton;
        [SerializeField, ReadOnly] private TextMeshProUGUI m_LevelText;
        
        //This exists for legacy reasons. It is ok if it is not set. To be removed in the future
        [SerializeField, ReadOnly] private CoinUpdater m_CoinUpdater;

        [SerializeField, ReadOnly] private ProgressBar m_ProgressBar;

        
        [SerializeField, ReadOnly] private Canvas m_Canvas;
        [SerializeField, ReadOnly] private CanvasGroup m_CanvasGroup;

        
        public RectTransform DefaultCoinsTarget => m_CoinUpdater.CoinMoveTarget;

        private HUDVariablesEditor m_HUDVars { get { return GameConfig.Instance.HUD; } }
        private MenuVariablesEditor m_MenuVars { get { return GameConfig.Instance.Menus; } }
        private InputVariablesEditor m_InputVars { get { return GameConfig.Instance.Input; } }

 
        private Vector2 m_DummyVector2;

        [Button]
        public void SetRefs()
        {
            m_InputButton = transform.FindDeepChild<ExtendedButton>("Input");
            m_LevelText = transform.FindDeepChild<TextMeshProUGUI>("Level ID");

            m_ProgressBar = transform.FindDeepChild<ProgressBar>("ProgressBar");
            
            m_Canvas = transform.GetComponent<Canvas>();

            m_CanvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        private void OnValidate()
        {
            SetRefs();
        }

        private void OnEnable()
        {
            m_InputButton.OnDownEvent += InputDown;
            m_InputButton.OnUpEvent += InputUp;
            GameManager.OnGameReset += OnReset;
            GameManager.OnLevelReset += OnReset;
            GameManager.OnLevelCompleted += OnLevelFinished;
            GameManager.OnLevelFailed += OnLevelFinished;
            GameManager.OnLevelFailedNoContinue += OnLevelFinished;

            Init();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            m_InputButton.OnDownEvent -= InputDown;
            m_InputButton.OnUpEvent -= InputUp;
            GameManager.OnGameReset -= OnReset;
            GameManager.OnLevelReset -= OnReset;
            GameManager.OnLevelCompleted -= OnLevelFinished;
            GameManager.OnLevelFailed -= OnLevelFinished;
            GameManager.OnLevelFailedNoContinue -= OnLevelFinished;

        }

        private void Init()
        {
            //Here you can put everything that you need to initialize when game starts
            OnReset();
        }



        private void OnReset()
        {
            UpdateCurrentLevelText();
            SetLevelTextVisibility(true);
        }

        private void OnLevelFinished()
        {
            SetLevelTextVisibility(false);
        }

        public void SetLevelTextVisibility(bool i_IsVisible)
        {
            m_LevelText.gameObject.SetActive(i_IsVisible);
        }

        private void UpdateCurrentLevelText()
        {
            m_LevelText.text = "LEVEL " + StorageManager.Instance.CurrentLevel;
        }

        public void Open(bool i_Animate = true)
        {
            DOTween.Kill(this);
            m_CanvasGroup.DOFade(1, i_Animate ? m_MenuVars.ScreenFadeInDuration : 0);
        }

        public void Close(bool i_Animate = true)
        {
            DOTween.Kill(this);
            m_CanvasGroup.DOFade(0, i_Animate ? m_MenuVars.ScreenFadeOutDuration : 0);
        }

        #region Input
        public void InputDown(PointerEventData i_PointerEventData)
        {
            InputManager.Instance.InputDown();
        }

        public void InputUp(PointerEventData i_PointerEventData)
        {
            InputManager.Instance.InputUp();
        }
        #endregion


        #region ProgressBar

        public void OpenProgressBar()
        {
            m_ProgressBar.Open();
        }

        public void CloseProgressBar()
        {
            m_ProgressBar.Close();
        }

        public void SetProgressBarProgress(float i_Progress)
        {
            m_ProgressBar.SetProgress(i_Progress);   
        }

        #endregion
    }
}