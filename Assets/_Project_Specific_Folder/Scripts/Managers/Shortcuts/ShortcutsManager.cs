using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KobGamesSDKSlim;

public class ShortcutsManager : ShortcutsManagerBase
{
#if UNITY_EDITOR
    protected override void OnInspectorGUI()
    {
        //keep null so it doesn't render the Base warning
    }
#endif

#if ENABLE_SHORTCUTS

    /// <summary>
    /// You should add the behaviour for the custom shortcuts you created 
    /// </summary>
    /// <param name="i_Type"></param>
    protected override void triggerShortcut(string i_Type)
    {
        base.triggerShortcut(i_Type);

        switch (i_Type)
        {
            //case ShortcutsActions.Example: Behaviour();
            //    break;
            default:
                break;
        }
    }
#endif
}
