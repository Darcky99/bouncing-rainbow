using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace KobGamesSDKSlim
{
    public class TriggerCheckBase : MonoBehaviour
    {
        public eTags[] TagsToCheck;

        [ReadOnly] public bool IsActive = false;

        [ReadOnly] public Action OnIsActive;
        [ReadOnly] public Action OnIsNotActive;

        [ReadOnly] public List<GameObject> ObjectsInsideTrigger = new List<GameObject>();

        public void Reset()
        {
            IsActive = false;
            ObjectsInsideTrigger.Clear();
        }

        public void OnTriggerEnter(Collider i_Collider)
        {
            if (CheckTag(i_Collider.gameObject.tag))
            {
                if (!IsActive)
                {
                    IsActive = true;
                    if (!ObjectsInsideTrigger.Contains(i_Collider.gameObject))
                        ObjectsInsideTrigger.Add(i_Collider.gameObject);

                    OnIsActive.InvokeSafe();
                }
            }
        }

        public void OnTriggerStay(Collider i_Collider)
        {
            if (CheckTag(i_Collider.gameObject.tag))
            {
                if (!IsActive)
                {
                    IsActive = true;
                    if (!ObjectsInsideTrigger.Contains(i_Collider.gameObject))
                        ObjectsInsideTrigger.Add(i_Collider.gameObject);

                    OnIsActive.InvokeSafe();
                }
            }
        }

        public void OnTriggerExit(Collider i_Collider)
        {
            if (CheckTag(i_Collider.gameObject.tag))
            {
                if (IsActive)
                {
                    IsActive = false;
                    //Debug.LogError(gameObject.name + "  OnTriggerExit");
                    if (ObjectsInsideTrigger.Contains(i_Collider.gameObject))
                        ObjectsInsideTrigger.Remove(i_Collider.gameObject);

                    OnIsNotActive.InvokeSafe();
                }
            }
        }

        private bool CheckTag(string i_Tag)
        {
            for (int i = 0; i < TagsToCheck.Length; i++)
            {
                if (TagsToCheck[i].ToString() == i_Tag)
                    return true;
            }

            return false;
        }
    }
}
