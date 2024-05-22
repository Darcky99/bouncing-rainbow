using KobGamesSDKSlim;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UIButtons
{
    [SerializeField] private ExtendedButton m_ExtendedButton;
    [SerializeField] private GameObject m_NormalRoot;
    [SerializeField] private GameObject m_RVRoot;
    [SerializeField] private GameObject m_DisabledRoot;
    [SerializeField] protected TextMeshProUGUI[] m_MainTexts;
    [SerializeField] private TextMeshProUGUI[] m_PriceTexts;
    [SerializeField] private GameObject m_TutorialHand;
    [SerializeField] private e_State m_State;
    public e_State State { get { return m_State; } }

    public UIButtons(ExtendedButton i_Button, e_State i_InitialState)
    {
        m_ExtendedButton = i_Button;
        m_State = i_InitialState;
        m_MainTexts = new TextMeshProUGUI[3];
        m_PriceTexts = new TextMeshProUGUI[3];

        m_NormalRoot = i_Button.transform.GetChild(0).gameObject;
        m_RVRoot = i_Button.transform.GetChild(1).gameObject;
        m_DisabledRoot = i_Button.transform.GetChild(2).gameObject;

        m_MainTexts[0] = m_NormalRoot.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        m_MainTexts[1] = m_RVRoot.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        m_MainTexts[2] = m_DisabledRoot.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();

        m_PriceTexts[0] = m_NormalRoot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        m_PriceTexts[1] = m_RVRoot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        m_PriceTexts[2] = m_DisabledRoot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
    }

    #region Initialize
    public void Setup(UnityAction i_OnClick)
    {
        m_ExtendedButton.gameObject.SetActive(true);
        m_ExtendedButton.ResetState();
        m_ExtendedButton.Setup(i_OnClick);
    }
    #endregion

    #region Text
    public void SetPriceText(int i_Price)
    {
        foreach (TextMeshProUGUI i_priceText in m_PriceTexts)
            i_priceText.text = "$" + i_Price.ToString();
    }
    #endregion

    #region State
    public void SetState(e_State i_State)
    {
        m_State = i_State;

        m_NormalRoot.SetActive(i_State == e_State.Normal);
        m_RVRoot.SetActive(i_State == e_State.RV);
        m_DisabledRoot.SetActive(i_State == e_State.Disabled);

        m_ExtendedButton.interactable = i_State != e_State.Disabled;
    }
    public void SetTutorial(bool i_IsActive)
    {

    }
    #endregion

    #region Complete
    public void SetComplete()
    {
        m_ExtendedButton.gameObject.SetActive(false);
    }
    #endregion
}
