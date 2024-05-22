using DG.Tweening;
using KobGamesSDKSlim;
using KobGamesSDKSlim.Collectable;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineArc : MonoBehaviour
{
    [Header("Variables")]
    private Squad m_ParentSquad;
    private int m_Order;
    private float m_LocalTime;
    private float m_Relation;
    private int m_Direction;
    [Header("References")]
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private LineArcSO m_LineType;
    [SerializeField] private TrailRenderer m_TrailRenderer;

    public LineArcSO LineType { get { return m_LineType; } }
    public int Order { get { return m_Order; } }

    #region Dependencies
    private SoundManager m_SoundManager => SoundManager.Instance;
    private GamePlayVariablesEditor m_GamePlayVars => GameConfig.Instance.GamePlay;
    private CollectableManager m_CollectableManager => CollectableManager.Instance;
    private StorageManager m_StorageManager => StorageManager.Instance;
    #endregion

    #region Init
    private void OnEnable()
    {
        SquadsLevel.OnTapStarted += onTapStarted;
        SquadsLevel.OnTapStoped += onTapStoped;
    }
    private void OnDisable()
    {
        SquadsLevel.OnTapStarted -= onTapStarted;
        SquadsLevel.OnTapStoped -= onTapStoped;
    }
    #endregion

    #region Callbacks
    private void onTapStarted()
    {
        DOVirtual.Float(0f, 0.2f, 0.3f, (float i_Value) => { m_TrailRenderer.time = i_Value; });
    }
    private void onTapStoped()
    {
        DOVirtual.Float(0.2f, 0f, 0.3f, (float i_Value) => { m_TrailRenderer.time = i_Value; });
    }
    #endregion

    #region Specific

    #region Pool
    public void Initialize(Squad i_ParentObject, LineArcSO i_LineType, float i_lenght, int i_Order = 1)
    {
        m_ParentSquad = i_ParentObject;
        m_LineType = i_LineType;
        m_SpriteRenderer.size = new Vector2(m_SpriteRenderer.size.x, i_lenght);
        m_Order = i_Order;
        m_SpriteRenderer.color = m_LineType.LineColor;
        m_TrailRenderer.transform.SetLocalY(i_lenght - 0.1f);
        m_TrailRenderer.startColor = i_LineType.LineColor;
        m_TrailRenderer.endColor = i_LineType.LineColor;
        m_TrailRenderer.sortingOrder = 10 - i_LineType.Level;
        m_SpriteRenderer.sortingOrder = 10 - i_LineType.Level;

        startMovement(i_ParentObject.SquadLevel.GlobalTime);
    }
    public void OnQueue()
    {
        stopMovement();
        m_ParentSquad = null;
        m_LineType = null;
        m_LocalTime = 0f;
        m_Direction = 0;
    }
    #endregion

    #region Movement
    private void startMovement(float i_GlobalTime)
    {
        //We need to know direction from the first moment to avoid a "wall hit" at the wrong time.
        m_Relation = (i_GlobalTime - (m_Order * m_GamePlayVars.LineTimeOffset)) * m_LineType.Speed;
        m_Direction = (Mathf.FloorToInt(m_Relation) % 2 == 0) ? 1 : -1;
        //Events...
        m_ParentSquad.SquadLevel.OnTimeUpdate += onTimeUpdate;
    }
    private void stopMovement()
    {
        m_ParentSquad.SquadLevel.OnTimeUpdate -= onTimeUpdate;
    }
    //[SerializeField] private float offset = 0f;
    private void changeDirection(int i_NewDirection)
    {
        if (i_NewDirection == m_Direction) return;

        m_Direction = i_NewDirection;
        m_SoundManager.PlayCustomClip(m_LineType.HitSound,m_LineType.Pitch,1);
        int i_lineCoins = m_GamePlayVars.CoinsPerLine * m_LineType.Level;
        int i_incomeLevel = m_ParentSquad.SquadLevel.IncomeLevel;
        int i_addAmount = i_lineCoins + Mathf.FloorToInt(i_lineCoins * i_incomeLevel * 0.1f);
        m_StorageManager.AddCollectable(eCollectableType.Coin, i_addAmount);
        
        Vector3 i_WorldLinePosition = transform.position + (transform.up * m_SpriteRenderer.size.y);
        Vector3 i_InQuadPosition = m_ParentSquad.transform.InverseTransformPoint(i_WorldLinePosition);
        i_InQuadPosition.x = i_InQuadPosition.x < 0 ? i_InQuadPosition.x + 0.05f : i_InQuadPosition.x - 0.05f;
        Vector3 i_worldPos = m_ParentSquad.transform.TransformPoint(i_InQuadPosition);

        m_CollectableManager.ShowEarnMessage(eCollectableType.Coin, i_addAmount, eCollectableEarnMessageAnimType.Default, i_worldPos);
    }
    private void onTimeUpdate(float i_newTime)
    {
        //m_Relation is the global time * speed. Here we use global time - an offset to take order into account too.
        float m_Relation = (i_newTime - (m_Order * m_GamePlayVars.LineTimeOffset)) * m_LineType.Speed;

        if (Mathf.FloorToInt(m_Relation) % 2 == 0) {
            //es par
            m_LocalTime = 1 - m_Relation % 1;
            changeDirection(1);
        }
        else {
            //impar
            m_LocalTime = m_Relation % 1;
            changeDirection(-1);
        }
        updateZAngle();
    }

    #region Set angles
    private Vector3 m_NewRotation = Vector3.zero;
    private void updateZAngle()
    {
        float Z = Mathf.Lerp(45f, -45f, m_LocalTime);
        SetZAngle(Z);
    }
    private void SetZAngle(float Z)
    {
        m_NewRotation.z = Z;
        transform.localEulerAngles = m_NewRotation;
    }
    #endregion

    #endregion

    #endregion
}