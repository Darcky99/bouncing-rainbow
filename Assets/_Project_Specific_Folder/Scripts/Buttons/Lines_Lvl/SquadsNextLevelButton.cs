using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadsNextLevelButton : UIButton
{
    private SquadsLevel m_SquadsLevel;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_SquadsLevel = (SquadsLevel)LevelManager.Instance.CurrentLevel;
        setPrice(m_GamePlayVariables.SquadsNextLevelPrice);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void onClick()
    {
        base.onClick();

        if (m_StorageManager.CoinsAmount < m_GamePlayVariables.SquadsNextLevelPrice)
        {
            Debug.LogError("Trying to buy with not enouch money.");
            return;
        }

        purchase();
    }
    protected override void onRVSuccess()
    {
        base.onRVSuccess();

        Debug.LogError("RVs not posible on this button");
    }
    protected override void purchase()
    {
        base.purchase();

        int i_cost = m_GamePlayVariables.SquadsNextLevelPrice;

        GameManager.Instance.LevelCompleted();

        m_StorageManager.CoinsAmount -= i_cost;

        MenuManager.Instance.CloseMenuScreen(nameof(Screen_ArcLine));
    }
}