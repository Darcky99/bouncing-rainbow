using KobGamesSDKSlim;
using KobGamesSDKSlim.Collectable;
using KobGamesSDKSlim.MenuManagerV1;
using System.Collections;
using UnityEngine;

public class Screen_Circles : MenuScreenBase
{
    private CircleLevel m_CircleLevel;
    public CircleLevel CircleLevel { get { return m_CircleLevel; } }

    #region Dependencies
    private AdsManager m_AdsManager => AdsManager.Instance;
    private LevelManager m_LevelManager => LevelManager.Instance;
    private StorageManager m_StorageManager => StorageManager.Instance;
    private GamePlayVariablesEditor m_GameplayVariables => GameConfig.Instance.GamePlay;
    #endregion

    #region Init
    protected override void OnEnable()
    {
        base.OnEnable();
        
        m_CircleLevel = (CircleLevel)m_LevelManager.CurrentLevel;

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

    [SerializeField] private UIButton m_CircleButton;
    [SerializeField] private UIButton m_GatesButton;
    [SerializeField] private UIButton m_NextLevelButton;
    [SerializeField] private TapToSpeed m_TapToSpeed;

    private void updateButtons() 
    {
        m_CircleButton.SetState(m_StorageManager.CoinsAmount >= m_GameplayVariables.CurrentCirclePrice && m_CircleLevel.CanAddCircle()
            ? e_State.Normal : m_CircleLevel.CanAddCircle() ? e_State.RV : e_State.Completed);

        m_GatesButton.SetState((m_StorageManager.CoinsAmount >= m_GameplayVariables.CurrentGatePrice && m_CircleLevel.CanAddGate()) 
            ? e_State.Normal : m_CircleLevel.CanAddGate() ? e_State.RV : e_State.Completed);

        m_NextLevelButton.SetState(m_StorageManager.CoinsAmount >= m_GameplayVariables.CirclesNextLevelPrice
            ? e_State.Normal : e_State.Disabled);

        checkTutorials();
    }
    private void checkTutorials()
    {
        if ((m_CircleLevel.EnabledGatesCount < 1 && m_GatesButton.State == e_State.RV) || m_CircleLevel.CircleCount < 1)
            m_GatesButton.SetState(e_State.Disabled);

        m_CircleButton.SetTutorial(m_CircleLevel.CircleCount < 1);

        m_GatesButton.SetTutorial(m_CircleLevel.CircleCount > 1 && m_CircleLevel.EnabledGatesCount < 1 && m_GatesButton.State == e_State.Normal);

        m_NextLevelButton.SetTutorial(m_CircleLevel.CanAddCircle() == false && m_CircleLevel.CanAddGate() == false && m_NextLevelButton.State == e_State.Normal);

        m_TapToSpeed.SetTutorial(m_CircleLevel.CircleCount >= 1 && m_StorageManager.CurrentLevel <= 2);
    }

    #region Money per second display
    [SerializeField] private MoneyPerSecondDisplay m_MoneyPerSecondDisplay;

    private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(1f);
    private IEnumerator updateText()
    {
        while (true)
        {
            m_MoneyPerSecondDisplay.SetIncomePerSecond(m_CircleLevel.IncomePerSecond);
            yield return m_WaitForSeconds;
        }
    }
    #endregion
}