using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SquareLevel : MonoBehaviour
{

    #region Init
    private void OnEnable()
    {
        startUpdateGlobalTime();
    }
    private void OnDisable()
    {
        stopUpdateGlobalTime();
    }
    #endregion

    #region Specific

    #region SquareData
    [Header("Squares configuration")]
    [SerializeField] private Gradient m_Gradient;
    [SerializeField] private AudioClip[] m_Sounds;
    [SerializeField] private float2 m_SpeedRange;
    [Header("Square data")]
    [SerializeField] SquareData[] m_SquareData;

    [Button]
    private void generateSquareData()
    {
        m_SquareData = new SquareData[m_Sounds.Length];
        float i_levelPercentage;

        for (int i_Level = 1; i_Level <= m_SquareData.Length; i_Level++)
        {
            i_levelPercentage = Mathf.InverseLerp(1, m_SquareData.Length, i_Level);

            float i_speed = Mathf.Lerp(m_SpeedRange.x, m_SpeedRange.y, i_levelPercentage);
            Color i_color = m_Gradient.Evaluate(i_levelPercentage);

            m_SquareData[i_Level - 1] = new SquareData(i_Level, i_color, i_speed, m_Sounds[i_Level - 1]);
        }
    }
    #endregion

    #region Time
    private Coroutine m_TimeUpdater;
    public event Action<float> OnTimeUpdate;

    private void startUpdateGlobalTime()
    {
        m_TimeUpdater = StartCoroutine(updateGlobalTime());
    }
    private void stopUpdateGlobalTime()
    {
        StopCoroutine(m_TimeUpdater);
        m_TimeUpdater = null;
    }

    private IEnumerator updateGlobalTime()
    {
        while (true)
        {
            OnTimeUpdate?.Invoke(Time.time);
            yield return null;
        }
    }
    #endregion

    #region Squares

    [SerializeField] private Square m_Test;

    [Button]
    private void moveSquare()
    {
        Vector2 tets = new Vector2(0.8f, 0.2f);

        m_Test.Initialize(this, m_SquareData[0], tets.normalized);
    }

    #endregion

    #endregion
}
