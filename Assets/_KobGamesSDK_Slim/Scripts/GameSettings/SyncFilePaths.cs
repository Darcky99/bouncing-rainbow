using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;



public class SyncFilePaths : MonoBehaviour
{
    [InlineButton(nameof(addFileToArray))]
    public UnityEngine.Object FileObject;

    //[ListDrawerSettings(OnBeginListElementGUI = "BeginGUI", OnEndListElementGUI = "EndGUI", ShowPaging = false)]
    [PropertyOrder(3)]
    public List<FilePath> Paths = new List<FilePath>();

    [Button, PropertyOrder(2)]
    private void DisableAllFilesPath()
    {
        foreach (var filePath in Paths)
        {
            filePath.IsEnabled = false;
        }
    }

    private void addFileToArray()
    {
#if UNITY_EDITOR
        if (FileObject != null)
        {
            Paths.Add(new FilePath(UnityEditor.AssetDatabase.GetAssetPath(FileObject), true));
        }
#endif
    }

#if UNITY_EDITOR
    private void BeginGUI(int i_Index)
    {
        GUILayout.BeginHorizontal();
    }

    private void EndGUI(int i_Index)
    {
        if (i_Index < Paths.Count)// && Paths[i_Index].gameObject != null)
        {
            var icon = Paths[i_Index].IsEnabled ? Sirenix.Utilities.Editor.EditorIcons.Checkmark : Sirenix.Utilities.Editor.EditorIcons.X;

            if (Sirenix.Utilities.Editor.SirenixEditorGUI.ToolbarButton(icon))
            {
                //HideAllScreens();

                //m_Screen = ScreensList[i_Index];
                //m_Screen.gameObject.SetActive(!m_Screen.gameObject.activeSelf);
            }
        }

        GUILayout.EndHorizontal();
    }
#endif
}

[Serializable]
public class FilePath
{
    [HorizontalGroup("1"), HideLabel] public string Path;
    [HorizontalGroup("1", Width = 20), HideLabel] public bool IsEnabled = true;
    public FilePath(string i_Path, bool i_IsEnabled)
    {
        Path = i_Path;
        IsEnabled = i_IsEnabled;
    }

    public FilePath()
    {
        IsEnabled = true;
    }
}
