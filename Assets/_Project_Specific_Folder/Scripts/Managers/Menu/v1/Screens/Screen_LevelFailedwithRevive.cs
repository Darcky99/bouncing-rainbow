using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using UnityEngine.UI;
using DG.Tweening;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using KobGamesSDKSlim.Animation;
using TMPro;

public class Screen_LevelFailedwithRevive : MenuScreenBase
{
    [SerializeField] private TextMeshProUGUI[] m_LevelNumber;

    [SerializeField, ReadOnly] private Image m_RVCircle;
    private Tween m_CircleTween;

    [SerializeField, ReadOnly] private TextMeshProUGUI m_ReviveTime;
    [SerializeField, ReadOnly] private Scaler m_ReviveTimeScaler;


    [SerializeField, ReadOnly] private ExtendedButton_RV m_ReviveButton;
    [SerializeField, ReadOnly] private ExtendedButton_FadeAnim m_NoThanks;

    private float m_CurrReviveDuration;

    private TweenData m_NoThanksTween => new TweenData(m_MenuVars.TimeToShowNoThanks, m_MenuVars.ShowNoThanksDuration, m_MenuVars.ShowNoThanksEase, 0);

    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_RVCircle = transform.FindDeepChild<Image>("RV Circle");

        m_ReviveTime = transform.FindDeepChild<TextMeshProUGUI>("Revive Time");
        m_ReviveTimeScaler = m_ReviveTime.GetComponent<Scaler>();

        m_ReviveButton = transform.FindDeepChild<ExtendedButton_RV>("Revive Button");
        m_NoThanks = transform.FindDeepChild<ExtendedButton_FadeAnim>("No Thanks Button");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_ReviveButton.Setup(RewardVideoButtonCallback, ReviveSuccess, ReviveFailed, RewardVideoPlacementIDs.LevelFailedRevive_ReviveRV);
        m_NoThanks.Setup(m_NoThanksTween, OnNoThanksButtonClick);


        m_CurrReviveDuration = m_MenuVars.TimeToAllowRevive;

        m_RVCircle.fillAmount = 1;
        m_ReviveTime.text = m_CurrReviveDuration.ToString();

        m_CircleTween = DOTween.To(() => m_CurrReviveDuration, x => m_CurrReviveDuration = x, 0, m_CurrReviveDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(()=>
            {
                m_RVCircle.fillAmount = m_CurrReviveDuration / m_MenuVars.TimeToAllowRevive;
                string nextNumber = Mathf.Ceil(m_CurrReviveDuration).ToString();
                if(nextNumber != m_ReviveTime.text)
                    m_ReviveTimeScaler.StartAnimation();
                m_ReviveTime.text = nextNumber;
            })
            .OnComplete(GameOver);

        foreach (var levelNumber in m_LevelNumber)
        {
            levelNumber.text = "LEVEL " + StorageManager.Instance.CurrentLevel;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }


    private void RewardVideoButtonCallback(eRewardVideoCallResult i_Result)
    {
        switch (i_Result)
        {
            case eRewardVideoCallResult.Success:
                SetButtonsInteractivityOff(true);
                m_CircleTween.Pause();
                break;
            case eRewardVideoCallResult.OpenedRVConfirmationScreen:
                SetButtonsInteractivityOff();
                m_CircleTween.Pause();
                break;
        }
    }

    private void ReviveSuccess()
    {
        m_CircleTween.Kill();
        GameManager.Instance.LevelContinue();
        Close();
    }

    private void ReviveFailed()
    {
        m_CircleTween.Play();
        SetButtonsInteractivityOn();
    }

    private void OnNoThanksButtonClick()
    {
        GameOver();
    }

    private void GameOver()
    {
        ResetButtonsState();

        GameManager.Instance.GameOver(true);
        m_CircleTween?.Kill();
        Close();
    }
}
