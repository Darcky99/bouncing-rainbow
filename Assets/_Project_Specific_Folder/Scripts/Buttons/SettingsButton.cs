using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;

public class SettingsButton : MonoBehaviour
{
    [SerializeField, ReadOnly] private ExtendedButton m_SettingsButton;

    [Button]
    private void SetRefs()
    {
        m_SettingsButton = GetComponentInChildren<ExtendedButton>();
    }

    private void OnEnable()
    {
        m_SettingsButton.Setup(OnSettingsButtonDown);
    }

    private void OnSettingsButtonDown()
    {
        MenuManager.Instance.CloseMenuScreen(nameof(Screen_Welcome));
        MenuManager.Instance.OpenMenuScreen(nameof(Screen_Settings));
        HUDManager.Instance.Close();
    }
}
