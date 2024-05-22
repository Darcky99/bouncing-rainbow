using System;
using UnityEditor;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [InitializeOnLoad]
    public class GameSettingsHelper
    {
        [NonSerialized]
        private static bool m_IsFocused = false;
        static GameSettingsHelper()
        {
            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (Selection.activeObject != null && Selection.activeObject.name.Equals(nameof(GameSettings)))
            {
                if (!m_IsFocused)
                {
                    m_IsFocused = true;
                    //Debug.LogError("GameSettingsHelper Focused");
                    GameSettings.Instance.OnFocus();
                }
            }
            else
            {
                m_IsFocused = false;
            }
        }
    }
}