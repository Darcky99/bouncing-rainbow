using DG.Tweening;
using KobGamesSDKSlim;
using KobGamesSDKSlim.Collectable;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    private CircleLevel m_CircleLevel;
    private CircleData m_CircleData;
    [SerializeField] private Vector3 m_InitialScale;

    [SerializeField] private SpriteRenderer m_Base;
    [SerializeField] private SpriteRenderer m_Line;
    [SerializeField] private SpriteRenderer m_Circle;

    [Button]
    private void setReferences()
    {
        SpriteRenderer[] i_spriteRenderer = transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>();

        m_Base = i_spriteRenderer[0];
        m_Line = i_spriteRenderer[1];
        m_Circle = i_spriteRenderer[2];
    }

    #region Dependencies
    private CollectableManager m_CollectablesManager => CollectableManager.Instance;
    private GamePlayVariablesEditor m_GameplayVariables => GameConfig.Instance.GamePlay;
    private PoolManager m_PoolManager => PoolManager.Instance;
    private StorageManager m_StorageManager => StorageManager.Instance;
    private SoundManager m_SoundManager => SoundManager.Instance;
    #endregion

    #region Init
    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }
    #endregion

    #region Callbacks
    private void onGateAdded()
    {
        updateNextGateTreshold();
    }
    #endregion

    #region Pool
    public void Initilize(CircleLevel i_CircleLevel, CircleData i_CircleData)
    {
        m_CircleLevel = i_CircleLevel;
        m_CircleData = i_CircleData;

        m_InitialScale = m_Circle.transform.localScale;
        m_Line.size = new Vector2(0.05f, m_CircleData.LineLenght);
        setColor(m_CircleData.Color);
        setSortingOrder(m_CircleLevel.CircleTypesAmount - i_CircleData.Level);
        m_Circle.transform.SetLocalY(m_CircleData.LineLenght);

        m_AngleLimit = m_AngleLimits[m_CircleData.Level - 1];

        startMovement();
    }
    public void Queue()
    {
        stopMovement();

        m_CircleLevel = default;
        m_CircleData = default;

        m_LocalTime = default;
        m_AngleLimit = default;
        m_Rotation = default;
        m_RotationDirection = default;
        m_NextGateThreshold = default;

        m_PoolManager.Queue(ePoolType.Circles, gameObject);
    }
    #endregion

    #region Specific

    #region Movement
    private float m_LocalTime;
    private float m_AngleLimit;
    private Vector3 m_Rotation;
    private int m_RotationDirection;
    private float m_NextGateThreshold;

    private readonly float[] m_AngleLimits = new float[]
    { 153.125f, 164.779f, 168.148f, 170.976f, 172.747f, 173.159f, 173.913f, 174.817f, 175.465f, 175.942f,176.3f, 176.466f, 176.889f, 177.159f, 177.5f, 177.724f };
    private void setRotation(float i_LocalTime)
    {
        m_Rotation.z = Mathf.Lerp(m_AngleLimit, -m_AngleLimit, i_LocalTime);
        transform.localEulerAngles = m_Rotation;
    }

    #region Init
    private void startMovement()
    {
        initialzeRotationValues();
        updateNextGateTreshold();

        m_CircleLevel.OnTimeUpdate += onTimeUpdate;
        m_CircleLevel.OnGateAdded += onGateAdded;
    }
    private void stopMovement()
    {
        m_CircleLevel.OnTimeUpdate -= onTimeUpdate;
        m_CircleLevel.OnGateAdded -= onGateAdded;
    }
    #endregion

    #region Loop
    private void onTimeUpdate(float i_NewTime)
    {
        float i_relation = i_NewTime * m_CircleData.Speed;
        bool i_isPair = Mathf.FloorToInt(i_relation) % 2 == 0;

        if (i_isPair) {
            checkDirectionChange(1);
            m_LocalTime = i_relation % 1f;
        }
        else {
            checkDirectionChange(-1);
            m_LocalTime = 1 - i_relation % 1f;
        }

        setRotation(m_LocalTime);
        checkGatePassThrough();
    }
    private void checkDirectionChange(int i_NewDirection)
    {
        if (m_RotationDirection == i_NewDirection) return;

        m_RotationDirection = i_NewDirection;
        onDirectionChangeTrigger();
    }
    private void checkGatePassThrough()
    {
        if (m_RotationDirection == 1 && m_LocalTime >= m_NextGateThreshold || m_RotationDirection == -1 && m_LocalTime <= m_NextGateThreshold)
            onGatePassThroughTrigger();
    }
    #endregion

    #region Triggers
    private void onDirectionChangeTrigger()
    {
        int i_earnAmount = m_GameplayVariables.CircleEarning * m_CircleData.Level;

        m_SoundManager.PlayCustomClip(m_CircleData.Sound, 1f, 1);
        m_StorageManager.AddCollectable(eCollectableType.Coin, i_earnAmount);
        m_CollectablesManager.ShowEarnMessage(eCollectableType.Coin, i_earnAmount, eCollectableEarnMessageAnimType.Default, m_Circle.transform.position, false);
        m_Circle.transform.DOPunchScale(Vector3.one * 0.7f, 0.2f).SetEase(Ease.InOutBounce).OnComplete(() => m_Circle.transform.localScale = m_InitialScale);

        updateNextGateTreshold();
    }
    private void onGatePassThroughTrigger()
    {
        int i_earnAmount = m_GameplayVariables.CircleEarning * m_CircleData.Level;

        m_StorageManager.AddCollectable(eCollectableType.Coin, i_earnAmount);
        m_CollectablesManager.ShowEarnMessage(eCollectableType.Coin, i_earnAmount, eCollectableEarnMessageAnimType.Default, m_Circle.transform.position, false);
        m_Circle.transform.DOPunchScale(Vector3.one * 0.7f, 0.2f).SetEase(Ease.InOutBounce).OnComplete(() => m_Circle.transform.localScale = m_InitialScale);

        updateNextGateTreshold();
    }
    #endregion

    #region Extra
    private void initialzeRotationValues()
    {
        float i_relation = m_CircleLevel.GlobalTime * m_CircleData.Speed;
        m_RotationDirection = (Mathf.FloorToInt(i_relation) % 2 == 0) ? 1 : -1;

        if(m_RotationDirection == 1)
            m_LocalTime = i_relation % 1f;
        else if (m_RotationDirection == -1)
            m_LocalTime = 1 - i_relation % 1f;
    }
    private void updateNextGateTreshold()
    {
        //Updated when: 1. Pass a gate | 2. Change direction | 3. Add a gate
        int i_enabledGates = m_CircleLevel.EnabledGatesCount;
        if (i_enabledGates == 0) {
            m_NextGateThreshold = (m_RotationDirection == 1) ? 10f : -10f;
            return;
        }

        float i_leftLimitInTurns = m_CircleLevel.SectionInTurns;
        float i_rightLimitInTurns = i_leftLimitInTurns * i_enabledGates;
        float i_currentGateInTurns;

        if (m_RotationDirection == 1)
        {
            for(i_currentGateInTurns = i_leftLimitInTurns; i_currentGateInTurns <= i_rightLimitInTurns; i_currentGateInTurns += i_leftLimitInTurns)
            {
                if(i_currentGateInTurns > m_LocalTime)
                {
                    m_NextGateThreshold = i_currentGateInTurns;
                    return;
                }
            }
            //We are past the last gate, set to unreacheable...
            m_NextGateThreshold = 10;
            return;
        }
        else if(m_RotationDirection == -1)
        {
            for (i_currentGateInTurns = i_rightLimitInTurns; i_currentGateInTurns >= i_leftLimitInTurns; i_currentGateInTurns -= i_leftLimitInTurns)
            {
                if (i_currentGateInTurns < m_LocalTime)
                {
                    m_NextGateThreshold = i_currentGateInTurns;
                    return;
                }
            }
            //We are past the last gate, set to unreacheable...
            m_NextGateThreshold = -10;
            return;
        }
        print($"The Var {m_RotationDirection} it's not working well. Something's off!");
        return;
    }
    #endregion

    #endregion

    #region Sprites
    private void setColor(Color i_Color)
    {
        m_Base.color = i_Color;
        m_Line.color = i_Color;
        m_Circle.color = i_Color;
    }
    private void setSortingOrder(int i_Order)
    {
        m_Base.sortingOrder = i_Order;
        m_Line.sortingOrder = i_Order;
        m_Circle.sortingOrder = i_Order + 1;
    }
    #endregion

    #endregion
}