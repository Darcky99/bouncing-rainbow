using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using KobGamesSDKSlim;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using System;

public class ScriptReplacer : MonoBehaviour
{
#if UNITY_EDITOR

    public const string DEFAULT_VALUE = "REMOVE ONLY WHEN NULL";

    [ValueDropdown(nameof(Values))]
    public string m_ScriptToReplaceFor = DEFAULT_VALUE;

    [Button]
    public void Replace()
    {
        int removedScripts = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        if(removedScripts > 0)
        {
            Debug.LogError("Missing script found. Updating with - " + m_ScriptToReplaceFor, gameObject);

            if(m_ScriptToReplaceFor != DEFAULT_VALUE)
            {
                gameObject.AddComponent(Type.GetType(m_ScriptToReplaceFor));
            }

            DestroyImmediate(this);
        }
    }

    public static ValueDropdownList<string> Values
    {
        get
        {
            ValueDropdownList<string> ValueDropdownList = new ValueDropdownList<string>();

            List<MonoScript> types = Resources
           .FindObjectsOfTypeAll(typeof(MonoScript))
           .Where(x => x.GetType() == typeof(MonoScript)) // Fix for Unity crash
           .Cast<MonoScript>()
           .Where(x => x.GetClass() != null && !x.GetClass().IsAbstract && !x.GetClass().IsGenericType)
           .ToList();

            // Ignore any MonoScript types defined by Unity, as it's extremely 
            // unlikely that they could ever be missing
            var editorAssembly = typeof(Editor).Assembly;
            var engineAssembly = typeof(MonoBehaviour).Assembly;
            types.RemoveAll(x => x.GetClass().Assembly == editorAssembly || x.GetClass().Assembly == engineAssembly);

            //Adding values to the list
            ValueDropdownList.Add(DEFAULT_VALUE, DEFAULT_VALUE);
            for (int i = 0; i < types.Count; i++)
            {
                ValueDropdownList.Add(types[i].GetClass().Name, types[i].GetClass().FullName);
            }

            return ValueDropdownList;
        }
    }
#endif
}