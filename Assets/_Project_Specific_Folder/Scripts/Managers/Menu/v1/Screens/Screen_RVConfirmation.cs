using System;
using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;

public class Screen_RVConfirmation : MenuScreenBase
{
    [SerializeField, ReadOnly] private ExtendedButton m_AcceptButton;
    [SerializeField, ReadOnly] private ExtendedButton m_CloseButton;

    private Action m_OnAccept = delegate {  };
    private Action m_OnCancel = delegate {  };
    
    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_AcceptButton = transform.FindDeepChild<ExtendedButton>("Accept Button");
        m_CloseButton = transform.FindDeepChild<ExtendedButton>("CloseButton");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_AcceptButton.Setup(accept);
        m_CloseButton.Setup(cancel);
    }

    public void Open(Action i_OnAccept, Action i_OnCancel)
    {
        Open(true);
        Setup(i_OnAccept, i_OnCancel);
    }

    public void Setup(Action i_OnAccept, Action i_OnCancel)
    {
        m_OnAccept = i_OnAccept;
        m_OnCancel = i_OnCancel;
    }

    private void accept()
    {
        m_OnAccept?.Invoke();
        Close();
    }

    private void cancel()
    {
        m_OnCancel?.Invoke();
        Close();
    }
}
