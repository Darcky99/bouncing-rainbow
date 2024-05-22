using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
namespace KobGamesSDKSlim.Debugging
{
    public class GameLoopDebugButtonManager : MonoBehaviour
    {
        [SerializeField] private Button SwitchButtonsState;
        [SerializeField] private Button HideButton;

        [SerializeField] private Text SwitchButtonsStateText;

        [SerializeField] private GameObject ButtonsParent;

        private bool m_ShowButtons = false;


        private void OnEnable()
        {
            SwitchButtonsState.onClick.AddListener(() => { SetButtonsState(!m_ShowButtons); });
            HideButton.onClick.AddListener(() => { GameConfig.Instance.Debug.ShowGameLoopButtons = false; });


            SetButtonsState(m_ShowButtons);
        }
        private void OnDisable()
        {
            SwitchButtonsState.onClick.RemoveAllListeners();
            HideButton.onClick.RemoveAllListeners();
        }


        private void SetButtonsState(bool i_State)
        {
            m_ShowButtons = i_State;

            //SwitchButtonsStateText.text = m_ShowButtons ? "Close" : "Open";
        }

        void Update()
        {
            bool ShowState = (GameManager.Instance.GameState == eGameState.Playing || GameManager.Instance.GameState == eGameState.Idle) && GameConfig.Instance.Debug.ShowGameLoopButtons;

            SwitchButtonsState.gameObject.SetActive(ShowState);
            HideButton.gameObject.SetActive(ShowState);
            ButtonsParent.SetActive(ShowState && m_ShowButtons);
        }
    }
}

