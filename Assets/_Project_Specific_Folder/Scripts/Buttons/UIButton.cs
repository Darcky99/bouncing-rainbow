using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField, ReadOnly] private ExtendedButton m_ExtendedButton;
    [SerializeField, ReadOnly] private TextMeshProUGUI[] m_PriceTexts;
    [SerializeField, ReadOnly] protected GameObject m_EnabledRoot;
    [SerializeField, ReadOnly] protected GameObject m_RVsRoot;
    [SerializeField, ReadOnly] protected GameObject m_DisabledRoot;
    [SerializeField, ReadOnly] private Transform m_TutorialHand;

    [Button]
    protected virtual void setRefs()
    {
        m_ExtendedButton = GetComponent<ExtendedButton>();
        m_EnabledRoot = transform.GetChild(0).gameObject;
        m_RVsRoot = transform.GetChild(1).gameObject;
        m_DisabledRoot = transform.GetChild(2).gameObject;

        m_PriceTexts = new TextMeshProUGUI[3];
        
        m_PriceTexts[0] = m_EnabledRoot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>(true);
        m_PriceTexts[1] = m_RVsRoot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>(true);
        m_PriceTexts[2] = m_DisabledRoot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>(true);

        m_TutorialHand = transform.GetChild(3);
    }

    protected StorageManager m_StorageManager => StorageManager.Instance;
    protected GamePlayVariablesEditor m_GamePlayVariables => GameConfig.Instance.GamePlay;

    protected virtual void OnEnable()
    {
        m_ExtendedButton.Setup(processClick);
    }
    protected virtual void OnDisable()
    {
        
    }

    private void processClick()
    {
        switch (m_State)
        {
            case e_State.Normal:
                onClick();
                break;
            case e_State.RV:
                AdsManager.Instance.ShowRewardVideo(onRVSuccess);
                break;
            case e_State.Disabled:
                break;
            default:
                break;
        }
    }

    protected virtual void onClick()
    {

    }
    protected virtual void onRVSuccess()
    {

    }
    protected virtual void purchase()
    {

    }

    #region Price
    protected void setPrice(int i_Price)
    {
        foreach (var price in m_PriceTexts)
        {
            price.SetText($"${i_Price}");
        }
    }
    #endregion

    #region State
    [ShowInInspector] private e_State m_State;
    public e_State State { get { return m_State; } }

    public void SetState(e_State i_State)
    {
        m_State = i_State;

        m_EnabledRoot.SetActive(m_State == e_State.Normal);
        m_RVsRoot.SetActive(m_State == e_State.RV);
        m_DisabledRoot.SetActive(m_State == e_State.Disabled);
    }
    public void SetTutorial(bool i_IsActive)
    {
        m_TutorialHand.gameObject.SetActive(i_IsActive);
    }
    #endregion
}
