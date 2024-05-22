using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using KobGamesSDKSlim.Debugging;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KobGamesSDKSlim
{
    [Serializable]
    public class DictionaryShortcutKeyCombinationGameLoopDebugAction : UnitySerializedDictionary<ShortcutsKeysCombination, eGameLoopDebugType>
    {
    }

    public enum eShortcutKeyState
    {
        OnDown,
        OnUp,
        Pressing,
        NotPressing
    }


    [Serializable]
    public struct ShortcutKey
    {
        public KeyCode Key;
        public eShortcutKeyState State;
    }


    [Serializable]
    public class ShortcutsKeysCombination
    {
        public ShortcutKey[] Keys = new ShortcutKey[0];

        [ValueDropdown(nameof(ShortcutActions))]
        public string Action = "Undefined";

        private ValueDropdownList<string> ShortcutActions()
        {
#if ENABLE_SHORTCUTS

            return ShortcutsActions.NotUsedFields;
#else
            return ShortcutsActionsBase.NotUsedFields;
#endif
        }
    }



    [Serializable]
    public class GameSettings_ShortcutsEditor
    {
        [SerializeField] private bool EditMode = false;

        [Title("Shortcuts"), ShowIf(nameof(EditMode))]
        public ShortcutsGroup[] ShortcutsGroup = new ShortcutsGroup[0];

#if UNITY_EDITOR
        [OnInspectorGUI]
        private void DrawTable()
        {
#if ENABLE_SHORTCUTS
            float keyWidth = 250;
            
            //if (EditMode)
            //    return;

            float colorMultiplier = EditorGUIUtility.isProSkin ? .85f : 1;
            Color normalColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            Color pressingColor = EditorGUIUtility.isProSkin ? Color.green : (Color)new Color32(73, 99, 194, 255);
            Color upColor = EditorGUIUtility.isProSkin ? Color.cyan : (Color)new Color32(201, 45, 200, 255);
            Color notPressingColor = Color.red;

            GUIStyle style = new GUIStyle();
            style.fontSize         = 17;
            style.normal.textColor = normalColor;
            style.fontStyle        = FontStyle.Bold;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("SHORTCUTS TABLE", style);



            for (int i = 0; i < ShortcutsGroup.Length; i++)
            {
                if (ShortcutsGroup[i] == null)
                    continue;

                style.fontStyle = FontStyle.Bold;
                style.fontSize = 15;
                style.richText = true;
                style.normal.textColor = normalColor;

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(ShortcutsGroup[i].GroupName, style);

                EditorGUILayout.Space();


                style.fontSize = 13;
                style.fontStyle = FontStyle.Normal;
                style.normal.textColor = normalColor * colorMultiplier;

                foreach (var item in ShortcutsGroup[i].Shortcuts)
                {
                    string key = "";

                    float changeableMultiplier = ShortcutsGroup[i].GroupName.ToUpper().Contains("FIXED") ? .75f : 1;
                    for (int k = 0; k < item.Keys.Length; k++)
                    {
                        if (k > 0)
                        {
                            key += GetColorFormatedString(" + ", normalColor * colorMultiplier * changeableMultiplier);
                        }

                        Color color = normalColor;
                        switch (item.Keys[k].State)
                        {
                            case eShortcutKeyState.OnUp:
                                color = upColor;
                                break;
                            case eShortcutKeyState.NotPressing:
                                color = notPressingColor;
                                break;
                            case eShortcutKeyState.Pressing:
                                color = pressingColor;
                                break;
                            default:
                                break;
                        }

                        key += GetColorFormatedString(item.Keys[k].Key.ToString(), color * colorMultiplier * changeableMultiplier);
                    }

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(key, style, GUILayout.Width(keyWidth));
                    EditorGUILayout.LabelField(GetColorFormatedString(item.Action, normalColor * colorMultiplier * changeableMultiplier), style);

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            style.normal.textColor = normalColor;
            style.fontSize = 13;
            style.richText = false;
            style.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("LEGEND", style);

            EditorGUILayout.Space();

            style.fontStyle = FontStyle.Bold;
            style.fontSize = 11;

            style.normal.textColor = normalColor * colorMultiplier;
            EditorGUILayout.LabelField("DOWN", style);
            style.normal.textColor = pressingColor * colorMultiplier;
            EditorGUILayout.LabelField("PRESSING", style);
            style.normal.textColor = notPressingColor * colorMultiplier;
            EditorGUILayout.LabelField("NOT PRESSING", style);
            style.normal.textColor = upColor * colorMultiplier;
            EditorGUILayout.LabelField("UP", style);

            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            style.normal.textColor = normalColor;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 13;
            style.richText = true;
            EditorGUILayout.LabelField("HOW TO CUSTOMIZE", style); 

            EditorGUILayout.Space();

            style.fontStyle = FontStyle.Normal;
            style.fontSize  = 11;
            style.wordWrap  = true;

            EditorGUILayout.LabelField(" - Add new <b>Actions</b> on the script <b>ShortcutsActions</b> using the template provided inside the script", style);
            EditorGUILayout.LabelField(" - Inside <b>ShortcutsManager</b> -> <b>triggerShortcut</b> add the new <b>Action</b> and its <b>Functionality</b> to the <b>Switch</b> ", style);
            EditorGUILayout.LabelField(" - Set above bool <b>Edit Mode</b> to true and add the new shortcut on the <b>Project Specific</b> group", style);
#else
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
            EditorGUILayout.LabelField("ENABLE_SHORTCUTS define is not setup. Project update needed!", style);
#endif
        }

        private string GetColorFormatedString(string i_String, Color i_Color)
        {
            string key = "<color=#" + ColorUtility.ToHtmlStringRGBA(i_Color) + ">";
            key += i_String;
            key += "</color>";

            return key;
        }
#endif
    }
}