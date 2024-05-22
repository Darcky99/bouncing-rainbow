using KobGamesSDKSlim;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class MultiDisplayButton : UIButtons
{
    #region Constructor
    public MultiDisplayButton(ExtendedButton i_Button, e_State i_InitialState, e_ButtonDisplay i_InitialDisplay) : base(i_Button, i_InitialState)
    {
        m_ButtonDisplay = i_InitialDisplay;
    }
    #endregion

    #region Display
    private e_ButtonDisplay m_ButtonDisplay;
    public e_ButtonDisplay ButtonDisplay { get { return m_ButtonDisplay; } }
    #endregion

    private void setNameText(string i_NewMainText)
    {
        foreach (TextMeshProUGUI i_mainText in m_MainTexts)
            i_mainText.text = i_NewMainText.ToString();
    }
    public void SetDisplay(e_ButtonDisplay i_ToDisplay, string i_Name)
    {
        m_ButtonDisplay = i_ToDisplay;
        setNameText(i_Name);
    }
}
