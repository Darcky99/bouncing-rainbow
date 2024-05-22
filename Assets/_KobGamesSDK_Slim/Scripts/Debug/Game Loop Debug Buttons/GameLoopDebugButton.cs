using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace KobGamesSDKSlim.Debugging
{
    public class GameLoopDebugButton : MonoBehaviour
    {
        [SerializeField] private eGameLoopDebugType m_Type;
        [SerializeField, ReadOnly] private ExtendedButton m_Button;
        //[SerializeField] private Text m_Text;

        

        [Button]
        private void SetRefs()
        {
            m_Button = GetComponent<ExtendedButton>();
            m_Button.name = m_Type.ToString(); 
            //m_Text = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            m_Button.onDown += Click;
            //m_Text.text = m_Type.ToString();
        }

        private void OnDisable()
        {
            m_Button.onDown -= Click;
        }

        private void Click()
        {
#if ENABLE_SHORTCUTS
            //Kind of awful but better than having 2 vars for the "same" thing?
            string Action = "";

            switch (m_Type)
            {
                case eGameLoopDebugType.Complete:
                    Action = ShortcutsActions.LevelComplete;
                    break;
                case eGameLoopDebugType.FailedWithRevive:
                    Action = ShortcutsActions.LevelFailedWithRevive;
                    break;
                case eGameLoopDebugType.Failed:
                    Action = ShortcutsActions.LevelFailed;
                    break;
                case eGameLoopDebugType.PrevLevel:
                    Action = ShortcutsActions.PrevLevel;
                    break;
                case eGameLoopDebugType.NextLevel:
                    Action = ShortcutsActions.NextLevel;
                    break;
                case eGameLoopDebugType.ResetLevel:
                    Action = ShortcutsActions.ResetLevel;
                    break;
                case eGameLoopDebugType.ResetGame:
                    Action = ShortcutsActions.ResetGame;
                    break;
                case eGameLoopDebugType.ToggleGDPR:
                    Action = ShortcutsActions.ToggleGDPRScreen;
                    break;
                default:
                    break;
            }

            ShortcutsManager.Instance.TriggerShortcut(Action);
#else
            GameManager.Instance.SimulateGameLoop(m_Type);
#endif
        }
    }
}
