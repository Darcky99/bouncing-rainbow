using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    [SerializeField] private SquadsLevel m_SquadLevel;
    private List<LineArc> m_LineList = new List<LineArc>();
    private Dictionary<int, int> m_LinesByLevel = new Dictionary<int, int>();

    public SquadsLevel SquadLevel { get { return m_SquadLevel; } }
    public Dictionary<int, int> LinesByLevel { get { return m_LinesByLevel; } }

    #region Dependencies
    private PoolManager m_PoolManager => PoolManager.Instance;
    private GameConfig m_GameConfig => GameConfig.Instance;
    private GamePlayVariablesEditor m_GamePlayVars => GameConfig.Instance.GamePlay;
    #endregion

    #region Specific

    #region Add and remove
    public bool CanAddLine()
    {
        int i_value = 0;

        for (int i_level = SquadLevel.LineTypeCount; i_level >= 1; i_level--) {
            m_LinesByLevel.TryGetValue(i_level, out i_value);
            if (i_value < 1) return true;
        }
        return false;
    }
    private void addLine(int i_Level = 1)
    {
        addLine(m_SquadLevel.GetLineType(i_Level));
    }
    private void addLines(int i_Level, int i_Amount)
    {
        for (int i = 0; i < i_Amount; i++)
            addLine(i_Level);
    }
    private void addLine(LineArcSO i_LineSO)
    {
        //get line prefab from pool...
        Vector3 i_linePosition = new Vector3(0f, 0.06f, 0f);
        LineArc i_lineArc = m_PoolManager.Dequeue(ePoolType.ArcLines, null, null, null, transform, true).GetComponent<LineArc>();
        i_lineArc.transform.localPosition = i_linePosition;
        i_lineArc.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        //add it to the list and dictionary.
        m_LineList.Add(i_lineArc);
        m_LinesByLevel[i_LineSO.Level] = (m_LinesByLevel.TryGetValue(i_LineSO.Level, out int i_value)) ? ++i_value : 1;
        //Init line
        float i_lineLenght = m_SquadLevel.StartingDistance + (m_SquadLevel.DistanceBetween * i_LineSO.Level);
        i_lineArc.Initialize(this, i_LineSO, i_lineLenght, m_LinesByLevel[i_LineSO.Level] - 1);
    }
    private void removeLines(int i_Level, int i_DeleteAmount)
    {
        if (!m_LinesByLevel.TryGetValue(i_Level, out int i_lineCount) || i_lineCount < i_DeleteAmount)
        {
            Debug.LogWarning("Trying to delete lines of level " + i_Level + " | Lines found: " + i_lineCount);
            return;
        }

        LineArc[] i_linesTodDelete = new LineArc[i_DeleteAmount];
        int i_listCount = 0;
        int i_orderThreshol = m_LinesByLevel[i_Level] - i_DeleteAmount;

        foreach (LineArc i_line in m_LineList) {
            if (i_line.LineType.Level == i_Level && i_line.Order >= i_orderThreshol) {
                i_linesTodDelete[i_listCount] = i_line;
                i_listCount++;
                if (i_listCount >= i_DeleteAmount) break;
            }
        }
        foreach (LineArc i_toDelete in i_linesTodDelete)
            removeLine(i_toDelete);
    }
    private void removeLine(LineArc i_Line)
    {
        m_LineList.Remove(i_Line);
        m_LinesByLevel[i_Line.LineType.Level]--;

        i_Line.OnQueue();
        m_PoolManager.Queue(ePoolType.ArcLines, i_Line.gameObject);
    }
    public void CleanSquad()
    {
        List<LineArc> i_toDelete = new List<LineArc>();

        foreach (LineArc i_line in m_LineList)
            i_toDelete.Add(i_line);

        foreach (LineArc i_line in i_toDelete)
            removeLine(i_line);
    }
    public void LoadSquad(SaveSquadsLevel.SquadData i_Data)
    {
        foreach (int i_level in i_Data.LinesByLevel.Keys)
            addLines(i_level, i_Data.LinesByLevel[i_level]);
    }

    #region Testing
    [Button]
    private void addAllLines()
    {
        LineArcSO[] i_AllLines = m_SquadLevel.GetAllLineTypes();

        for (int i = 0; i < i_AllLines.Length; i++)
            addLine(i_AllLines[i]);
    }
    #endregion

    #endregion

    #region Line count
    public int LineCount { get { return m_LineList.Count; } }
    public int GetLineCount(int i_Level)
    {
        return m_LinesByLevel[i_Level];
    }
    public int GetPotential(int i_Level)
    {
        m_LinesByLevel.TryGetValue(i_Level, out int i_lineCount);

        if(i_lineCount < m_GamePlayVars.AmountToMerge)
        {
            return i_lineCount; // 0 ---> AM-1
        }
        else if(i_lineCount == m_GamePlayVars.AmountToMerge)
        {
            return -1; // 0
        }
        else if (i_lineCount > m_GamePlayVars.AmountToMerge)
        {
            return m_GamePlayVars.AmountToMerge - i_lineCount - 1; // -1 ---> (-infinity)
        }
        Debug.LogError("i_lineCount or AmountToMerge set to invalid numbers");
        return 0;
    }
    #endregion

    #region Line merging
    private bool canMergeLines()
    {
        //check every level of lines, see if we have above x.
        foreach(int i_count in m_LinesByLevel.Values)
        {
            if (i_count >= m_GamePlayVars.AmountToMerge) return true;
        }
        return false;
    }
    public bool CanMergeLines(int i_Level)
    {
        //Use try get value, if 0 then return false-
        m_LinesByLevel.TryGetValue(i_Level, out int value);
        return value >= m_GamePlayVars.AmountToMerge;
    }
    private void mergeLines()
    {
        if (canMergeLines() == false) return;
        
        for(int i = 1; i < m_SquadLevel.LineTypeCount ; i++)
        {
            if(m_LinesByLevel[i] >= m_GamePlayVars.AmountToMerge) {
                mergeLines(i);
                break;
    }   }   }
    private void mergeLines(int i_LineLevel)
    {
        if (CanMergeLines(i_LineLevel) == false) return;

        removeLines(i_LineLevel, m_GamePlayVars.AmountToMerge);
        addLine(i_LineLevel + 1);
    }
    #endregion

    #region For Player
    public void AddLine()
    {
        int i_count;
        for (int i_currentLevel = 1; i_currentLevel <= SquadLevel.LineTypeCount; i_currentLevel++)
        {
            m_LinesByLevel.TryGetValue(i_currentLevel, out i_count);

            if (i_count < 1) {
                addLine(i_currentLevel);
                return;
            }
        }
        Debug.LogError("Called addline when we can't add any");
    }
    public void MergeLines()
    {
        //From lower to higher...
        mergeLines();
    }
    #endregion

    #endregion
}
