using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        [ShowInInspector]
        private static HashSet<string> PersistentGameObjects = new HashSet<string>();

        private string m_Key;
        void Awake()
        {
            m_Key = string.Format("{0}_{1}", this.gameObject.name, this.gameObject.transform.GetSiblingIndex());

            if (!PersistentGameObjects.Contains(m_Key))
            {
                PersistentGameObjects.Add(m_Key);
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            PersistentGameObjects.Clear();
        }
    }
}