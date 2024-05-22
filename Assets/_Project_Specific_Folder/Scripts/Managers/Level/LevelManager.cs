using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;

public class LevelManager : Singleton<LevelManager>
{
    private LevelsVariablesEditor m_LevelParameters => GameConfig.Instance.Levels;

    [Title("Refs")]
    [SerializeField, ReadOnly] private LevelDictionary m_Levels;

    public Level CurrentLevel { get; private set; }

    #region Editor
    [Button]
    private void SetRefs()
    {
        var levels = GetComponentsInChildren<Level>(true);

        m_Levels = new LevelDictionary();

        foreach (var level in levels)
        {
            m_Levels.Add(level.gameObject.name, level);
        }
    }
    [Button]
    private void CleanRefs()
    {
        m_Levels.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetRefs();
    }
#endif

    public ValueDropdownList<string> LevelNames
    {
        get
        {
            ValueDropdownList<string> ValueDropdownList = new ValueDropdownList<string>();

            foreach (var item in m_Levels.Keys)
            {
                ValueDropdownList.Add(item);
            }

            return ValueDropdownList;
        }
    }
    #endregion

    #region Init
    private void OnEnable()
    {
        GameManager.OnLevelLoaded += onLevelLoaded;
    }
    public override void OnDisable()
    {
        GameManager.OnLevelLoaded -= onLevelLoaded;

        base.OnDisable();
    }
    #endregion

    #region Callbacks
    private void onLevelLoaded()
    {
        disableAllLevels();

        var levelName = m_LevelParameters.Levels[(StorageManager.Instance.CurrentLevel - 1) % m_LevelParameters.Levels.Length];

        CurrentLevel = m_Levels[levelName];

        CurrentLevel.gameObject.SetActive(true);
    }
    #endregion

    #region Specific
    private void disableAllLevels()
    {
        foreach (var level in m_Levels)
        {
            level.Value.gameObject.SetActive(false);
        }
    }
    #endregion

    [System.Serializable] public class LevelDictionary : UnitySerializedDictionary<string, Level> { }
}
