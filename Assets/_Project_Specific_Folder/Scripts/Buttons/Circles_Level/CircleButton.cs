using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleButton : UIButton
{
    private CircleLevel m_CircleLevel;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_CircleLevel = (CircleLevel)LevelManager.Instance.CurrentLevel;
        setPrice(m_GamePlayVariables.CurrentCirclePrice);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void onClick()
    {
        base.onClick();

        if (m_StorageManager.CoinsAmount < m_GamePlayVariables.CurrentCirclePrice)
        {
            Debug.LogError("Trying to buy with not enouch money.");
            return;
        }

        purchase();
    }
    protected override void onRVSuccess()
    {
        base.onRVSuccess();

        if (m_CircleLevel.CanAddCircle() == false) //This might not even be required.
            return;

        m_CircleLevel.AddCircle();

        setPrice(m_GamePlayVariables.CurrentCirclePrice);
    }
    protected override void purchase()
    {
        base.purchase();

        int i_cost = m_GamePlayVariables.CurrentCirclePrice;

        if (m_CircleLevel.CanAddCircle() == false) //This might not even be required.
            return;

        m_CircleLevel.AddCircle();

        setPrice(m_GamePlayVariables.CurrentCirclePrice);

        m_StorageManager.CoinsAmount -= i_cost;
    }
}