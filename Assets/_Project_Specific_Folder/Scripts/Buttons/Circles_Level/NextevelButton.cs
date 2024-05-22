using KobGamesSDKSlim;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class NextevelButton : UIButton
{
    private CircleLevel m_CircleLevel;

    #region Init
    protected override void OnEnable()
    {
        base.OnEnable();

        m_CircleLevel = (CircleLevel)LevelManager.Instance.CurrentLevel;
        setPrice(m_GamePlayVariables.CirclesNextLevelPrice);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }
    #endregion

    protected override void onClick()
    {
        base.onClick();

        if (m_StorageManager.CoinsAmount < m_GamePlayVariables.CirclesNextLevelPrice)
        {
            Debug.LogError("Trying to buy with not enouch money.");
            return;
        }

        purchase();
    }
    protected override void onRVSuccess()
    {
        base.onRVSuccess();

        if (m_CircleLevel.CanAddGate() || m_CircleLevel.CanAddCircle())
            return;

        Debug.LogError("Not posible from here he");
    }
    protected override void purchase()
    {
        base.purchase();

        int i_cost = m_GamePlayVariables.CirclesNextLevelPrice;

        if (m_CircleLevel.CanAddGate() || m_CircleLevel.CanAddCircle())
            return;

        m_StorageManager.CoinsAmount -= i_cost;

        GameManager.Instance.LevelCompleted();
    }
}
