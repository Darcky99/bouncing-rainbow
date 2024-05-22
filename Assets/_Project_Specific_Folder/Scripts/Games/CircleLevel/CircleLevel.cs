using Cinemachine;
using DG.Tweening;
using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class CircleLevel : Level
{
    [SerializeField] private int m_InitialMoneyAmount = 10;

    #region Dependencies
    private GamePlayVariablesEditor m_GameplayVariables => GameConfig.Instance.GamePlay;
    private PoolManager m_PoolManager => PoolManager.Instance;
    private StorageManager m_StorageManager => StorageManager.Instance;
    #endregion

    #region Init
    private void OnEnable()
    {
        resetLevel();
        adjustCamera();

        GameManager.OnLevelStarted += onLevelStarted;
        GameManager.OnLevelCompleted += onLevelCompleted;
        InputManager.OnInputDown += onInputDown;
        OnTapStarted += onTapStarted;
        OnTapStoped += onTapStoped;
        OnCameraPositionChanged += m_Background.OnCameraPositionChanged;
        OnCameraSizeChanged += m_Background.OnCameraSizeChanged;
    }
    private void OnDisable()
    {
        GameManager.OnLevelStarted -= onLevelStarted;
        GameManager.OnLevelCompleted -= onLevelCompleted;
        InputManager.OnInputDown -= onInputDown;
        OnTapStarted -= onTapStarted;
        OnTapStoped -= onTapStoped;
        OnCameraPositionChanged -= m_Background.OnCameraPositionChanged;
        OnCameraSizeChanged -= m_Background.OnCameraSizeChanged;

        stopUpdateGlobalTime();
    }
    private void OnApplicationPause(bool pause)
    {
        save();
    }
    #endregion

    #region Callbacks
    private void onLevelStarted()
    {
        tryLoad();

        startUpdateGlobalTime();

        MenuManager.Instance.OpenMenuScreen(nameof(Screen_Circles));
    }
    private void onLevelCompleted()
    {
        deleteSaveFile();

        MenuManager.Instance.CloseMenuScreen(nameof(Screen_Circles));
    }
    private void onInputDown(Vector2 i_Position)
    {
        if (m_TimeScale <= 1f)
            OnTapStarted?.Invoke();

        if (m_StopTap != null) StopCoroutine(m_StopTap);
        m_StopTap = StartCoroutine(stopTap());
    }
    #endregion

    #region Specific
    public float IncomePerSecond 
    { 
        get {
            float i_total = 0;
            for(int i_level = 1; i_level <= CircleCount; i_level++)
            {
                float i_circleEarning = m_GameplayVariables.CircleEarning * i_level;
                i_total += ((2f * i_circleEarning) + (EnabledGatesCount * i_circleEarning)) * m_CircleData[i_level - 1].Speed;
            }
            return i_total * m_TimeScale;
        } 
    }
    private void resetLevel() 
    {
        if (m_Circles == null)
            m_Circles = new List<Circle>();
        else
            removeAllCircles();

        m_EnabledGates = 0;
        m_StorageManager.CoinsAmount = m_InitialMoneyAmount;
        m_Gates.EnableGates(m_EnabledGates);
    }

    #region Tap
    public static event Action OnTapStarted;
    public static event Action OnTapStoped;

    private Coroutine m_StopTap;
    private WaitForSeconds m_WFS = new WaitForSeconds(0.5f);
    private IEnumerator stopTap()
    {
        yield return m_WFS;
        m_StopTap = null;
        OnTapStoped?.Invoke();
    }
    private void onTapStarted()
    {
        DOVirtual.Float(1f, 2.4f, 0.3f, (float i_Value) => { m_TimeScale = i_Value; });
    }
    private void onTapStoped()
    {
        DOVirtual.Float(2.4f, 1f, 0.3f, (float i_Value) => { m_TimeScale = i_Value; });
    }
    #endregion

    #region Time
    private float m_GlobalTime;
    [ShowInInspector, Range(0f, 3.5f)] private float m_TimeScale = 1f;
    private Coroutine m_TimeUpdater;
    public event Action<float> OnTimeUpdate;

    public float GlobalTime { get { return m_GlobalTime; } }

    private void startUpdateGlobalTime()
    {
        m_TimeUpdater = StartCoroutine(updateGlobalTime());
    }
    private void stopUpdateGlobalTime()
    {
        if (m_TimeUpdater != null)
            StopCoroutine(m_TimeUpdater);
        m_TimeUpdater = null;
    }
    private IEnumerator updateGlobalTime()
    {
        m_GlobalTime = 0f;
        while (true) {
            m_GlobalTime += Time.deltaTime * m_TimeScale;
            OnTimeUpdate?.Invoke(m_GlobalTime);
            yield return null;
        }
    }
    #endregion

    #region Circles
    [SerializeField] private Transform m_CircleHolder;
    [SerializeField] private List<Circle> m_Circles;

    public int CircleCount { get { return m_Circles.Count; } }

    #region Data
    [Title("Circle Data")]
    [SerializeField, PropertyOrder(Order = 0f)] private int m_CircleAmount;
    [SerializeField, PropertyOrder(Order = 0f)] private Gradient m_Gradient;
    [SerializeField, PropertyOrder(Order = 0f)] private float2 m_LenghtRange;
    [SerializeField, PropertyOrder(Order = 0f)] private float2 m_SpeedRange;
    [SerializeField, PropertyOrder(Order = 1f), ListDrawerSettings(ListElementLabelName = "Identifier")] private CircleData[] m_CircleData;
    public int CircleTypesAmount { get { return m_CircleData.Length; } }

    [Button, PropertyOrder(Order = 0.5f)]
    private void generateCirclesData()
    {
        m_CircleData = new CircleData[m_CircleAmount];
        float t;

        for(int i = 1; i <= m_CircleAmount; i++)
        {
            t = Mathf.InverseLerp(1f, m_CircleAmount, i);

            Color i_color = m_Gradient.Evaluate(t);
            float i_lenght = Mathf.Lerp(m_LenghtRange.x, m_LenghtRange.y, t);
            float i_speed = Mathf.Lerp(m_SpeedRange.x, m_SpeedRange.y, t);

            m_CircleData[i - 1] = new CircleData(i, i_color, i_lenght, i_speed);
        }
    }
    #endregion

    #region Add
    private void addCircle()
    {
        if(CircleCount < CircleTypesAmount)
            addCircle(CircleCount + 1);
    }
    [Button(Style = ButtonStyle.CompactBox), PropertyOrder(Order = 2f)]
    private void addCircle(int i_Level)
    {
        i_Level = Mathf.Clamp(i_Level, 1, m_CircleData.Length);
        addCircle(m_CircleData[i_Level - 1]);
    }
    private void addCircle(CircleData i_CircleData)
    {
        Circle i_Circle = m_PoolManager.Dequeue(ePoolType.Circles, i_Parent : m_CircleHolder).GetComponent<Circle>();
        i_Circle.Initilize(this, i_CircleData);

        m_Circles.Add(i_Circle);
    }
    private void removeAllCircles()
    {
        foreach (Circle i_circle in m_Circles)
            i_circle.Queue();
        
        m_Circles.Clear();
    }

    [Button]
    private void addAllCircles() 
    {
        for(int i = 1; i <= CircleTypesAmount; i++)
            addCircle(i);
    }
    public bool CanAddCircle() {
        return CircleCount < CircleTypesAmount;
    }
    #endregion

    #endregion

    #region Gates
    [SerializeField] private Gates m_Gates;
    [SerializeField, ReadOnly] private int m_EnabledGates;
    public event Action OnGateAdded;

    private int m_TotalGatesAmount { get { return m_Gates.GetTotalGateCount(); } }
    public int EnabledGatesCount { get { return m_EnabledGates; } }
    public float SectionInTurns { get { return 1f / (m_TotalGatesAmount + 1); } }

    [Button]
    private void addGate()
    {
        m_EnabledGates++;
        m_Gates.EnableGates(m_EnabledGates);
        OnGateAdded?.Invoke();
    }
    public bool CanAddGate()
    {
        return EnabledGatesCount < m_TotalGatesAmount;
    }
    #endregion

    #region Camera settings
    [SerializeField] private BackgroundReSize m_Background;
    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;
    public event Action<Vector2> OnCameraPositionChanged;
    public event Action OnCameraSizeChanged;

    private void adjustCamera()
    {
        CinemachineTransposer i_cinemachineTransposer = m_VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        Vector3 i_prevOffset = i_cinemachineTransposer.m_FollowOffset;
        Vector3 i_newOffset = Vector2.zero; 
        i_newOffset.z = -10f;

        DOVirtual.Float(m_VirtualCamera.m_Lens.OrthographicSize, 5.5f, 0.5f, (float i_value) => { updateOrthoSize(i_value); }).SetEase(Ease.InOutSine);
        DOVirtual.Vector3(i_prevOffset, i_newOffset, 1f, (Vector3 i_value) => { updateFollowOffset(i_value); }).SetEase(Ease.InOutSine);

        Camera.main.backgroundColor = Color.white;

        void updateFollowOffset(Vector3 i_value) {
            i_cinemachineTransposer.m_FollowOffset = i_value;
            OnCameraPositionChanged?.Invoke(i_value);
        }
        void updateOrthoSize(float i_value)
        {
            m_VirtualCamera.m_Lens.OrthographicSize = i_value;
            OnCameraSizeChanged?.Invoke();
        }
    }
    #endregion

    #region Player
    public void AddCircle() {
        addCircle();
    }
    public void AddGate() {
        addGate();
    }
    #endregion

    #region Save and Load
    private const string k_KEY = "SALAMIO";

    private void save()
    {
        CirclesSavingSystem i_saveData = new CirclesSavingSystem(this);
        string i_stringOfData = JsonConvert.SerializeObject(i_saveData);
        PlayerPrefs.SetString(k_KEY, i_stringOfData);
        print($"Saved, it should contaign {i_saveData.CircleCount} circles, {i_saveData.GatesCount} gates, and {i_saveData.Coins} coins");
    }
    private bool tryLoad()
    {
        if (!PlayerPrefs.HasKey(k_KEY)) return false;

        resetLevel();

        string i_stringOfData = PlayerPrefs.GetString(k_KEY);
        CirclesSavingSystem i_saveData = JsonConvert.DeserializeObject<CirclesSavingSystem>(i_stringOfData);

        for(int i = 0; i < i_saveData.CircleCount; i++)
            AddCircle();
        for (int i = 0; i < i_saveData.GatesCount; i++)
            AddGate();

        StorageManager.Instance.SetCollectable(KobGamesSDKSlim.Collectable.eCollectableType.Coin, i_saveData.Coins);
        print($"Loaded, it should contaign {i_saveData.CircleCount} circles, {i_saveData.GatesCount} gates, and {i_saveData.Coins} coins");
        return true;
    }
    private void deleteSaveFile()
    {
        if (PlayerPrefs.HasKey(k_KEY))
            PlayerPrefs.DeleteKey(k_KEY);
    }


    private class CirclesSavingSystem
    {
        public int CircleCount;
        public int GatesCount;
        public int Coins;

        public CirclesSavingSystem(CircleLevel i_CircleLevel)
        {
            CircleCount = i_CircleLevel.CircleCount;
            GatesCount = i_CircleLevel.m_EnabledGates;
            Coins = StorageManager.Instance.CoinsAmount;
        }
        public CirclesSavingSystem() { }
    }
    #endregion

    #endregion
}
