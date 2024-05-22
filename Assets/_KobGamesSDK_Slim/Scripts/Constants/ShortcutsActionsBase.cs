using System.Reflection;
using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    public class ShortcutsActionsBase
    {
        //CHANGEABLE
        public const string LevelComplete = "Level Complete";
        public const string LevelFailedWithRevive = "Level Failed With Revive";
        public const string LevelFailed = "Level Failed";
        public const string PrevLevel = "Previous Level";
        public const string NextLevel = "Next Level";
        public const string ResetLevel = "Reset Level";
        public const string ResetGame = "Reset Game";

        public const string ToggleGameLoopButtons = "Toggle GameLoop Buttons";
        public const string ToggleGDPRScreen = "Toggle GDPR Screen";


        public const string ShowBanner = "Show Banner";
        public const string HideBanner = "Hide Banner";

        //FIXED
        public const string EditorPauseToggle = "Editor Pause Toggle";
        public const string OpenGameSettings = "Open Game Settings";
        public const string OpenGameConfig = "Open Game Config";
        public const string SetExtendedHierarchy = "Set Extended Hierarchy";



        //Use this to show Editor Dropdown with names like it was an Enum
        public static ValueDropdownList<string> AllFields { get  
            {
                ValueDropdownList<string> ValueDropdownList = new ValueDropdownList<string>();

                FieldInfo[] fields = Fields;
                for (int i = 0; i < fields.Length; i++)
                {
                    ValueDropdownList.Add(fields[i].Name, (string)fields[i].GetValue(null));
                }

                for (int i = 0; i < ValueDropdownList.Count - 1; i++)
                {
                    for (int j = i + 1; j < ValueDropdownList.Count; j++)
                    {
                        if(ValueDropdownList[i].Value == ValueDropdownList[j].Value)
                        {
                            Debug.LogError("There are Shortcuts with the same ids - " + ValueDropdownList[i].Text + ": " + ValueDropdownList[i].Value + " - " + ValueDropdownList[j].Text + ": " + ValueDropdownList[j].Value);
                        }
                    }
                }

                return ValueDropdownList;
            } }

        public static ValueDropdownList<string> NotUsedFields{ get 
            {
                ValueDropdownList<string> ValueDropdownList = AllFields;
#if ENABLE_SHORTCUTS
                ShortcutsGroup[] shortcuts = GameSettings.Instance.Shortcuts.ShortcutsGroup;

                for (int i = 0; i < shortcuts.Length; i++)
                {
                    if (shortcuts[i] == null)
                    {
                        Debug.LogError("Missing ShortcutGroup Asset. Please check GameSettings -> Shortcuts Tab");
                        continue;
                    }

                    foreach (var item in shortcuts[i].Shortcuts)
                    {
                        for (int j = 0; j < ValueDropdownList.Count; j++)
                        {
                            if (ValueDropdownList[j].Value == item.Action)
                            {
                                ValueDropdownList.RemoveAt(j);
                                break;
                            }
                        }
                    }
                }

                if (ValueDropdownList.Count == 0)
                    ValueDropdownList.Add("Dummy", "Dummy");

#endif
                return ValueDropdownList;
            }
                
        }

        private static FieldInfo[] Fields { get  
            {
                List<FieldInfo> allFields = new List<FieldInfo>();

                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

#if ENABLE_SHORTCUTS
                allFields.AddRange(typeof(ShortcutsActions).GetFields(flags));
#endif
                allFields.AddRange(typeof(ShortcutsActionsBase).GetFields(flags));

                for (int i = allFields.Count - 1; i >= 0; i--)
                {
                    if (allFields[i].FieldType != typeof(string))
                        allFields.RemoveAt(i);
                }

                return allFields.ToArray();
            }
        }
    }
}