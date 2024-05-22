using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


namespace KobGamesSDKSlim
{
    [Serializable]
    public struct ResourcesObject
    {
#if UNITY_EDITOR
        [SerializeField, OnValueChanged(nameof(ForceUpdatePath)), InlineButton(nameof(ForceUpdatePath), "Force Update Path")]
        private UnityEngine.Object m_Prefab;

        [HideInInspector]
        public UnityEngine.Object Prefab
        {
            get
            {
                return m_Prefab;
            }
            set
            {
                m_Prefab = value;
                ForceUpdatePath();
            }
        }
#endif

        [SerializeField, HideInInspector]
        private string m_FullPath;
        [SerializeField, HideInInspector]
        private string m_Path;
        [ShowInInspector, ReadOnly]
        public string Path
        {
            get
            {
#if UNITY_EDITOR
                if (m_Prefab == null)
                {
                    UnityEngine.Debug.LogError("Prefab is null. Please set it.");
                    return "";
                }
                else
                {
                    if (UnityEditor.AssetDatabase.GetAssetPath(m_Prefab) != m_FullPath)
                    {
                        if (Application.isPlaying)
                            Debug.LogError("ResourcesObjectPath <b>" + m_Prefab.name + "</b> Path is incorrect. Please Update and Save. Hacking it for you during this gameplay session.");

                        ForceUpdatePath();
                    }
                }
#endif
                return m_Path;
            }
            set
            {
                m_Path = value;
            }
        }

#if UNITY_EDITOR
        //[Button]
        private void ForceUpdatePath()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(m_Prefab);

            if (!path.Contains("Resources"))
            {
                UnityEngine.Debug.LogError("Prefab <b>" + m_Prefab.name + "</b> is not inside the Resources folder! This will not work.");
                return;
            }

            m_FullPath = path;
            Path = path
                .Split(new string[] { "Resources" }, StringSplitOptions.None)[1]
                .TrimStart('/')
                .Split('.')[0];
        }
#endif
    }
}