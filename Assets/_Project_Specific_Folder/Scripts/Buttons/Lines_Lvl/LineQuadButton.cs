using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LineQuadButton : UIButton
{
    private SquadsLevel m_SquadsLevel;
    [SerializeField] private TextMeshProUGUI[] m_TitleTexts;

    protected override void setRefs()
    {
        base.setRefs();

        m_TitleTexts = new TextMeshProUGUI[3];

        m_TitleTexts[0] = m_EnabledRoot.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>(true);
        m_TitleTexts[1] = m_RVsRoot.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>(true);
        m_TitleTexts[2] = m_DisabledRoot.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_SquadsLevel = (SquadsLevel)LevelManager.Instance.CurrentLevel;
        //instead, call set functionality and there call setPrice.
        setFunctionality();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void setFunctionality()
    {
        if (m_SquadsLevel.CanAddLine)
        {
            //set all to line: title, price, 
            setTitle("ADD \n LINE");
        }
        else if (m_SquadsLevel.CanAddSquad)
        {
            //set al to squad: title, price, 
            setTitle("ADD \n SQUAD");
        }
        else
        {
            //disable!
        }

        setPrice(m_GamePlayVariables.CurrentLineOrQuadPrice);
    }
    private void setTitle(string i_NewTitle)
    {
        foreach (TextMeshProUGUI i_text in m_TitleTexts)
            i_text.text = i_NewTitle;
    }

    protected override void onClick()
    {
        base.onClick();

        if (m_StorageManager.CoinsAmount < m_GamePlayVariables.CurrentLineOrQuadPrice)
        {
            Debug.LogError("Trying to buy with not enouch money.");
            return;
        }

        purchase();
    }
    protected override void onRVSuccess()
    {
        base.onRVSuccess();

        if (m_SquadsLevel.CanAddSomething == false)
        {
            Debug.LogError("Trying to add somethign without enough space.");
            return;
        }

        m_SquadsLevel.AddLineOrQuad();

        setFunctionality();
    }
    protected override void purchase()
    {
        base.purchase();

        int i_cost = m_GamePlayVariables.CurrentLineOrQuadPrice;

        m_SquadsLevel.AddLineOrQuad();

        setFunctionality();

        m_StorageManager.CoinsAmount -= i_cost;
    }
}