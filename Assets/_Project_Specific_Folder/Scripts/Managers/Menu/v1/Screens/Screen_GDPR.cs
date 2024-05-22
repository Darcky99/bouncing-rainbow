using DG.Tweening;
using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_GDPR : MenuScreenBase
{
    private GDPRData m_Parameters => GameConfig.Instance.Menus.GDPR;

    [SerializeField, ReadOnly] private ExtendedButton m_TermsOfServiceButton;
    [SerializeField, ReadOnly] private ExtendedButton m_PrivacyPolicyButton;
    [SerializeField, ReadOnly] private ExtendedButton m_AcceptButton;
    [SerializeField, ReadOnly] private TextMeshProUGUI m_TitleText;
    [SerializeField, ReadOnly] private TextMeshProUGUI m_TermsOfServiceLinkText;
    [SerializeField, ReadOnly] private TextMeshProUGUI m_PrivacyPolicyLinkText;
    [SerializeField, ReadOnly] private TextMeshProUGUI m_AgreeConditionText;
    [SerializeField, ReadOnly] private TextMeshProUGUI m_AcceptButtonText;

    public static Action OnAcceptButtonPressed = () => { };

    private const float k_ShowDuration = 0.3f;

    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_TitleText = transform.FindDeepChild<TextMeshProUGUI>("Title");
        m_TermsOfServiceLinkText = transform.FindDeepChild<TextMeshProUGUI>("TermsOfService");
        m_PrivacyPolicyLinkText = transform.FindDeepChild<TextMeshProUGUI>("PrivacyPolicy");
        m_AgreeConditionText = transform.FindDeepChild<TextMeshProUGUI>("AgreeCondition");
        m_AcceptButtonText = transform.FindDeepChild<TextMeshProUGUI>("Accept");
        m_TermsOfServiceButton = transform.FindDeepChild<ExtendedButton>("TermsOfServiceButton");
        m_PrivacyPolicyButton = transform.FindDeepChild<ExtendedButton>("PrivacyPolicyButton");
        m_AcceptButton = transform.FindDeepChild<ExtendedButton>("AcceptButton");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_AcceptButton.Setup(onAcceptButtonPressed);
        m_TermsOfServiceButton.Setup(onTermsOfServiceButtonPressed);
        m_PrivacyPolicyButton.Setup(onPrivacyPolicyButtonPressed);
    }

    public override void Open()
    {
        base.Open();

        m_TitleText.SetText(m_Parameters.Text.TitleText);
        m_PrivacyPolicyLinkText.SetText(m_Parameters.Text.PrivacyPolicyLinkText);
        m_TermsOfServiceLinkText.SetText(m_Parameters.Text.TermsOfServiceLinkText);
        m_AgreeConditionText.SetText(m_Parameters.Text.AgreeConditionText);
        m_AcceptButtonText.SetText(m_Parameters.Text.AcceptButtonText);
    }

    private void onAcceptButtonPressed()
    {
        StorageManager.Instance.IsGDPRConsentGiven = true;

        Close();

        OnAcceptButtonPressed.InvokeSafe();
    }

    private void onPrivacyPolicyButtonPressed()
    {
        Application.OpenURL(m_Parameters.PrivacyPolicyURL);
    }

    private void onTermsOfServiceButtonPressed()
    {
        Application.OpenURL(m_Parameters.TermsOfServiceURL);
    }

   
}
