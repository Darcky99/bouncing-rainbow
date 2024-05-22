#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used to render Bottleneck Scan Window - where all tests are being run and reports evaluated by the user
    /// </summary>
    [System.Serializable]
    public class PS_Window_BottleneckScan : EditorWindow
    {
        private static PS_Window_BottleneckScan PS_BottleneckScan_Window;

        private static PS_Window_BottleneckScan Instance { get; set; }

        private static PS_Data_BottleneckResults PSData_Results;
        private static PS_Data_BottleneckSettings PSData_Settings;

        private bool filterPanelEnabled = false;

        private bool selected_ALL_Default = false;
        private bool selected_ALL_Dismissed = false;
        private bool selected_Something = false;

        private int nResults = 0;

        public Vector2 scrollView_side, scrollView_results;
        public List<PS_Card> ResultCards_ALL = new List<PS_Card>();

        private GUIStyle popupStyle;

        private Rect topBarArea;

        public List<PS_Card>
        ResultCards_AUDIO = new List<PS_Card>(),
        ResultCards_CODE = new List<PS_Card>(),
        ResultCards_LIGHT = new List<PS_Card>(),
        ResultCards_MESH = new List<PS_Card>(),
        ResultCards_PHYSICS = new List<PS_Card>(),
        ResultCards_EDITOR = new List<PS_Card>(),
        ResultCards_SHADER = new List<PS_Card>(),
        ResultCards_TEXTURE = new List<PS_Card>(),
        ResultCards_UI = new List<PS_Card>(),
        ResultCards_PARTICLE = new List<PS_Card>();

        public float sideBarWidth = 200;
        private float currentSideBarWidth = 200;

        private bool isDragging = false;

        public ORDERING resultOrdering = ORDERING.SortByCategory;
        public DISPLAY displayMode = DISPLAY.ShowDefault;

        public enum ORDERING
        { SortByResultID, SortByCategory, SortByAffectedObjects }

        public enum DISPLAY
        { ShowDefault, ShowDismissed }

        private Texture2D
            box_1,
            box_2,
            box_3;

        private void OnEnable()
        {
            PSData_Results = PS_Utils.GetData_BottleneckResults();
            PSData_Settings = PS_Utils.GetData_BottleneckSettings();

            FetchResults();
        }

        private void OnGUI()
        {
            PSData_Results = PS_Utils.GetData_BottleneckResults();
            PSData_Settings = PS_Utils.GetData_BottleneckSettings();

            nResults = (ResultCards_ALL.Count - PSData_Results.dismissedResultsID.Count);

            popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.margin = new RectOffset(0, 0, 6, 0);

            if (PS_Utils.ProSkinEnabled())
            {
                GUI.skin = (GUISkin)AssetDatabase.LoadAssetAtPath(PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/PS_Skin_Dark.guiskin", typeof(GUISkin));

                box_2 = (Texture2D)AssetDatabase.LoadAssetAtPath(PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_box_2_dark.png", typeof(Texture2D));
                box_3 = (Texture2D)AssetDatabase.LoadAssetAtPath(PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_box_3_dark.png", typeof(Texture2D));
            }
            else
            {
                GUI.skin = (GUISkin)AssetDatabase.LoadAssetAtPath(PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/PS_Skin_Light.guiskin", typeof(GUISkin));

                box_2 = (Texture2D)AssetDatabase.LoadAssetAtPath(PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_box_2.png", typeof(Texture2D));
                box_3 = (Texture2D)AssetDatabase.LoadAssetAtPath(PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/Items/ps_box_3.png", typeof(Texture2D));
            }

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.margin = new RectOffset(0, 0, 0, 0);
            boxStyle.padding = new RectOffset(0, 1, 0, 1);

            Repaint();

            //RENDER SIDE PANEL
            if (PSData_Settings.SHOW_SIDEBAR)
            {
                currentSideBarWidth = sideBarWidth;
                GUILayout.BeginArea(new Rect(3, 3, sideBarWidth, position.height - 53));
                EditorGUILayout.BeginVertical(boxStyle);
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Test Categories");

                GUILayout.BeginHorizontal(GUILayout.Width(sideBarWidth - 9));

                if (GUILayout.Button(new GUIContent("All", "Select all tests")))
                {
                    PSData_Settings.AUDIO_TESTS_ENABLED = true;
                    PSData_Settings.CODE_TESTS_ENABLED = true;
                    PSData_Settings.LIGHT_TESTS_ENABLED = true;
                    PSData_Settings.MESH_TESTS_ENABLED = true;
                    PSData_Settings.PHYSICS_TESTS_ENABLED = true;
                    PSData_Settings.EDITOR_TESTS_ENABLED = true;
                    PSData_Settings.SHADER_TESTS_ENABLED = true;
                    PSData_Settings.TEXTURE_TESTS_ENABLED = true;
                    PSData_Settings.UI_TESTS_ENABLED = true;
                    PSData_Settings.PARTICLE_TESTS_ENABLED = true;
                }

                if (GUILayout.Button(new GUIContent("None", "De-select all tests")))
                {
                    PSData_Settings.AUDIO_TESTS_ENABLED = false;
                    PSData_Settings.CODE_TESTS_ENABLED = false;
                    PSData_Settings.LIGHT_TESTS_ENABLED = false;
                    PSData_Settings.MESH_TESTS_ENABLED = false;
                    PSData_Settings.PHYSICS_TESTS_ENABLED = false;
                    PSData_Settings.EDITOR_TESTS_ENABLED = false;
                    PSData_Settings.SHADER_TESTS_ENABLED = false;
                    PSData_Settings.TEXTURE_TESTS_ENABLED = false;
                    PSData_Settings.UI_TESTS_ENABLED = false;
                    PSData_Settings.PARTICLE_TESTS_ENABLED = false;
                }

                if (GUILayout.Button(new GUIContent("Inverse", "Inverse tests (selected tests become deselected and vise versa)")))
                {
                    PSData_Settings.AUDIO_TESTS_ENABLED = !PSData_Settings.AUDIO_TESTS_ENABLED;
                    PSData_Settings.CODE_TESTS_ENABLED = !PSData_Settings.CODE_TESTS_ENABLED;
                    PSData_Settings.LIGHT_TESTS_ENABLED = !PSData_Settings.LIGHT_TESTS_ENABLED;
                    PSData_Settings.MESH_TESTS_ENABLED = !PSData_Settings.MESH_TESTS_ENABLED;
                    PSData_Settings.PHYSICS_TESTS_ENABLED = !PSData_Settings.PHYSICS_TESTS_ENABLED;
                    PSData_Settings.EDITOR_TESTS_ENABLED = !PSData_Settings.EDITOR_TESTS_ENABLED;
                    PSData_Settings.SHADER_TESTS_ENABLED = !PSData_Settings.SHADER_TESTS_ENABLED;
                    PSData_Settings.TEXTURE_TESTS_ENABLED = !PSData_Settings.TEXTURE_TESTS_ENABLED;
                    PSData_Settings.UI_TESTS_ENABLED = !PSData_Settings.UI_TESTS_ENABLED;
                    PSData_Settings.PARTICLE_TESTS_ENABLED = !PSData_Settings.PARTICLE_TESTS_ENABLED;
                }

                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                scrollView_side = GUILayout.BeginScrollView(scrollView_side, GUIStyle.none, GUI.skin.verticalScrollbar);
                GUILayout.BeginVertical();

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.alignment = TextAnchor.MiddleLeft;
                buttonStyle.margin = new RectOffset(4, 4, 0, 4);

                PSData_Settings.AUDIO_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.AUDIO_TESTS_ENABLED, new GUIContent(" Audio Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.AUDIO)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.CODE_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.CODE_TESTS_ENABLED, new GUIContent(" Code Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.CODE)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.LIGHT_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.LIGHT_TESTS_ENABLED, new GUIContent(" Light Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.LIGHTING)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.MESH_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.MESH_TESTS_ENABLED, new GUIContent(" Mesh Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.MESH)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.PHYSICS_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.PHYSICS_TESTS_ENABLED, new GUIContent(" Physics Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.PHYSICS)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.EDITOR_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.EDITOR_TESTS_ENABLED, new GUIContent(" Editor Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.EDITOR)), buttonStyle, GUILayout.Height(23));
                //PSData_Settings.SHADER_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.SHADER_TESTS_ENABLED, new GUIContent(" Shader Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.SHADER)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.TEXTURE_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.TEXTURE_TESTS_ENABLED, new GUIContent(" Texture Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.TEXTURE)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.UI_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.UI_TESTS_ENABLED, new GUIContent(" UI Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.UI)), buttonStyle, GUILayout.Height(23));
                PSData_Settings.PARTICLE_TESTS_ENABLED = GUILayout.Toggle(PSData_Settings.PARTICLE_TESTS_ENABLED, new GUIContent(" Particle Tests", PS_Utils.GetCategoryIcon(PS_Result.CATEGORY.PARTICLE)), buttonStyle, GUILayout.Height(23));

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                if (GUI.Button(new Rect(3, position.height - 45, sideBarWidth, 40), "Run Tests"))
                {
                    RunTest();
                }

                SidePanelResizeEvent();
            }
            else
            {
                currentSideBarWidth = -3;
            }

            //RENDER TOP PANEL
            float resultHeightPos = 0;

            if (ResultCards_ALL.Count > 0)
            {
                resultHeightPos = 60;

                GUILayout.BeginArea(new Rect(currentSideBarWidth + 5, 3, position.width - (currentSideBarWidth + 7), 60));
                GUIStyle topPanelBox = new GUIStyle(GUI.skin.box);
                topPanelBox.margin = new RectOffset(topPanelBox.margin.left, topPanelBox.margin.right, 0, 0);

                topBarArea = EditorGUILayout.BeginHorizontal(topPanelBox);

                filterPanelEnabled = GUILayout.Toggle(filterPanelEnabled, "Show Filters", "Button", GUILayout.Width(100));

                selected_Something = ResultCards_ALL.Where(x => x.IsSelected).Any();

                if (displayMode == DISPLAY.ShowDefault)
                {
                    if (selected_Something)
                    {
                        if (GUILayout.Button("Dismiss Selected", GUILayout.Width(110)))
                        {
                            for (int i = 0; i < ResultCards_ALL.Count; i++)
                            {
                                PS_Card card = ResultCards_ALL[i];

                                if (card.IsSelected && !card.IsDismissed)
                                {
                                    card.ToggleDismiss();
                                    card.IsSelected = false;
                                    selected_ALL_Default = false;
                                    selected_Something = false;
                                }
                            }
                        }
                    }
                }
                else if (displayMode == DISPLAY.ShowDismissed)
                {
                    if (selected_Something)
                    {
                        if (GUILayout.Button("Restore Selected", GUILayout.Width(110)))
                        {
                            for (int i = 0; i < ResultCards_ALL.Count; i++)
                            {
                                PS_Card card = ResultCards_ALL[i];

                                if (card.IsSelected && card.IsDismissed)
                                {
                                    card.ToggleDismiss();
                                    card.IsSelected = false;
                                    selected_ALL_Dismissed = false;
                                    selected_Something = false;
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                GUI.changed = false;
                displayMode = (DISPLAY)EditorGUILayout.EnumPopup(displayMode, popupStyle, GUILayout.Width(120));
                if (GUI.changed)
                {
                    selected_ALL_Default = false;
                    selected_ALL_Dismissed = false;
                    selected_Something = false;

                    for (int i = 0; i < ResultCards_ALL.Count; i++)
                    {
                        ResultCards_ALL[i].IsSelected = false;
                    }
                }

                GUILayout.Space(5);

                GUI.changed = false;
                resultOrdering = (ORDERING)EditorGUILayout.EnumPopup(resultOrdering, popupStyle, GUILayout.Width(145));

                if (GUI.changed)
                {
                    SortResults();
                }

                if (GUILayout.Button("Expand All", GUILayout.Width(100)))
                {
                    ExpandAll();
                }

                if (GUILayout.Button("Collapse All", GUILayout.Width(100)))
                {
                    CollapseAll();
                }

                if (GUILayout.Button(new GUIContent("", "Bottleneck Settings"), "IconButton_Settings", GUILayout.Width(20)))
                {
                    PS_WindowManager.Init_Window_BottleneckSettings();
                }

                EditorGUILayout.EndHorizontal();

                //RENDER FILTERS PANEL (IF NECESSARY)
                if (!filterPanelEnabled)
                {
                    resultHeightPos = 35;
                }

                ShowFilterPanel();

                GUILayout.EndArea();
            }
            else
            {
                resultHeightPos = 6;
            }

            //RENDER RESULT CARDS
            GUILayout.BeginArea(new Rect(currentSideBarWidth + 5, resultHeightPos - 3, position.width - (currentSideBarWidth + 7), position.height - (resultHeightPos + 1)));

            GUILayout.BeginVertical("box");
            scrollView_results = GUILayout.BeginScrollView(scrollView_results, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
            GUIStyle customBox = new GUIStyle(GUI.skin.box);
            customBox.normal.background = box_2;
            customBox.margin = new RectOffset(0, customBox.margin.right + 1, 0, 0);

            if (displayMode == DISPLAY.ShowDefault)
            {
                EditorGUILayout.BeginVertical(customBox);

                if (ResultCards_ALL.Count == 0)
                    topBarArea = EditorGUILayout.BeginHorizontal();
                else
                    EditorGUILayout.BeginHorizontal();

                GUI.changed = false;

                if (nResults > 0)
                {
                    selected_ALL_Default = EditorGUILayout.Toggle(selected_ALL_Default, GUILayout.Width(10), GUILayout.ExpandWidth(false));
                }

                if (GUI.changed)
                {
                    ToggleAllResultSelections(selected_ALL_Default);
                }

                string sResults = "0 ";

                if (nResults > 0)
                {
                    sResults = nResults.ToString() + " ";
                }

                EditorGUILayout.LabelField(sResults + "Potential Performance Bottlenecks", EditorStyles.boldLabel);
                if (ResultCards_ALL.Count == 0)
                {
                    if (GUILayout.Button(new GUIContent("", "Bottleneck Settings"), "IconButton_Settings", GUILayout.Width(20)))
                    {
                        PS_WindowManager.Init_Window_BottleneckSettings();
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                RenderResults();
            }
            else if (displayMode == DISPLAY.ShowDismissed)
            {
                EditorGUILayout.BeginVertical(customBox);
                EditorGUILayout.BeginHorizontal();
                GUI.changed = false;

                if (PSData_Results.dismissedResultsID.Count > 0)
                {
                    selected_ALL_Dismissed = EditorGUILayout.Toggle(selected_ALL_Dismissed, GUILayout.Width(10), GUILayout.ExpandWidth(false));
                }

                if (GUI.changed)
                {
                    ToggleAllResultSelections(selected_ALL_Dismissed);
                }

                EditorGUILayout.LabelField(PSData_Results.dismissedResultsID.Count + " Dismissed Results", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                RenderDismissedResults();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();

            PanelContextMenu(topBarArea);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(PSData_Settings);
            }
        }

        /// <summary>
        /// Expands all Result Cards. If it's already been expanded then Affected Objects List will be displayed as well
        /// </summary>
        private void ExpandAll()
        {
            for (int i = 0; i < ResultCards_ALL.Count; i++)
            {
                PS_Card card = ResultCards_ALL[i];

                if (displayMode == DISPLAY.ShowDefault)
                {
                    if (!card.IsDismissed)
                    {
                        if (!card.IsExpanded)
                        {
                            card.UpdateScenesList();
                            card.SortObjectsByCurrentScene();
                            card.ToggleCard();
                        }
                        else
                        {
                            card.showAffectedObjects = true;
                        }
                    }
                }
                else if (displayMode == DISPLAY.ShowDismissed)
                {
                    if (card.IsDismissed)
                    {
                        if (!card.IsExpanded)
                        {
                            card.UpdateScenesList();
                            card.SortObjectsByCurrentScene();
                            card.IsExpanded = true;
                        }
                        else
                        {
                            card.showAffectedObjects = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Collapses all Result Cards
        /// </summary>
        private void CollapseAll()
        {
            for (int i = 0; i < ResultCards_ALL.Count; i++)
            {
                PS_Card card = ResultCards_ALL[i];

                if (displayMode == DISPLAY.ShowDefault)
                {
                    if (!card.IsDismissed && card.IsExpanded)
                    {
                        card.ToggleCard();
                        card.showAffectedObjects = false;
                    }
                }
                else if (displayMode == DISPLAY.ShowDismissed)
                {
                    if (card.IsDismissed && card.IsExpanded)
                    {
                        card.ToggleCard();
                        card.showAffectedObjects = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sorts results based on a given order
        /// </summary>
        private void SortResults()
        {
            switch (resultOrdering)
            {
                case (ORDERING.SortByCategory):
                    ResultCards_ALL = ResultCards_ALL.OrderBy(x => x.RESULT.ResultCategory).ToList();
                    break;

                case (ORDERING.SortByAffectedObjects):
                    ResultCards_ALL = ResultCards_ALL.OrderByDescending(x => x.RESULT.affectedObjects.Count).ToList();
                    break;

                case (ORDERING.SortByResultID):
                    ResultCards_ALL = ResultCards_ALL.OrderBy(x => x.RESULT.ID).ToList();
                    break;
            }

            scrollView_results = Vector2.zero;
        }

        /// <summary>
        /// Runs the BottleneckTest and passes in parameters from Bottleneck Settings
        /// </summary>
        private void RunTest()
        {
            PSData_Results.bottleneckResults = new PS_BottleneckTest(
                PSData_Settings.PHYSICS_TESTS_ENABLED,
                PSData_Settings.TEXTURE_TESTS_ENABLED,
                PSData_Settings.MESH_TESTS_ENABLED,
                PSData_Settings.AUDIO_TESTS_ENABLED,
                PSData_Settings.EDITOR_TESTS_ENABLED,
                PSData_Settings.CODE_TESTS_ENABLED,
                PSData_Settings.SHADER_TESTS_ENABLED,
                PSData_Settings.UI_TESTS_ENABLED,
                PSData_Settings.LIGHT_TESTS_ENABLED,
                PSData_Settings.PARTICLE_TESTS_ENABLED);

            PSData_Results.bottleneckResults.Execute();

            selected_ALL_Default = false;
            selected_ALL_Dismissed = false;

            displayMode = DISPLAY.ShowDefault;

            FetchResults();
        }

        /// <summary>
        /// Deletes old Result Cards and then converts Results from tests into Result Cards. They are then sorted through SortResults() in the end
        /// </summary>
        public void FetchResults()
        {
            if (PSData_Results.bottleneckResults != null)
            {
                ResultCards_ALL.Clear();

                ResultCards_AUDIO.Clear();
                ResultCards_CODE.Clear();
                ResultCards_LIGHT.Clear();
                ResultCards_MESH.Clear();
                ResultCards_PHYSICS.Clear();
                ResultCards_EDITOR.Clear();
                ResultCards_SHADER.Clear();
                ResultCards_TEXTURE.Clear();
                ResultCards_UI.Clear();
                ResultCards_PARTICLE.Clear();

                if (PSData_Results.bottleneckResults.RESULTS_AUDIO != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_AUDIO.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_AUDIO[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_AUDIO.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_CODE != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_CODE.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_CODE[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_CODE.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_LIGHT != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_LIGHT.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_LIGHT[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_LIGHT.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_MESH != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_MESH.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_MESH[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_MESH.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_PHYSICS != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_PHYSICS.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_PHYSICS[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_PHYSICS.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_EDITOR != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_EDITOR.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_EDITOR[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_EDITOR.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_SHADER != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_SHADER.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_SHADER[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_SHADER.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_TEXTURE != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_TEXTURE.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_TEXTURE[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_TEXTURE.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_UI != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_UI.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_UI[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_UI.Add(newCard);
                        }
                    }
                }

                if (PSData_Results.bottleneckResults.RESULTS_PARTICLE != null)
                {
                    for (int i = 0; i < PSData_Results.bottleneckResults.RESULTS_PARTICLE.Count; i++)
                    {
                        PS_Result result = PSData_Results.bottleneckResults.RESULTS_PARTICLE[i];

                        if (!result.hasPassed)
                        {
                            PS_Card newCard = new PS_Card(PSData_Results, result, PSData_Results.bottleneckResults.projectNamesList);

                            ResultCards_ALL.Add(newCard);
                            ResultCards_PARTICLE.Add(newCard);
                        }
                    }
                }

                for (int i = 0; i < PSData_Results.dismissedResultsID.Count; i++)
                {
                    if (!ResultCards_ALL.Any(x => x.RESULT.ID == PSData_Results.dismissedResultsID[i]))
                    {
                        PSData_Results.dismissedResultsID.RemoveAt(i);
                    }
                }

                for (int i = 0; i < ResultCards_ALL.Count; i++)
                {
                    PS_Card card = ResultCards_ALL[i];

                    int resultIndex = PSData_Results.dismissedResultsID.IndexOf(card.RESULT.ID);

                    if (resultIndex != -1)
                    {
                        card.IsDismissed = true;
                    }
                }

                SortResults();
            }
        }

        /// <summary>
        /// Renders a Filter Panel from which user could then choose what Category results to see and which to hide
        /// </summary>
        private void ShowFilterPanel()
        {
            if (filterPanelEnabled)
            {
                GUIStyle filterBox = new GUIStyle(GUI.skin.box);
                filterBox.normal.background = box_3;
                filterBox.margin = new RectOffset(filterBox.margin.left, filterBox.margin.right, 0, 0);
                filterBox.padding = new RectOffset(0, 0, 0, 0);
                GUILayout.BeginHorizontal(filterBox);

                GUI.changed = false;

                if (ResultCards_AUDIO.Count > 0) PSData_Settings.SHOW_AUDIO_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_AUDIO_RESULTS, "Audio (" + ResultCards_AUDIO.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_CODE.Count > 0) PSData_Settings.SHOW_CODE_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_CODE_RESULTS, "Code (" + ResultCards_CODE.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_LIGHT.Count > 0) PSData_Settings.SHOW_LIGHT_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_LIGHT_RESULTS, "Lighting (" + ResultCards_LIGHT.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_MESH.Count > 0) PSData_Settings.SHOW_MESH_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_MESH_RESULTS, "Mesh (" + ResultCards_MESH.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_PHYSICS.Count > 0) PSData_Settings.SHOW_PHYSICS_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_PHYSICS_RESULTS, "Physics (" + ResultCards_PHYSICS.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_EDITOR.Count > 0) PSData_Settings.SHOW_EDITOR_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_EDITOR_RESULTS, "Editor (" + ResultCards_EDITOR.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_SHADER.Count > 0) PSData_Settings.SHOW_SHADER_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_SHADER_RESULTS, "Shader (" + ResultCards_SHADER.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_TEXTURE.Count > 0) PSData_Settings.SHOW_TEXTURE_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_TEXTURE_RESULTS, "Texture (" + ResultCards_TEXTURE.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_UI.Count > 0) PSData_Settings.SHOW_UI_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_UI_RESULTS, "UI (" + ResultCards_UI.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));
                if (ResultCards_PARTICLE.Count > 0) PSData_Settings.SHOW_PARTICLE_RESULTS = GUILayout.Toggle(PSData_Settings.SHOW_PARTICLE_RESULTS, "Particle (" + ResultCards_PARTICLE.Count + ")", "Button", GUILayout.Width(100), GUILayout.Height(17));

                if (GUI.changed)
                {
                    UpdateFilters();
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Selects all Result Cards based on whether it's dismissed or default list
        /// </summary>
        /// <param name="b">Boolean parameter. True, means Result Cards will be selected. False, means Result Cards will be deselected</param>
        private void ToggleAllResultSelections(bool b)
        {
            selected_Something = b;

            if (displayMode == DISPLAY.ShowDefault)
            {
                for (int i = 0; i < ResultCards_ALL.Count; i++)
                {
                    if (!ResultCards_ALL[i].IsDismissed)
                    {
                        ResultCards_ALL[i].IsSelected = b;
                    }
                }
            }
            else if (displayMode == DISPLAY.ShowDismissed)
            {
                for (int i = 0; i < ResultCards_ALL.Count; i++)
                {
                    if (ResultCards_ALL[i].IsDismissed)
                    {
                        ResultCards_ALL[i].IsSelected = b;
                    }
                }
            }
        }

        /// <summary>
        /// Renders Result Cards from Default List. If there are no results, it will render an appropriate message instead
        /// </summary>
        private void RenderResults()
        {
            GUILayout.BeginVertical();

            float cardWidth = position.width - (currentSideBarWidth + 35);

            if (nResults > 0)
            {
                UpdateFilters();

                for (int i = 0; i < ResultCards_ALL.Count; i++)
                {
                    PS_Card card = ResultCards_ALL[i];

                    if (!card.IsDismissed)
                    {
                        card.Render(cardWidth);
                    }
                }
            }
            else
            {
                if (ResultCards_ALL.Count == 0)
                {
                    EditorGUILayout.HelpBox("No problems have been found. From time to time, be sure to re-run the test to check for potential performance improvements!", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("There are no results here. However, there are " + PSData_Results.dismissedResultsID.Count + " results in DISMISSED list. To see them, select \"Show Dismissed\" in the drop-down on top", MessageType.Info);
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Render Result Cards from Dismissed List. If there are no results, it will render an appropriate message instead
        /// </summary>
        private void RenderDismissedResults()
        {
            GUILayout.BeginVertical();

            float cardWidth = position.width - (currentSideBarWidth + 35);

            if (ResultCards_ALL.Count > 0)
            {
                UpdateFilters();

                if (PSData_Results.dismissedResultsID.Count > 0)
                {
                    for (int i = 0; i < ResultCards_ALL.Count; i++)
                    {
                        PS_Card card = ResultCards_ALL[i];

                        if (card.IsDismissed)
                        {
                            card.Render(cardWidth);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("You haven't dismissed any results. Dismissed results will never show up in your default list. To dismiss a result, simply right-click a desired result and select \"Dismiss\"", MessageType.Info);
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Applies filter changes onto Result Cards
        /// </summary>
        private void UpdateFilters()
        {
            for (int i = 0; i < ResultCards_AUDIO.Count; i++)
            {
                PS_Card card = ResultCards_AUDIO[i];

                card.IsHidden = !PSData_Settings.SHOW_AUDIO_RESULTS;
            }

            for (int i = 0; i < ResultCards_CODE.Count; i++)
            {
                PS_Card card = ResultCards_CODE[i];

                card.IsHidden = !PSData_Settings.SHOW_CODE_RESULTS;
            }

            for (int i = 0; i < ResultCards_LIGHT.Count; i++)
            {
                PS_Card card = ResultCards_LIGHT[i];

                card.IsHidden = !PSData_Settings.SHOW_LIGHT_RESULTS;
            }

            for (int i = 0; i < ResultCards_MESH.Count; i++)
            {
                PS_Card card = ResultCards_MESH[i];

                card.IsHidden = !PSData_Settings.SHOW_MESH_RESULTS;
            }

            for (int i = 0; i < ResultCards_PHYSICS.Count; i++)
            {
                PS_Card card = ResultCards_PHYSICS[i];

                card.IsHidden = !PSData_Settings.SHOW_PHYSICS_RESULTS;
            }

            for (int i = 0; i < ResultCards_EDITOR.Count; i++)
            {
                PS_Card card = ResultCards_EDITOR[i];

                card.IsHidden = !PSData_Settings.SHOW_EDITOR_RESULTS;
            }

            for (int i = 0; i < ResultCards_SHADER.Count; i++)
            {
                PS_Card card = ResultCards_SHADER[i];

                card.IsHidden = !PSData_Settings.SHOW_SHADER_RESULTS;
            }

            for (int i = 0; i < ResultCards_TEXTURE.Count; i++)
            {
                PS_Card card = ResultCards_TEXTURE[i];

                card.IsHidden = !PSData_Settings.SHOW_TEXTURE_RESULTS;
            }

            for (int i = 0; i < ResultCards_UI.Count; i++)
            {
                PS_Card card = ResultCards_UI[i];

                card.IsHidden = !PSData_Settings.SHOW_UI_RESULTS;
            }

            for (int i = 0; i < ResultCards_PARTICLE.Count; i++)
            {
                PS_Card card = ResultCards_PARTICLE[i];

                card.IsHidden = !PSData_Settings.SHOW_PARTICLE_RESULTS;
            }
        }

        /// <summary>
        /// Calls a Resize Event for a Side Panel
        /// </summary>
        private void SidePanelResizeEvent()
        {
            Repaint();

            if (new Rect(sideBarWidth + 1, 0, 5, position.height).Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(new Rect(sideBarWidth + 1, Event.current.mousePosition.y, 140, 40), MouseCursor.ResizeHorizontal);

                if (Event.current.rawType == EventType.MouseDown)
                {
                    isDragging = true;
                }
            }

            Repaint();

            if (isDragging)
            {
                sideBarWidth = Event.current.mousePosition.x;
                sideBarWidth = Mathf.Clamp(sideBarWidth, 180, 500);
            }

            Repaint();
            if (Event.current.rawType == EventType.MouseUp)
            {
                isDragging = false;
            }
        }

        /// <summary>
        /// Used to call Context Menu from which user could Hide or Show a Sidebar
        /// </summary>
        /// <param name="targetRect">Rect at which Mouse should be to trigger this Context Menu</param>
        public void PanelContextMenu(Rect targetRect)
        {
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.ContextClick)
            {
                Vector2 mousePos = currentEvent.mousePosition;

                if (targetRect.Contains(mousePos))
                {
                    GenericMenu objectMenu = new GenericMenu();

                    objectMenu.AddItem(new GUIContent("Show/Sidebar"), PSData_Settings.SHOW_SIDEBAR, ToggleSidebar);
                    objectMenu.AddItem(new GUIContent("Show/Result Categories"), PSData_Settings.SHOW_RESULT_CATEGORIES, ToggleResultsCategory);
                    objectMenu.AddItem(new GUIContent("Show/Result ID"), PSData_Settings.SHOW_RESULT_ID, ToggleResultsID);

                    objectMenu.ShowAsContext();
                    currentEvent.Use();
                }
            }
        }

        /// <summary>
        /// Toggles Side Bar between Displayed and Hidden
        /// </summary>
        private void ToggleSidebar()
        {
            PSData_Settings.SHOW_SIDEBAR = !PSData_Settings.SHOW_SIDEBAR;
        }

        /// <summary>
        /// Toggles the results identifier.
        /// </summary>
        private void ToggleResultsID()
        {
            PSData_Settings.SHOW_RESULT_ID = !PSData_Settings.SHOW_RESULT_ID;
        }

        /// <summary>
        /// Toggles the results category.
        /// </summary>
        private void ToggleResultsCategory()
        {
            PSData_Settings.SHOW_RESULT_CATEGORIES = !PSData_Settings.SHOW_RESULT_CATEGORIES;
        }
    }
}

#endif