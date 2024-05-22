using System.Reflection;
using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class DOTWeenIDsBase
{
    public const int RateUsRays = -100;
    public const int NoKillOnReset = -90;
    public const int Default = -80;

    //This ids won't be killed by OnGameReset
    public static object[] IdsBase = { RateUsRays, NoKillOnReset};


    private static object[] m_IdsToExcludeFromGameReset = new object[0];
    public static object[] IdsToExcludeFromGameReset { get 
        {
            //caching array
            if(m_IdsToExcludeFromGameReset.Length == 0)
            {
                List<object> idsToExclude = new List<object>();

                idsToExclude.AddRange(DOTweenIDs.IdsBase);
                idsToExclude.AddRange(DOTweenIDs.Ids);

                m_IdsToExcludeFromGameReset = idsToExclude.ToArray();

                //Checking if there isn't any conflict IDs set by the Dev
#if UNITY_EDITOR
                FieldInfo[] fields = DotweenIDsFields;

                for (int i = 0; i < fields.Length; i++)
                {
                    for (int j = i + 1; j < fields.Length; j++)
                    {
                        //Debug.LogError(fields[i].Name + "       " + fields[i].GetValue(null) + "        " + fields[j].Name + "       " + fields[j].GetValue(null));

                        if ((int)fields[i].GetValue(null) == (int)fields[j].GetValue(null))
                            Debug.LogError("DotweenIDs have duplicated IDs     " + fields[i].Name + "    " + fields[j].Name + "    " + fields[i].GetValue(null));
                    }
                }
#endif
            }

            return m_IdsToExcludeFromGameReset;
        } }


    //Use this to show Editor Dropdown with names like it was an Enum
    public static ValueDropdownList<int> ValuesDropdownData { get 
        {
            ValueDropdownList<int> ValueDropdownList = new ValueDropdownList<int>();

            FieldInfo[] fields = DotweenIDsFields;
            for (int i = 0; i < fields.Length; i++)
            {
                ValueDropdownList.Add(fields[i].Name, (int)fields[i].GetValue(null));
            }

            return ValueDropdownList;
        } }

    private static FieldInfo[] DotweenIDsFields { get 
        {
            List<FieldInfo> allFields = new List<FieldInfo>();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

            allFields.AddRange(typeof(DOTweenIDs).GetFields(flags));
            allFields.AddRange(typeof(DOTWeenIDsBase).GetFields(flags));

            for (int i = allFields.Count - 1; i >= 0; i--)
            {
                if (allFields[i].FieldType != typeof(int))
                    allFields.RemoveAt(i);
            }

            return allFields.ToArray();
        }
    }
}
