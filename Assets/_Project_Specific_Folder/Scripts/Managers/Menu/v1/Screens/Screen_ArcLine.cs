using KobGamesSDKSlim;
using KobGamesSDKSlim.Collectable;
using KobGamesSDKSlim.MenuManagerV1;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class Screen_ArcLine : MenuScreenBase
{
    private SquadsLevel m_SquadsManager;

    #region Dependencies
    private AdsManager m_AdsManager => AdsManager.Instance;
    private StorageManager m_StorageManager => StorageManager.Instance;
    private GamePlayVariablesEditor m_GameplayVariables => GameConfig.Instance.GamePlay;
    #endregion

    #region Init
    protected override void OnEnable()
    {
        base.OnEnable();
        
        m_SquadsManager = (SquadsLevel) LevelManager.Instance.CurrentLevel;

        StartCoroutine(updateText());

        StorageManager.OnCollectableAmountChanged += onCollectableAmountChanged;

        updateButtons();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();

        StorageManager.OnCollectableAmountChanged -= onCollectableAmountChanged;
    }
    #endregion

    #region Callbacks
    private void onCollectableAmountChanged(eCollectableType i_CollectableType, int i_EarnAmount, int i_ResultAmount)
    {
        updateButtons();
    }
    #endregion

    [SerializeField] private UIButton m_LineSquadButton;
    [SerializeField] private UIButton m_IncomeButton;
    [SerializeField] private UIButton m_NextLevelButton;
    [SerializeField] private TapToSpeed m_TapToSpeed;

    private void updateButtons()
    {
        m_LineSquadButton.SetState(m_StorageManager.CoinsAmount >= m_GameplayVariables.CurrentLineOrQuadPrice && m_SquadsManager.CanAddSomething
            ? e_State.Normal : m_SquadsManager.CanAddSomething ? e_State.RV : e_State.Completed);

        m_IncomeButton.SetState(m_StorageManager.CoinsAmount >= m_GameplayVariables.CurrentIncomePrice
            ? e_State.Normal : e_State.RV);

        m_NextLevelButton.SetState(m_StorageManager.CoinsAmount >= m_GameplayVariables.SquadsNextLevelPrice
            ? e_State.Normal : e_State.Disabled);

        checkTutorials();
    }
    private void checkTutorials()
    {
        if(m_SquadsManager.LineCount < 1)
            m_IncomeButton.SetState(e_State.Disabled);

        m_LineSquadButton.SetTutorial(m_SquadsManager.LineCount < 1);

        m_IncomeButton.SetTutorial(m_SquadsManager.LineCount >= 1 && m_SquadsManager.IncomeLevel < 1 && m_IncomeButton.State == e_State.Normal);

        m_NextLevelButton.SetTutorial(m_SquadsManager.IsFull && m_StorageManager.CoinsAmount >= m_GameplayVariables.SquadsNextLevelPrice);

        m_TapToSpeed.SetTutorial(m_SquadsManager.LineCount >= 1 && m_StorageManager.CurrentLevel <= 2);
    }

    #region Money per second display
    [SerializeField] private MoneyPerSecondDisplay m_MoneyPerSecondDisplay;

    private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(1f);
    private IEnumerator updateText()
    {
        while (true)
        {
            yield return m_WaitForSeconds;
            m_MoneyPerSecondDisplay.SetIncomePerSecond(m_SquadsManager.IncomePerSecond);
        }
    }
    #endregion
}