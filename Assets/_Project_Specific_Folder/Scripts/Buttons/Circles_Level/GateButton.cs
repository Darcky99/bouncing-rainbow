using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateButton : UIButton
{
    private CircleLevel m_CircleLevel;

    #region Init
    protected override void OnEnable()
    {
        base.OnEnable();
        
        m_CircleLevel = (CircleLevel) LevelManager.Instance.CurrentLevel;
        setPrice(m_GamePlayVariables.CurrentGatePrice);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }
    #endregion

    protected override void onClick()
    {
        base.onClick();

        if(m_StorageManager.CoinsAmount < m_GamePlayVariables.CurrentGatePrice)
        {
            Debug.LogError("Trying to buy with not enouch money.");
            return;
        }

        purchase();
    }
    protected override void onRVSuccess()
    {
        base.onRVSuccess();

        if (m_CircleLevel.CanAddGate() == false)
            return;

        m_CircleLevel.AddGate();

        setPrice(m_GamePlayVariables.CurrentGatePrice);
    }
    protected override void purchase()
    {
        base.purchase();

        int i_cost = m_GamePlayVariables.CurrentGatePrice;

        if (m_CircleLevel.CanAddGate() == false)
            return;

        m_CircleLevel.AddGate();

        setPrice(m_GamePlayVariables.CurrentGatePrice);

        m_StorageManager.CoinsAmount -= i_cost;
    }
}
