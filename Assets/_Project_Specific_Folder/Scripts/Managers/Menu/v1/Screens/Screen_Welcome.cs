using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using UnityEngine.UI;
using KobGamesSDKSlim.GameManagerV1;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;

public class Screen_Welcome : MenuScreenBase
{
    [SerializeField, ReadOnly] private GameObject m_SettingsButton;
    [SerializeField, ReadOnly] private GameObject m_VibrationButton;
    [SerializeField, ReadOnly] private ExtendedButton m_TapToStart;

    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_TapToStart = transform.FindDeepChild<ExtendedButton>("Tap To Start Button");
        m_SettingsButton = transform.FindDeepChild<GameObject>("SettingsButton");
        m_VibrationButton = transform.FindDeepChild<GameObject>("VibrationButton");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        GameManager.OnLevelStarted += OnLevelStarted;

        m_TapToStart.Setup(OnTapToStart);
        //m_TapToStart.Setup();


        SetSettingsButton();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        GameManager.OnLevelStarted -= OnLevelStarted;
    }
    

    #region Events
    private void OnLevelStarted()
    {
        Close();
    }

    private void OnTapToStart()
    {
        GameManagerBase.Instance.StartGame();
    }
    #endregion

    public override void Reset()
    {
        base.Reset();

        SetSettingsButton();
    }

    private void SetSettingsButton()
    {
        m_SettingsButton.SetActive(GameConfig.Instance.Menus.UseScreenSettings);
        m_VibrationButton.SetActive(!GameConfig.Instance.Menus.UseScreenSettings);
    }
}
