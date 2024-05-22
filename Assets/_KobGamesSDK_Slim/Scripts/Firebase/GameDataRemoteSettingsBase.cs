using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [Serializable]
    public abstract class GameDataRemoteSettingsBase
    {
        [HideInInspector]
        public Action OnValueChangedEditorCallback;

        public abstract void LoadFromGameConfiguration();

        public virtual void Init(string i_JsonString = null, Action i_OnValueChangedCallback = null)
        {
            LoadFromGameConfiguration();

            if (i_JsonString != null)
            {
                SetJson(i_JsonString);
            }
        }

        public virtual void SetJson(string i_JsonString)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(i_JsonString, this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(GameDataRemoteSettingsBase)}-{Utils.GetFuncName()}-JsonString error: {ex.Message}");
            }
        }

        [ShowInInspector, MultiLineProperty(10), BoxGroup("Game Data Config"), OnValueChanged(nameof(OnValueChangedEditor), true)]
        public string SettingsJsonParsedPretty { get => JsonUtility.ToJson(this, true); set => SetJson(value); }

        [HideInInspector]
        public string SettingsJsonParsedNonPretty { get => JsonUtility.ToJson(this, false); }       

        public void OnValueChangedEditor()
        {
#if UNITY_EDITOR
            //Debug.LogError("ValueChanged from editing SettingsJsonParsedPretty");

            OnValueChangedEditorCallback.InvokeSafe();
#endif
        }

        #region InspectorOnFocus
#if UNITY_EDITOR
        public void OnFocus()
        {
            LoadFromGameConfiguration();
            OnValueChangedEditor();
        }

        private static bool hasFocused = false;

        [OnInspectorGUI]
        private void myInspectorGUI()
        {
            if (hasFocused == false)
            {
                hasFocused = true;

                OnFocus();

                //Debug.LogError("FOCUS " + UnityEditor.Selection.activeObject);
            }
        }

        private static void onSelectionChanged()
        {
            //Debug.LogError("SEL CHANGED " + UnityEditor.Selection.activeObject);

            hasFocused = false;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //Debug.LogError("OnScriptsReloaded");

            UnityEditor.Selection.selectionChanged += onSelectionChanged;
        }
#endif
        #endregion
    }
}