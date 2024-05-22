using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using UnityEngine.UI;
using DG.Tweening;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using KobGamesSDKSlim.UI;
using KobGamesSDKSlim.Animation;
using TMPro;
using KobGamesSDKSlim.Collectable;

public class Screen_LevelCompleted : MenuScreenBase
{
    [SerializeField] private TextMeshProUGUI[] m_LevelNumber;
    [SerializeField] private ParticleSystem[]  m_Confetti;

    [SerializeField, ReadOnly] private Button_ClaimMoney_RV_2 m_ClaimRV;

    [SerializeField, ReadOnly] private ExtendedButton_FadeAnim m_NoThanks;
    [SerializeField, ReadOnly] private CollectableSender       m_CoinSender;

    [SerializeField] private TextMeshProUGUI m_NormalAmountMoneyToReceiveText;

    private int m_NormalAmountMoneyToReceive = 0;

    private TweenData m_NoThanksTween => new TweenData(m_MenuVars.TimeToShowNoThanks, m_MenuVars.ShowNoThanksDuration, m_MenuVars.ShowNoThanksEase, 0);

    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_NormalAmountMoneyToReceiveText = transform.FindDeepChild<TextMeshProUGUI>("Normal Amount to Receive");

        m_ClaimRV = transform.FindDeepChild<Button_ClaimMoney_RV_2>("Claim RV Button");

        m_NoThanks   = transform.FindDeepChild<ExtendedButton_FadeAnim>("No Thanks Button");
        m_CoinSender = m_NoThanks.GetComponent<CollectableSender>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_NoThanks.Setup(m_NoThanksTween, OnNoThanksButtonClick);

        //Dev should fill this in according to the game
        SetNormalAmountMoneyToReceive(90);
        m_CoinSender.Set(m_NormalAmountMoneyToReceive);
        m_ClaimRV.Set(m_NormalAmountMoneyToReceive, onRVButtonCallback, onRVClaimSuccess, onRVClaimFailed, onClaimFinished);

        foreach (var levelNumber in m_LevelNumber)
        {
            levelNumber.text = "LEVEL " + StorageManager.Instance.CurrentLevel;
        }

        playConfetti();

    }

    public void SetNormalAmountMoneyToReceive(int i_MoneyAmount)
    {
        m_NormalAmountMoneyToReceive          = i_MoneyAmount;
        m_NormalAmountMoneyToReceiveText.text = $"+ {i_MoneyAmount}";
    }

    private void playConfetti()
    {
        for (int i = 0; i < m_Confetti.Length; i++)
        {
            m_Confetti[i].Play();
        }
    }

#region Buttons Behaviour

    private void onRVButtonCallback(eRewardVideoCallResult i_Result)
    {
        switch (i_Result)
        {
            case eRewardVideoCallResult.Success:
                SetButtonsInteractivityOff(true);
                break;
            case eRewardVideoCallResult.OpenedRVConfirmationScreen:
                SetButtonsInteractivityOff();
                break;
        }
    }

    private void onRVClaimSuccess()
    {
        ResetButtonsState();
        playConfetti();
    }

    private void onRVClaimFailed()
    {
        SetButtonsInteractivityOn();
    }

    private void OnNoThanksButtonClick()
    {
        ResetButtonsState();

        m_CoinSender.Send(onClaimFinished);
    }

#endregion

#region Anim

    private void onClaimFinished()
    {
        //DOVirtual.DelayedCall(.5f, GameManager.Instance.ResetGame);
        DOVirtual.DelayedCall(.5f, Close);
    }

    protected override void OnScreenCloseEnd()
    {
        base.OnScreenCloseEnd();
        GameManager.Instance.ResetGame();
    }

#endregion
}