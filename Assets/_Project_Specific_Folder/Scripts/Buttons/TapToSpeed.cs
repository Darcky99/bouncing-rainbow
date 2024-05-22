using DG.Tweening;
using KobGamesSDKSlim;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapToSpeed : MonoBehaviour
{
    [SerializeField] private float m_Delay;
    [SerializeField] private Transform m_Root;
    public bool IsCompleted;

    private StorageManager m_StorageManager => StorageManager.Instance;

    private void OnEnable()
    {
        IsCompleted = false;
    }

    public void SetTutorial(bool i_Enable)
    {
        if (IsCompleted == false)
        {
            m_Root.gameObject.SetActive(i_Enable);
            
            if(i_Enable)
                completeTutorial();
        }
    }
    private void completeTutorial()
    {
        IsCompleted = true;
        DOVirtual.DelayedCall(m_Delay, () => m_Root.gameObject.SetActive(false));
    }
}