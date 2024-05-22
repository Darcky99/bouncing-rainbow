using KobGamesSDKSlim;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Square : MonoBehaviour
{
    private SquareLevel m_SquareLevel;
    private SquareData m_SquareData;

    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    private Vector2 m_Direction;

    private GamePlayVariablesEditor m_GameplayVariables => GameConfig.Instance.GamePlay;

    #region Specific

    #region Pool
    public void Initialize(SquareLevel i_SquareLevel, SquareData i_SquareData, Vector2 i_Direction)
    {
        m_SquareLevel = i_SquareLevel;
        m_SquareData = i_SquareData;
        m_SpriteRenderer.color = m_SquareData.Color;
        m_Direction = i_Direction;

        startMovement();
    }
    public  void OnQueue()
    {
        stopMovement();
        m_SquareLevel = null;
    }
    #endregion

    #region Movement
    private float m_localTimeX;
    private float m_localTimeY;
    private void startMovement()
    {
        m_SquareLevel.OnTimeUpdate += onTimeUpdate;
    }
    private void stopMovement()
    {
        m_SquareLevel.OnTimeUpdate -= onTimeUpdate;
    }
    private void onTimeUpdate(float i_GlobalTime)
    {
        //Conver to a local x and y.
        float i_relationX = i_GlobalTime * m_Direction.x * m_SquareData.Speed;
        float i_relationY = i_GlobalTime * m_Direction.y * m_SquareData.Speed;

        if (Mathf.FloorToInt(i_relationX) % 2 == 0)
            m_localTimeX = i_relationX % 1f;
        else
            m_localTimeX = 1 - i_relationX % 1f;

        if(Mathf.FloorToInt(i_relationY) % 2 == 0)
            m_localTimeY = i_relationY % 1f;
        else
            m_localTimeY = 1 - i_relationY % 1f;

        updateSquarePosition();
    }
    private void updateSquarePosition()
    {
        float2 i_xLimits = m_GameplayVariables.XCoordinateRange;
        float2 i_yLimits = m_GameplayVariables.YCoordinateRange;
        
        float i_xCoord = Mathf.Lerp(i_xLimits.x, i_xLimits.y, m_localTimeX);
        float i_yCoord = Mathf.Lerp(i_yLimits.x, i_yLimits.y, m_localTimeY);

        transform.position = new Vector3(i_xCoord, i_yCoord);
    }
    #endregion

    #endregion
}
