using System;
using System.Collections;
using System.Collections.Generic;
using KobGamesSDKSlim;
using KobGamesSDKSlim.GameManagerV1;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class JoystickUI : MonoBehaviour
{
    [SerializeField, ReadOnly] private Image m_JoystickOutline;
    [SerializeField, ReadOnly] private Image m_JoystickHandle;
    
    [SerializeField, ReadOnly] private Canvas m_Canvas;
    [SerializeField, ReadOnly] private CanvasScaler m_CanvasScaler;

    private float m_CurrentMatchWidthOrHeight;

    private InputVariablesEditor m_InputVars => GameConfig.Instance.Input;

    [Button]
    public void SetRefs()
    {
        m_JoystickOutline = transform.FindDeepChild<Image>("Joystick Outline");
        m_JoystickHandle  = transform.FindDeepChild<Image>("Joystick Handle");
        m_Canvas          = transform.GetComponentInParent<Canvas>();
        m_CanvasScaler    = transform.GetComponentInParent<CanvasScaler>();
    }

    #region Enable/Disable
    private void OnEnable()
    {
        InputManager.OnInputDown        += onInputDown;
        InputManager.OnInputUp          += onInputUp;
        GameManagerBase.OnScreenChanged += onScreenChanged;

        if (!m_InputVars.IsUseJoystick || !m_InputVars.Joystick.IsShowVisuals)
        {
            gameObject.SetActive(false);
        }

        m_CurrentMatchWidthOrHeight = m_CanvasScaler.matchWidthOrHeight;
        setSizes();
    }

    private void OnDisable()
    {
        InputManager.OnInputDown        -= onInputDown;
        InputManager.OnInputUp          -= onInputUp;
        GameManagerBase.OnScreenChanged -= onScreenChanged;
    }
    #endregion
    
    #region Events
    private void onInputDown(Vector2 i_Pos)
    {
        if (m_InputVars.IsUseJoystick && m_InputVars.Joystick.IsShowVisuals)
        {
            m_JoystickOutline.gameObject.SetActive(true);
        }
    }
    
    private void onInputUp()
    {
        if (m_InputVars.IsUseJoystick && m_InputVars.Joystick.IsShowVisuals)
        {
            m_JoystickOutline.gameObject.SetActive(false);
        }
    }

    private void onScreenChanged(int i_Width, int i_Height)
    {
        setSizes();
    }
    #endregion

    private void Update()
    {
        if (m_CurrentMatchWidthOrHeight != m_CanvasScaler.matchWidthOrHeight)
        {
            m_CurrentMatchWidthOrHeight = m_CanvasScaler.matchWidthOrHeight;
            setSizes();
        }
            
        if (m_InputVars.IsUseJoystick && m_InputVars.Joystick.IsShowVisuals)
        {
            //I decided to make the joystick full height - 1920 - so, when we do the following operations we can end up with 1 inch diameter circle independently of resolution or physical size.
            //We get 1 inch circle by dividing 1 by height. Then we multiply by diameter set in GameConfig
            var screenSize = Mathf.Lerp(InputManager.Instance.ScreenData.ScreenSizeInch.x, InputManager.Instance.ScreenData.ScreenSizeInch.y, m_CanvasScaler.matchWidthOrHeight);
            m_JoystickOutline.rectTransform.localScale       = Vector3.one / screenSize * m_InputVars.Joystick.Radius * 2;
            m_JoystickOutline.rectTransform.anchoredPosition = InputManager.Instance.JoystickPosition / m_Canvas.scaleFactor;
            //Since HandlePosition is normalized - 0-1 - we can multiply by parent size and so get correct final position
            m_JoystickHandle.rectTransform.anchoredPosition = InputManager.Instance.JoystickHandleLocalPosition * m_JoystickOutline.rectTransform.sizeDelta.y * .5f;
        }       
    }
    
    private void setSizes()
    {
        m_JoystickOutline.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(m_CanvasScaler.referenceResolution.x, m_CanvasScaler.referenceResolution.y, m_CanvasScaler.matchWidthOrHeight);
        m_JoystickHandle.rectTransform.sizeDelta  = m_JoystickOutline.rectTransform.sizeDelta * m_InputVars.Joystick.HandleRadiusMultiplier;
    }
}
