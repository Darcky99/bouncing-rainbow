using System;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using UnityEngine;

public class FPSManager : MonoBehaviour
{
	[SerializeField, Range(1,30)] private float m_UpdatesPerSecond = 5; //amount of updates per second
	
	[ShowInInspector, ReadOnly] public static int FPSAverageLastUpdate = 0;
	[ShowInInspector, ReadOnly] public static int FPSTicksLastUpdate   = 0;

	[ShowInInspector, ReadOnly] public static int FPSAveragePreviousLevel  = 0;
	[ShowInInspector, ReadOnly] public static int FPSTickPreviousLevel     = 0;

    private float m_LastTime = 0;
    private float m_DeltaTime;
    private float m_DeltaTimeSum;

    private float m_FPSSumPreviousLevel;
    
    private float m_CurrTime => Time.time;

    private void OnEnable()
    {
	    FPSAverageLastUpdate = 0;
	    m_LastTime = m_CurrTime;
	    
	    GameManager.OnLevelStarted += OnLevelStarted;
    }

    private void OnDisable()
    {
	    GameManager.OnLevelStarted -= OnLevelStarted;
    }

    private void OnLevelStarted()
    {
	    m_FPSSumPreviousLevel = FPSTickPreviousLevel = FPSAveragePreviousLevel = 0;
    }
    
    private void Update()
    {
    	m_DeltaTime = m_CurrTime - m_LastTime;
        
        if(m_DeltaTime <= 0) return;// will happen on the first tick
        
        m_LastTime     =  m_CurrTime;
        m_DeltaTimeSum += m_DeltaTime;
        
        FPSTicksLastUpdate++;
        if (m_DeltaTimeSum > 1 / m_UpdatesPerSecond)
        {
	        //better to use floating point to avoid deviating too much from the actual FPS values when calculating the level average
	        float fps = FPSTicksLastUpdate / m_DeltaTimeSum;
	        FPSAverageLastUpdate = Mathf.RoundToInt(fps);
	        
	        m_DeltaTimeSum = FPSTicksLastUpdate = 0;

	        if (GameManager.Instance.GameState == eGameState.Playing)
	        {
		        FPSTickPreviousLevel++;
		        m_FPSSumPreviousLevel     += fps;
		        FPSAveragePreviousLevel =  Mathf.RoundToInt(m_FPSSumPreviousLevel / FPSTickPreviousLevel);
	        }
        }
    }
}
