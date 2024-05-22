using Cinemachine;
using DG.Tweening;
using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class SquadsLevel : Level
{
    #region Variables that should be in GameConfig.
    [Header("Configuration")]
    [SerializeField] private float m_StartingDistance = 0.7f;
    [SerializeField] private float m_DistanceBetween = 0.3f;
    public float StartingDistance { get { return m_StartingDistance; } }
    public float DistanceBetween { get { return m_DistanceBetween; } }
    #endregion

    #region Dependencies
    private StorageManager m_StorageManager => StorageManager.Instance;
    private InputManager m_InputManager => InputManager.Instance;
    private MenuManager m_MenuMangaer => MenuManager.Instance;
    private GamePlayVariablesEditor m_Gameplay => GameConfig.Instance.GamePlay;
    #endregion

    #region Init
    private void OnEnable()
    {
        m_Squads = transform.GetComponentsInChildren<Squad>(true);

        restartGameState();

        GameManager.OnLevelCompleted += onLevelCompleted;
        GameManager.OnLevelStarted += onLevelStarted;
        InputManager.OnInputDown += onInputDown;
        OnTapStarted += onTapStarted;
        OnTapStoped += onTapStoped;
        OnCameraPositionChanged += m_Background.OnCameraPositionChanged;
        OnCameraSizeChanged += m_Background.OnCameraSizeChanged;
    }
    private void OnDisable()
    {
        GameManager.OnLevelCompleted -= onLevelCompleted;
        GameManager.OnLevelStarted -= onLevelStarted;
        InputManager.OnInputDown -= onInputDown;
        OnTapStarted -= onTapStarted;
        OnTapStoped -= onTapStoped;
        OnCameraPositionChanged -= m_Background.OnCameraPositionChanged;
        OnCameraSizeChanged -= m_Background.OnCameraSizeChanged;
    }
    private void OnApplicationQuit()
    {

    }
    private void OnApplicationPause(bool pause)
    {
        Save();
    }
    #endregion

    #region Callbacks
    private void onLevelStarted()
    {
        //if there's no saved game...
        if (FileExists())
            TryLoadGame();

        m_MenuMangaer.OpenMenuScreen(nameof(Screen_ArcLine));
        
        startUpdateGlobalTime();
    }
    private void onLevelCompleted()
    {
        stopUpdateGlobalTime();
        SaveSquadsLevel.DeleteSaveFile();
    }
    private void onInputDown(Vector2 i_Position)
    {
        if (m_TimeScaleValue <= 1f)
            OnTapStarted?.Invoke();

        if (m_StopTap != null) StopCoroutine(m_StopTap);
        m_StopTap = StartCoroutine(stopTap());
    }
    #endregion

    #region Specific
    private void restartGameState()
    {
        foreach (Squad i_squad in m_Squads)
            i_squad.CleanSquad();

        m_StorageManager.SetCollectable(KobGamesSDKSlim.Collectable.eCollectableType.Coin, 80);
        m_SquadCount = 1;
        m_IncomeLevel = 0;
        refreshSquads();
    }
    public int GetTotalLineCount()
    {
        int i_lineCount = 0;

        foreach (Squad i_squad in m_Squads)
            i_lineCount += i_squad.LineCount;

        return i_lineCount;
    }

    #region LineTypes
    [SerializeField] private LinesSO m_LinesData;
    public int LineTypeCount { get { return m_LinesData.LinesList.Length; } }
    public LineArcSO GetLineType(int i_level)
    {
        i_level = Mathf.Clamp(i_level, 1, m_LinesData.LinesList.Length);

        return m_LinesData.LinesList[i_level - 1];
    }
    public LineArcSO[] GetLineTypes(params int[] i_levels)
    {
        if (i_levels.Length <= 0) return null;

        LineArcSO[] i_lines = new LineArcSO[i_levels.Length];

        for (int i = 0; i < i_levels.Length; i++)
        {
            i_lines[i] = GetLineType(i_levels[i]);
        }
        return i_lines;
    }
    public LineArcSO[] GetAllLineTypes()
    {
        LineArcSO[] i_lines = new LineArcSO[m_LinesData.LinesList.Length];

        for (int i = 1; i <= m_LinesData.LinesList.Length; i++)
        {
            i_lines[i - 1] = GetLineType(i);
        }
        return i_lines;
    }
    #endregion

    #region Squads
    private Squad[] m_Squads;
    [ShowInInspector, ReadOnly] private int m_SquadCount;

    public Squad[] Squads { get { return m_Squads; } }
    public int SquadCount { get { return m_SquadCount; } }
    public int LineCount { get {  int i_totalLineCount  = 0; foreach (Squad i_squad in m_Squads) i_totalLineCount += i_squad.LineCount; return i_totalLineCount; } }
    public bool CanAddLine
    {
        get
        {
            for (int i = 0; i < SquadCount; i++)
            {
                if (m_Squads[i].CanAddLine()) return true;
            }
            return false;
        }
        
    }
    public bool CanAddSquad
    {
        get
        {
            return m_SquadCount < m_Squads.Length && !m_Squads[m_SquadCount - 1].CanAddLine();
        }
    }
    public bool CanAddSomething
    {
        get
        {
            return CanAddLine || CanAddSquad;
        }
    }

    private void addSquad()
    {
        if (!CanAddSquad) return;

        m_SquadCount++;
        refreshSquads();
    }
    public bool IsFull
    {
        get
        {
            return (CanAddSquad || CanAddLine) == false;
        }
    }
    private void refreshSquads()
    {
        if (m_SquadCount > m_Squads.Length) return;

        for (int i = 0; i < Squads.Length; i++)
            m_Squads[i].gameObject.SetActive(i < m_SquadCount);

        moveCameraOffSet();
    }
    private Squad getSquadByPotential(int i_level)
    {
        Squad i_toReturn = m_Squads[0];
        int i_potentialValue = m_Squads[0].GetPotential(i_level);

        foreach (Squad i_squad in m_Squads) {
            if (i_squad.gameObject.activeInHierarchy && i_squad.GetPotential(i_level) > i_potentialValue)
                i_toReturn = i_squad;
        }
        return i_toReturn;
    }
    private Squad getSquadByLineCount()
    {
        Squad i_toReturn = m_Squads[0];

        foreach (Squad i_squad in m_Squads) {
            if (i_squad.gameObject.activeInHierarchy && i_squad.LineCount < i_toReturn.LineCount)
                i_toReturn = i_squad;
        }
        return i_toReturn;
    }
    private bool tryLineMerge(out Squad i_Result)
    {
        i_Result = null;

        for (int i_level = 1; i_level <= m_LinesData.LinesList.Length; i_level++) {
            for (int i_squad = 0; i_squad < m_SquadCount; i_squad++) {
                if (m_Squads[i_squad].CanMergeLines(i_level))
                {
                    i_Result = m_Squads[i_squad];
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Income
    [SerializeField, ReadOnly] private int m_IncomeLevel;
    public int IncomeLevel { get { return m_IncomeLevel; } }
    public int IncomePerSecond {
        get {
            float i_totalIncome = 0;
            for (int i = LineCount; i >= 1; i--) {
                int valueToAdd = ((i - 1) % 7) * 5 + 5;
                i_totalIncome += valueToAdd;
            }
            i_totalIncome += i_totalIncome * m_IncomeLevel * 0.1f;
            return Mathf.FloorToInt((i_totalIncome * m_TimeScaleValue));
        }
    }
    
    #endregion

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
        DOVirtual.Float(1f, 1.5f, 0.3f, (float i_Value) => { m_TimeScaleValue = i_Value; });
    }
    private void onTapStoped()
    {
        DOVirtual.Float(1.5f, 1f, 0.3f, (float i_Value) => { m_TimeScaleValue = i_Value; });
    }
    #endregion

    #region Time
    [SerializeField, Range(0.5f, 5f)]
    private float m_TimeScaleValue = 1;
    private float m_GlobalTime;
    public float GlobalTime { get { return m_GlobalTime; } }

    public event Action<float> OnTimeUpdate;
    private Coroutine m_TimeUpdater;


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
        m_GlobalTime = 0f;

        while (true)
        {
            m_GlobalTime += Time.deltaTime * m_TimeScaleValue;
            OnTimeUpdate?.Invoke(m_GlobalTime); 
            yield return null;
        }
    }
    #endregion

    #region Player
    public void AddLineOrQuad()
    {
        if (CanAddLine)
            m_Squads[SquadCount - 1].AddLine();
        else if (CanAddSquad)
            addSquad();
        else
            Debug.LogError("Trying to add withoud space!");
    }
    public void IncreaseIncome()
    {
        m_IncomeLevel++;
    }
    #endregion

    #region Camera
    [SerializeField] private BackgroundReSize m_Background;
    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;
    public event Action<Vector2> OnCameraPositionChanged;
    public event Action OnCameraSizeChanged;
    private void moveCameraOffSet()
    {
        float i_vectorLength = 1.3f; //idealy this should be in the gameconfig?
        Vector3 i_newOffset;

        switch (m_SquadCount)
        {
            case 1:
                i_newOffset = Vector3.up * i_vectorLength;
                break;
            case 3:
            case 4:
                DOVirtual.Float(m_VirtualCamera.m_Lens.OrthographicSize, 6f, 1f, (float i_value) => { updateOrthoSize(i_value); }).SetEase(Ease.InOutSine);
                i_newOffset = Vector2.down * (i_vectorLength / 2f);
                break;
            default:
                i_newOffset = Vector2.zero;
                break;
        }
        i_newOffset.z = -10f;

        CinemachineTransposer i_cinemachineTransposer = m_VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        Vector3 i_prevOffset = i_cinemachineTransposer.m_FollowOffset;

        DOVirtual.Vector3(i_prevOffset, i_newOffset, 1f, (Vector3 i_value) => { updateFollowOffset(i_value); }).SetEase(Ease.InOutSine);

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

    #region Saving and loading
    [Button]
    private void Save()
    {
        SaveSquadsLevel.Save(this);
    }
    [Button]
    private bool TryLoadGame()
    {
        if (SaveSquadsLevel.TryLoad(out SaveSquadsLevel i_SaveData) == false) { //No file? ok.
            Debug.Log("There is no save file");
            return false;
        }

        restartGameState();

        m_SquadCount = i_SaveData.SquadCount;
        refreshSquads();

        for (int i = 0; i < i_SaveData.Squads.Length; i++)
            m_Squads[i].LoadSquad(i_SaveData.Squads[i]);

        StorageManager.Instance.CoinsAmount = i_SaveData.Coins;
        m_IncomeLevel = i_SaveData.IncomeLevel;

        return true;
    }
    private bool FileExists()
    {
        return SaveSquadsLevel.Exists();
    }
    [Button]
    private void DeleteFile()
    {
        SaveSquadsLevel.DeleteSaveFile();
    }
    #endregion

    #endregion
}