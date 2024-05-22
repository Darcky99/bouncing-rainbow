using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using KobGamesSDKSlim.MenuManagerV1;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KobGamesSDKSlim
{
#if ENABLE_SHORTCUTS
    public class ShortcutsManagerBase : Singleton<ShortcutsManager>
#else
    public class ShortcutsManagerBase : Singleton<ShortcutsManagerBase>
#endif
    {
#if UNITY_EDITOR
        [OnInspectorGUI]
        protected virtual void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();

            style.fontSize = 15;
            style.normal.textColor = Color.yellow;
            style.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("WARNING", style);

            EditorGUILayout.Space();

            style.fontSize = 13;
            style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            style.fontStyle = FontStyle.Normal;
            EditorGUILayout.LabelField("<b>ShortcutsManager</b> might be missing. Project update needed!", style);

            EditorGUILayout.Space();
            EditorGUILayout.Space(); 
        }
#endif

#if ENABLE_SHORTCUTS

        private Action m_OnSimulateLevelCompleted;
        private Action m_OnSimulateLevelFailedWithRevive;
        private Action m_OnSimulateLevelFailed;
        private Action m_OnSimulatePrevLevel;
        private Action m_OnSimulateNextLevel;
        private Action m_OnSimulateResetLevel;
        private Action m_OnSimulateResetGame;

        public void SetGameManagerShortcutsEvents(
            Action i_LevelCompleted,
            Action i_LevelFailedWithRevive,
            Action i_LevelFailed,
            Action i_PrevLevel,
            Action i_NextLevel,
            Action i_ResetLevel,
            Action i_ResetGame)
        {
            m_OnSimulateLevelCompleted = i_LevelCompleted;
            m_OnSimulateLevelFailedWithRevive = i_LevelFailedWithRevive;
            m_OnSimulateLevelFailed = i_LevelFailed;
            m_OnSimulatePrevLevel = i_PrevLevel;
            m_OnSimulateNextLevel = i_NextLevel;
            m_OnSimulateResetLevel = i_ResetLevel;
            m_OnSimulateResetGame = i_ResetGame;
        }

        public override void OnDisable()
        {
            m_OnSimulateLevelCompleted =
            m_OnSimulateLevelFailedWithRevive = 
            m_OnSimulateLevelFailed = 
            m_OnSimulatePrevLevel = 
            m_OnSimulateNextLevel = 
            m_OnSimulateResetLevel = 
            m_OnSimulateResetGame = null;
        }


        private void Update()
        {
#if UNITY_EDITOR
            for (int i = 0; i < GameSettings.Instance.Shortcuts.ShortcutsGroup.Length; i++)
            {
                foreach (var item in GameSettings.Instance.Shortcuts.ShortcutsGroup[i].Shortcuts)
                {
                    if (GameSettings.Instance.Shortcuts.ShortcutsGroup[i].GroupName.ToUpper().Contains("FIXED"))
                        continue;

                    bool conditionIsTrue = true;
                    for (int j = 0; j < item.Keys.Length; j++)
                    {
                        bool keyCondition =
                            (Input.GetKeyDown(item.Keys[j].Key) && item.Keys[j].State == eShortcutKeyState.OnDown) ||
                            (Input.GetKeyUp(item.Keys[j].Key) && item.Keys[j].State == eShortcutKeyState.OnUp) ||
                            (Input.GetKey(item.Keys[j].Key) && item.Keys[j].State == eShortcutKeyState.Pressing) ||
                            (!Input.GetKey(item.Keys[j].Key) && item.Keys[j].State == eShortcutKeyState.NotPressing);

                        conditionIsTrue = conditionIsTrue && keyCondition;
                    }

                    if (conditionIsTrue)
                        TriggerShortcut(item.Action);
                }
            }

#endif
        }

        public void TriggerShortcut(string i_Type)
        {
            //If Unity Editor it should always go through. On mobile it will depend on DebugMode
#if !UNITY_EDITOR
            if (!GameConfig.Instance.Debug.DebugMode)
                return;
#endif
            triggerShortcut(i_Type);
        }

        protected virtual void triggerShortcut(string i_Type)
        {
            switch (i_Type)
            {
                case ShortcutsActions.LevelComplete:
                    m_OnSimulateLevelCompleted.InvokeSafe();
                    break;
                case ShortcutsActions.LevelFailedWithRevive:
                    m_OnSimulateLevelFailedWithRevive.InvokeSafe();
                    break;
                case ShortcutsActions.LevelFailed:
                    m_OnSimulateLevelFailed.InvokeSafe();
                    break;
                case ShortcutsActions.PrevLevel:
                    m_OnSimulatePrevLevel.InvokeSafe();
                    break;
                case ShortcutsActions.NextLevel:
                    m_OnSimulateNextLevel.InvokeSafe();
                    break;
                case ShortcutsActions.ResetLevel:
                    m_OnSimulateResetLevel.InvokeSafe();
                    break;
                case ShortcutsActions.ResetGame:
                    m_OnSimulateResetGame.InvokeSafe();
                    break;
                case ShortcutsActions.ShowBanner:
                    AdsManager.Instance.ShowBanner();
                    break;
                case ShortcutsActions.HideBanner:
                    AdsManager.Instance.HideBanner();
                    break;
                case ShortcutsActions.ToggleGameLoopButtons:
                    GameConfig.Instance.Debug.ShowGameLoopButtons = !GameConfig.Instance.Debug.ShowGameLoopButtons;
                    break;
                case ShortcutsActions.ToggleGDPRScreen:
                    ToggleGDPRScreen();
                    break;
                default:
                    break;
            }
        }
#endif

        private void ToggleGDPRScreen()
        {
            if (MenuManager.Instance.GetMenuScreen<Screen_GDPR>().HasValue())
            {
                if (MenuManager.Instance.IsScreenOpened(nameof(Screen_GDPR)))
                {
                    MenuManager.Instance.CloseMenuScreen(nameof(Screen_GDPR));
                }
                else
                {
                    MenuManager.Instance.OpenMenuScreen(nameof(Screen_GDPR));
                }
            }
        }
    }
}
