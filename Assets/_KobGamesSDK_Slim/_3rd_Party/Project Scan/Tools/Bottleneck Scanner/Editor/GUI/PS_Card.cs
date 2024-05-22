#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Stores Bottleneck Results and also allows users to manage it 
    /// </summary>
    [Serializable]
    public class PS_Card
    {
        [SerializeField] public PS_Result RESULT;

        public bool IsSelected = false;
        public bool IsDismissed = false;
        public bool IsExpanded = false;
        public bool IsHidden = true;

        public Vector2 scrollView_info, scrollView_affectedObjects;

        public bool showAffectedObjects = false;

        public int currentSceneID = 0;

        private string[] fullSceneNameList;
        private string[] sceneNamesList;

        private List<PS_Object> AFFECTED_OBJECTS;
        public List<int> SelectedObjectsIndex = new List<int>();
        public int lastSelectedIndex = 0;

        private GUIStyle
            popupStyle,
            listItemBoxStyle,
            itemStyle,
            errorLabelStyle,
            linkLabelStyle,
            greyLabel;

        private Texture2D box, itemSelected;

        private AnimBool animBool_showCardSection, animBool_showAffectedObjectsSection;

        private PS_Data_BottleneckResults PSData;

        private GUIContent CardLabelContent
        {
            get
            {
                string categoryLabel = string.Format("[{0}] ", RESULT.ResultCategory.ToString());

                if (!PS_Utils.GetData_BottleneckSettings().SHOW_RESULT_CATEGORIES)
                    categoryLabel = "";

                if (IsExpanded)
                {
                    return new GUIContent(categoryLabel + RESULT.TITLE, EditorStyles.foldout.onNormal.background);
                }
                else
                {
                    return new GUIContent(categoryLabel + RESULT.TITLE, EditorStyles.foldout.normal.background);
                }
            }
        }

        public PS_Card(PS_Data_BottleneckResults PSData, PS_Result res, string[] sceneNamesList)
        {
            this.PSData = PSData;
            RESULT = res;
            fullSceneNameList = sceneNamesList;
            AFFECTED_OBJECTS = new List<PS_Object>();

            animBool_showCardSection = new AnimBool();
            animBool_showAffectedObjectsSection = new AnimBool();
        }

        Color selectedItemDefaultColor = Color.black;

        public void Render(float width)
        {
            if (!IsHidden)
            {
                EditorStyles.label.wordWrap = true;

                LoadGUISkin();

                EditorGUILayout.BeginVertical(listItemBoxStyle, GUILayout.Width(width));
                Rect cardRect = EditorGUILayout.BeginVertical();

                Render_CardHeader();

                EditorGUILayout.EndVertical();

                if (EditorGUILayout.BeginFadeGroup(animBool_showCardSection.faded))
                {
                    cardRect = EditorGUILayout.BeginVertical();

                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.SelectableLabel(RESULT.DESCRIPTION);

                    if (RESULT.URL != "")
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
                        EditorGUILayout.Space();
                        if (GUILayout.Button(
                            new GUIContent("Learn More...", "Click here to open link in your default browser"),
                            GUILayout.Width(100)))
                        {
                            System.Diagnostics.Process.Start(RESULT.URL);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("Solution", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.SelectableLabel(RESULT.SOLUTION);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();

                    if (RESULT.affectedObjects != null)
                    {
                        if (RESULT.affectedObjects.Count > 0)
                        {
                            GUILayout.BeginHorizontal();

                            showAffectedObjects = GUILayout.Toggle(showAffectedObjects,
                                "Affected Objects (" + RESULT.affectedObjects.Count + ")", "Button",
                                GUILayout.Width(135));
                            animBool_showAffectedObjectsSection.target = showAffectedObjects;

                            if (RESULT.showScenesDropdown)
                            {
                                if (sceneNamesList.Length > 1)
                                {
                                    GUILayout.Space(5);

                                    GUI.changed = false;
                                    currentSceneID = EditorGUILayout.Popup(currentSceneID, sceneNamesList, popupStyle,
                                        GUILayout.Width(135));

                                    if (GUI.changed)
                                    {
                                        SortObjectsByCurrentScene();

                                        SelectedObjectsIndex.Clear();
                                        Selection.objects = new UnityEngine.Object[0];
                                        lastSelectedIndex = -1;
                                    }
                                }
                                else if (sceneNamesList.Length == 1)
                                {
                                    currentSceneID = 0;
                                }
                            }

                            GUILayout.EndHorizontal();

                            GUILayout.Space(3);

                            if (EditorGUILayout.BeginFadeGroup(animBool_showAffectedObjectsSection.faded))
                            {
                                listItemBoxStyle.padding.bottom = 1;
                                listItemBoxStyle.padding.right = 0;

                                Color background_color_selected = new Color(62, 255, 231);

                                EditorGUILayout.BeginHorizontal();

                                float fWidth = (PS_Utils.GetData_BottleneckSettings().OBJECT_WIDTH_PERCENTAGE / 100) *
                                               width;
                                float fHeight = PS_Utils.GetData_BottleneckSettings().OBJECT_HEIGHT_AMOUNT;

                                EditorGUILayout.BeginVertical(GUILayout.Width(fWidth), GUILayout.MinWidth(135),
                                    GUILayout.Height(fHeight));

                                scrollView_affectedObjects = GUILayout.BeginScrollView(scrollView_affectedObjects,
                                    false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(fHeight));

                                Color color_background_default = GUI.backgroundColor;

                                for (int i = 0; i < AFFECTED_OBJECTS.Count; i++)
                                {
                                    if (SelectedObjectsIndex.Contains(i))
                                    {
                                        GUI.backgroundColor = background_color_selected;
                                        itemStyle.normal.textColor = Color.white;
                                    }
                                    else
                                    {
                                        GUI.backgroundColor = Color.clear;
                                        itemStyle.normal.textColor = selectedItemDefaultColor;
                                    }

                                    AFFECTED_OBJECTS[i].getData();

                                    PS_Object resultTarget = AFFECTED_OBJECTS[i];

                                    Rect objectRect;

                                    if (resultTarget.obj != null)
                                    {
                                        objectRect = EditorGUILayout.BeginVertical();
                                        GUILayout.Label(
                                            new GUIContent(" " + resultTarget.objName,
                                                AssetPreview.GetMiniThumbnail(resultTarget.obj),
                                                resultTarget.objectAssetPath), itemStyle, GUILayout.Height(20));
                                        EditorGUILayout.EndVertical();

                                        Event ev = Event.current;

                                        if (objectRect.Contains(ev.mousePosition))
                                        {
                                            if (ev.type == EventType.MouseDown && (ev.control || ev.command))
                                            {
                                                if (!SelectedObjectsIndex.Contains(i))
                                                {
                                                    SelectedObjectsIndex.Add(i);
                                                }
                                                else
                                                {
                                                    if (SelectedObjectsIndex.Count > 1)
                                                    {
                                                        SelectedObjectsIndex.Remove(i);
                                                    }
                                                }

                                                var objList = new List<UnityEngine.Object>();
                                                objList.Clear();

                                                for (int objIndex = 0;
                                                    objIndex < SelectedObjectsIndex.Count;
                                                    objIndex++)
                                                {
                                                    objList.Add(AFFECTED_OBJECTS[SelectedObjectsIndex[objIndex]].obj);
                                                }

                                                lastSelectedIndex = i;
                                                Selection.objects = objList.ToArray();
                                            }
                                            else if (ev.type == EventType.MouseDown && Event.current.shift)
                                            {
                                                //get all indexes between last selected index and the latest selected index
                                                if (lastSelectedIndex >= 0)
                                                {
                                                    if (i > lastSelectedIndex)
                                                    {
                                                        for (int selectIndex = lastSelectedIndex + 1;
                                                            selectIndex <= i;
                                                            selectIndex++)
                                                        {
                                                            if (!SelectedObjectsIndex.Contains(selectIndex))
                                                            {
                                                                SelectedObjectsIndex.Add(selectIndex);
                                                            }
                                                        }
                                                    }
                                                    else if (i < lastSelectedIndex)
                                                    {
                                                        for (int selectIndex = i;
                                                            selectIndex < lastSelectedIndex;
                                                            selectIndex++)
                                                        {
                                                            if (!SelectedObjectsIndex.Contains(selectIndex))
                                                            {
                                                                SelectedObjectsIndex.Add(selectIndex);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (!SelectedObjectsIndex.Contains(i))
                                                    {
                                                        SelectedObjectsIndex.Add(i);
                                                    }
                                                }

                                                var objList = new List<UnityEngine.Object>();

                                                for (int objIndex = 0;
                                                    objIndex < SelectedObjectsIndex.Count;
                                                    objIndex++)
                                                {
                                                    objList.Add(AFFECTED_OBJECTS[SelectedObjectsIndex[objIndex]].obj);
                                                }

                                                lastSelectedIndex =
                                                    SelectedObjectsIndex[SelectedObjectsIndex.Count - 1];
                                                Selection.objects = objList.ToArray();
                                            }
                                            else if (ev.type == EventType.MouseDown)
                                            {
                                                SelectedObjectsIndex.Clear();
                                                SelectedObjectsIndex.Add(i);

                                                EditorGUIUtility.PingObject(resultTarget.obj);

                                                if (AFFECTED_OBJECTS[i].Equals(typeof(GameObject)))
                                                {
                                                    Selection.activeGameObject = (GameObject)resultTarget.obj;
                                                }
                                                else
                                                {
                                                    Selection.activeObject = resultTarget.obj;
                                                }

                                                lastSelectedIndex = i;
                                            }
                                        }
                                        ContextMenu_AffectedObject(objectRect, resultTarget);
                                    }
                                    else
                                    {
                                        if (resultTarget.objectType == PS_Object.TYPE.FOLDER)
                                        {
                                            objectRect = EditorGUILayout.BeginVertical();
                                            GUILayout.Label(
                                                new GUIContent(resultTarget.objectAssetPath,
                                                    resultTarget.objectAssetPath), itemStyle, GUILayout.Height(20));
                                            EditorGUILayout.EndVertical();

                                            Event ev = Event.current;

                                            if (objectRect.Contains(ev.mousePosition))
                                            {
                                                if (ev.type == EventType.MouseDown)
                                                {
                                                    SelectedObjectsIndex.Clear();
                                                    SelectedObjectsIndex.Add(i);
                                                }
                                            }

                                            ContextMenu_AffectedObject(objectRect, resultTarget);
                                        }
                                        else
                                        {
                                            EditorGUILayout.LabelField(
                                                "An object is missing. It's either been deleted, or moved elsewhere. Please re-run the test to get an updated result",
                                                errorLabelStyle);
                                        }
                                    }

                                    GUI.backgroundColor = color_background_default;
                                    itemStyle.normal.textColor = selectedItemDefaultColor;
                                }

                                GUILayout.EndScrollView();
                                EditorGUILayout.EndVertical();

                                //Anything rendered here will be on the right side of Affected Objects List

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.EndFadeGroup();
                        }
                        else
                        {
                            showAffectedObjects = false;
                        }
                    }
                }

                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.EndVertical();
                EditorStyles.label.wordWrap = false;

                ContextMenu_ResultCard(cardRect);
            }
        }

        private void LoadGUISkin()
        {
            if (PS_Utils.ProSkinEnabled())
            {
                box = (Texture2D)AssetDatabase.LoadAssetAtPath(
                    PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_box_2_dark.png",
                    typeof(Texture2D));
                itemSelected = (Texture2D)AssetDatabase.LoadAssetAtPath(
                    PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_item_selected.png",
                    typeof(Texture2D));
                selectedItemDefaultColor = Color.white;
            }
            else
            {
                box = (Texture2D)AssetDatabase.LoadAssetAtPath(
                    PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_box_2.png", typeof(Texture2D));
                itemSelected = (Texture2D)AssetDatabase.LoadAssetAtPath(
                    PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_item_selected_light.png",
                    typeof(Texture2D));
            }

            popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.margin = new RectOffset(0, 0, 6, 0);

            listItemBoxStyle = new GUIStyle(GUI.skin.box);
            listItemBoxStyle.normal.background = box;
            listItemBoxStyle.margin = new RectOffset(0, 0, 0, 0);

            itemStyle = new GUIStyle(GUI.skin.button);
            itemStyle.alignment = TextAnchor.MiddleLeft;
            itemStyle.normal.background = itemSelected;
            itemStyle.normal.textColor = selectedItemDefaultColor;
            itemStyle.margin = new RectOffset(0, 0, 0, 0);

            errorLabelStyle = new GUIStyle(GUI.skin.label);
            errorLabelStyle.normal.textColor = Color.red;
            errorLabelStyle.fontStyle = FontStyle.Italic;

            linkLabelStyle = new GUIStyle(GUI.skin.label);
            linkLabelStyle.margin = new RectOffset(0, 0, 6, 0);
            linkLabelStyle.fontStyle = FontStyle.Bold;
            linkLabelStyle.normal.textColor = Color.blue;

            greyLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            greyLabel.alignment = TextAnchor.MiddleRight;
        }

        private void Render_CardHeader()
        {
            Event ev = Event.current;

            Rect cardHeaderArea = EditorGUILayout.BeginHorizontal();

            IsSelected = EditorGUILayout.Toggle(IsSelected, GUILayout.ExpandWidth(false), GUILayout.Width(10));

            GUILayout.Label(CardLabelContent);

            if (PS_Utils.GetData_BottleneckSettings().SHOW_RESULT_ID)
            {
                GUILayout.Label(RESULT.ID.ToString(), greyLabel);
            }

            EditorGUILayout.EndHorizontal();

            if (cardHeaderArea.Contains(ev.mousePosition))
            {
                if (ev.type == EventType.MouseDown && ev.button == 0 && ev.isMouse)
                {
                    ToggleCard();
                }
            }
        }

        public void ToggleCard()
        {
            Event.current.Use();

            IsExpanded = !IsExpanded;
            animBool_showCardSection.target = IsExpanded;

            UpdateScenesList();
            SortObjectsByCurrentScene();
        }

        private void ContextMenu_ResultCard(Rect targetRect)
        {
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.ContextClick)
            {
                Vector2 mousePos = currentEvent.mousePosition;
                if (targetRect.Contains(mousePos))
                {
                    GenericMenu windowMenu = new GenericMenu();

                    if (IsDismissed)
                    {
                        windowMenu.AddItem(new GUIContent("Restore"), false, ToggleDismiss);
                    }
                    else
                    {
                        windowMenu.AddItem(new GUIContent("Dismiss"), false, ToggleDismiss);
                    }

                    windowMenu.ShowAsContext();
                    currentEvent.Use();
                }
            }
        }

        private void ContextMenu_AffectedObject(Rect targetRect, PS_Object obj)
        {
            if (obj.objectType != PS_Object.TYPE.GAMEOBJECT)
            {
                Event currentEvent = Event.current;

                if (currentEvent.type == EventType.ContextClick)
                {
                    Vector2 mousePos = currentEvent.mousePosition;
                    if (targetRect.Contains(mousePos))
                    {
                        GenericMenu objectMenu = new GenericMenu();

                        if (obj.objectType == PS_Object.TYPE.ASSET)
                        {
                            objectMenu.AddItem(new GUIContent("Show in Folder"), false, RevealFile, obj.fullObjectPath);
                            objectMenu.AddItem(new GUIContent("Open File"), false, OpenFile, obj.fullObjectPath);
                            objectMenu.AddItem(new GUIContent("Copy Path"), false, CopyPath, obj.fullObjectPath);

                            objectMenu.AddItem(new GUIContent("Filters/Ignore this File"), false,
                                PS_Utils.AddFileToFilters, obj.fullObjectPath);
                            objectMenu.AddItem(new GUIContent("Filters/Ignore files of this Extension"), false,
                                PS_Utils.AddExtensionToFilters, obj.fullObjectPath);
                        }
                        else if (obj.objectType == PS_Object.TYPE.FOLDER)
                        {
                            objectMenu.AddItem(new GUIContent("Open Folder"), false, OpenFile, obj.fullObjectPath);
                            objectMenu.AddItem(new GUIContent("Copy Path"), false, CopyPath, obj.fullObjectPath);
                        }

                        objectMenu.ShowAsContext();
                        currentEvent.Use();
                    }
                }
            }
        }

        /// <summary>
        /// Copies the path to a clipboard.
        /// </summary>
        /// <param name="obj">File path to copy</param>
        private void CopyPath(object obj)
        {
            try
            {
                TextEditor tE = new TextEditor
                {
                    text = (string)obj
                };

                tE.SelectAll();
                tE.Copy();
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("<b>Project Scan</b> Error occurred while trying to Copy Path: " + e);
            }
        }

        /// <summary>
        /// Reveals a file in Explorer (or Finder).
        /// </summary>
        /// <param name="obj">File path which leads to the file</param>
        private void RevealFile(object obj)
        {
            try
            {
                EditorUtility.RevealInFinder((string)obj);
            }
            catch (Exception e)
            {
                Debug.LogError("<b>Project Scan</b> Error occurred while trying to reveal file/folder: " + e);
            }
        }

        /// <summary>
        /// Opens the file/folder.
        /// </summary>
        /// <param name="obj">File path which leads to the file</param>
        private void OpenFile(object obj)
        {
            try
            {
                EditorUtility.OpenWithDefaultApp((string)obj);
            }
            catch (Exception e)
            {
                Debug.LogError("<b>Project Scan</b> Error occurred while trying to open folder: " + e);
            }
        }

        /// <summary>
        /// Sorts the objects by current scene
        /// </summary>
        public void SortObjectsByCurrentScene()
        {
            if (RESULT.showScenesDropdown)
            {
                AFFECTED_OBJECTS.Clear();

                for (int i = 0; i < RESULT.affectedObjects.Count; i++)
                {
                    if (RESULT.affectedObjects[i].objectType == PS_Object.TYPE.GAMEOBJECT)
                    {
                        RESULT.affectedObjects[i].getData();
                        GameObject targetObj = (GameObject)RESULT.affectedObjects[i].obj;

                        if (targetObj.scene.name == SceneManager.GetSceneByName(sceneNamesList[currentSceneID]).name)
                        {
                            AFFECTED_OBJECTS.Add(RESULT.affectedObjects[i]);
                        }
                    }
                    else
                    {
                        AFFECTED_OBJECTS.Add(RESULT.affectedObjects[i]);
                    }
                }

                if (AFFECTED_OBJECTS.Any(x => x.objectType == PS_Object.TYPE.GAMEOBJECT))
                {
                    AFFECTED_OBJECTS = AFFECTED_OBJECTS.OrderBy(x => ((GameObject)x.obj).transform.GetSiblingIndex())
                        .ToList();
                }
            }
            else
            {
                AFFECTED_OBJECTS = RESULT.affectedObjects;
            }
        }

        /// <summary>
        /// Toggles the dismiss
        /// </summary>
        public void ToggleDismiss()
        {
            IsDismissed = !IsDismissed;

            IsSelected = false;
            IsExpanded = false;
            showAffectedObjects = false;

            if (IsDismissed)
            {
                PSData.dismissedResultsID.Add(RESULT.ID);
            }
            else
            {
                PSData.dismissedResultsID.Remove(RESULT.ID);
            }

            EditorUtility.SetDirty(PSData);
        }

        /// <summary>
        /// Updates the scenes list
        /// </summary>
        public void UpdateScenesList()
        {
            if (RESULT.showScenesDropdown)
            {
                if (RESULT.affectedObjects != null)
                {
                    if (RESULT.affectedObjects.Count > 0)
                    {
                        List<string> tempSceneList = new List<string>();

                        for (int i = 0; i < fullSceneNameList.Length; i++)
                        {
                            for (int j = 0; j < RESULT.affectedObjects.Count; j++)
                            {
                                PS_Object PSObj = RESULT.affectedObjects[j];

                                PSObj.getData();

                                if (PSObj.objectType == PS_Object.TYPE.GAMEOBJECT)
                                {
                                    if (((GameObject)PSObj.obj).scene.name == fullSceneNameList[i])
                                    {
                                        tempSceneList.Add(fullSceneNameList[i]);
                                        break;
                                    }
                                }
                            }
                        }

                        sceneNamesList = new string[tempSceneList.Count];

                        for (int i = 0; i < sceneNamesList.Length; i++)
                        {
                            sceneNamesList[i] = tempSceneList[i];
                        }
                    }
                }
            }
        }
    }
}

#endif