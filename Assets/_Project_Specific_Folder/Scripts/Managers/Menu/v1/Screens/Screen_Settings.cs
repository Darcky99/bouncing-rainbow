using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using UnityEngine.UI;
using KobGamesSDKSlim.GameManagerV1;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using KobGamesSDKSlim.UI;

public class Screen_Settings : MenuScreenBase
{
    [SerializeField, ReadOnly] private ToggleButton2[] m_Toggles;
    [SerializeField, ReadOnly] private ExtendedButton m_CloseButton;
    [SerializeField, ReadOnly] private ExtendedButton m_RestorePurchaseButton;

    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_Toggles = GetComponentsInChildren<ToggleButton2>();
        m_CloseButton = transform.FindDeepChild<ExtendedButton>("CloseButton");
        m_RestorePurchaseButton = transform.FindDeepChild<ExtendedButton>("Restore Purchase Button");
    }

    protected override void Awake()
    {
        base.Awake();

#if UNITY_IOS && UNITY_PURCHASING
        m_RestorePurchaseButton.gameObject.SetActive(true);
        
#else
        m_RestorePurchaseButton.gameObject.SetActive(false);
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_CloseButton.Setup(Close);
        m_RestorePurchaseButton.Setup(onRestorePurchaseClick);
    }

    public override void Close()
    {
        base.Close();
        MenuManager.Instance.OpenMenuScreen(nameof(Screen_Welcome), true);
        HUDManager.Instance.Open();
    }

    private void onRestorePurchaseClick()
    {
#if UNITY_PURCHASING
        iAPManager.Instance.InititateRestore();
#endif
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < m_Toggles.Length; i++)
        {
            if (i == 0) m_Toggles[i].Set("MUSIC", !SoundManager.Instance.IsMusicMuted, (i_State) => SoundManager.Instance.SetMusicMute(!i_State));
            if (i == 1) m_Toggles[i].Set("SFX", !SoundManager.Instance.IsSFXMuted, (i_State) => SoundManager.Instance.SetSFXMute(!i_State));
            //TODO - This should go through HapticsManager instead
            if (i == 2) m_Toggles[i].Set("HAPTICS", StorageManager.Instance.IsVibrationOn, (i_State) => StorageManager.Instance.IsVibrationOn = i_State);

        }
    }
}
