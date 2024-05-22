using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gates : MonoBehaviour
{
    [SerializeField, ReadOnly] private CircleLevel m_CircleLevel;
    [SerializeField, ReadOnly] private Gate[] m_Gates;

    [Button]
    private void setReferences()
    {
        m_CircleLevel = GetComponentInParent<CircleLevel>();
        m_Gates = GetComponentsInChildren<Gate>(true);
    }

    #region Specific
    private void enableGates(int i_EnabledGateCount)
    {
        disableGates();

        for (int i = 0; i < i_EnabledGateCount; i++) {
            m_Gates[i].gameObject.SetActive(true);
            m_Gates[i].SetAngle(m_CircleLevel.SectionInTurns * (i + 1));
        }
    }
    private void disableGates()
    {
        foreach(Gate i_gate in m_Gates) {
            i_gate.gameObject.SetActive(false);
        }
    }
    public void EnableGates(int i_EnabledGateCount)
    {
        enableGates(i_EnabledGateCount);
    }
    public int GetTotalGateCount()
    {
        return m_Gates.Length;
    }
    #endregion
}
