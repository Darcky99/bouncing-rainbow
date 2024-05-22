using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomeButton : UIButton
{
    private SquadsLevel m_SquadsLevel;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_SquadsLevel = (SquadsLevel)LevelManager.Instance.CurrentLevel;
        setPrice(m_GamePlayVariables.CurrentIncomePrice);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void onClick()
    {
        base.onClick();

        if(m_StorageManager.CoinsAmount < m_GamePlayVariables.CurrentIncomePrice)
        {
            Debug.LogError("Trying to buy with not enouch money.");
            return;
        }

        purchase();
    }
    protected override void onRVSuccess()
    {
        base.onRVSuccess();

        //if (m_SquadsLevel.CanAddIncome == false) need this condition?
        //    return;

        m_SquadsLevel.IncreaseIncome();

        setPrice(m_GamePlayVariables.CurrentIncomePrice);
    }
    protected override void purchase()
    {
        base.purchase();

        int i_cost = m_GamePlayVariables.CurrentIncomePrice;

        //if (m_SquadsLevel.CanAddIncome == false) need this condition?
        //    return;

        m_SquadsLevel.IncreaseIncome();

        setPrice(m_GamePlayVariables.CurrentIncomePrice);

        m_StorageManager.CoinsAmount -= i_cost;
    }
}