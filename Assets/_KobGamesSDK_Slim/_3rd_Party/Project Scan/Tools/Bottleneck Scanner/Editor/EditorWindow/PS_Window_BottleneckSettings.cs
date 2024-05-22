using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used to render Bottleneck Settings Window - where user can modify Bottleneck Settings
    /// </summary>
    [System.Serializable]
    public class PS_Window_BottleneckSettings : EditorWindow
    {
        private static PS_Data_BottleneckSettings PSData;
        private Vector2 scrollView_benchmarkSettings_sideBar;
        private Vector2 scrollView_treemapDirectories;

        public int currentCategoryID = 0;

        private GUIStyle sidebar_ButtonStyle;
        private GUIStyle sideBar_BoxStyle;
        private PS_DirectoryTreeEditor _treeEditor;
        private readonly string[] _settingCategories = { "General", "Benchmarks", "Filters", "Other" };

        private void OnEnable()
        {
            PSData = PS_Utils.GetData_BottleneckSettings();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// Start rendering
        /// </summary>
        private void OnGUI()
        {
            PSData = PS_Utils.GetData_BottleneckSettings();

            if (PS_Utils.ProSkinEnabled())
            {
                GUI.skin = (GUISkin)AssetDatabase.LoadAssetAtPath(
                    PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/PS_Skin_Dark.guiskin", typeof(GUISkin));
            }
            else
            {
                GUI.skin = (GUISkin)AssetDatabase.LoadAssetAtPath(
                    PS_Utils.GetData_ProjectScanPath() + "/Editor/Skins/PS_Skin_Light.guiskin", typeof(GUISkin));
            }

            sidebar_ButtonStyle = new GUIStyle(GUI.skin.button);
            sidebar_ButtonStyle.margin = new RectOffset(0, 0, 0, 3);
            sidebar_ButtonStyle.fixedHeight = 40;
            sidebar_ButtonStyle.alignment = TextAnchor.MiddleLeft;

            sideBar_BoxStyle = new GUIStyle(GUI.skin.box);
            sideBar_BoxStyle.padding = new RectOffset(1, 1, 1, 1);

            render_Sidebar();

            EditorGUIUtility.labelWidth = 180;

            switch (currentCategoryID)
            {
                case 0:
                    render_setting_General();
                    TrackChanges();
                    break;

                case 1:
                    render_setting_Benchmarks();
                    TrackChanges();
                    break;

                case 2:
                    render_setting_Filters();
                    TrackChanges();
                    break;

                case 3:
                    render_setting_Other();
                    break;
            }

            GUILayout.FlexibleSpace();
        }

        private void render_Sidebar()
        {
            GUILayout.BeginArea(new Rect(3, 3, 200, position.height - 6));
            GUILayout.BeginVertical(sideBar_BoxStyle, GUILayout.ExpandHeight(true));

            currentCategoryID = GUILayout.SelectionGrid(currentCategoryID, _settingCategories, 1, sidebar_ButtonStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void render_setting_General()
        {
            GUILayout.BeginArea(new Rect(206, 3, position.width - 209, position.height - 6));

            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            GUILayout.Label("Project", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            PSData.GENERAL_PROJECT_TYPE_ID =
                EditorGUILayout.Popup("Project Type", PSData.GENERAL_PROJECT_TYPE_ID, PSData.ProjectType);

            switch (PSData.GENERAL_PROJECT_TYPE_ID)
            {
                case 0: // Chosen "Auto"

                    EditorGUILayout.HelpBox(
                        "Project type is going to determined based on \"Default Behavior Mode\" in Editor Settings",
                        MessageType.Info);

                    if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D)
                    {
                        EditorGUILayout.HelpBox(
                            "Default Behavior Mode is set to 2D Mode. This means:\n* Mesh Tests will be completely ignored\n* You will be warned if any 3D models have been detected",
                            MessageType.Warning);
                    }

                    break;

                case 1: // Chosen "3D"

                    EditorGUILayout.HelpBox("Project is a 3D Game/Application", MessageType.Info);

                    break;

                case 2: // Chosen "2D"

                    EditorGUILayout.HelpBox("Project is a 2D Game/Application", MessageType.Info);
                    EditorGUILayout.HelpBox(
                        "Please note:\n• Mesh Tests will be completely ignored\n• You will be warned if any 3D models have been found within the project",
                        MessageType.Warning);

                    break;

                case 3: // Chosen "Mixed"

                    EditorGUILayout.HelpBox(
                        "Project is a mix of 2D and 3D elements\nE.g. 3D backgrounds with 2D characters",
                        MessageType.Info);

                    break;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            GUILayout.Label("Visual", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            PSData.SHOW_SIDEBAR = EditorGUILayout.Toggle(
                new GUIContent("Show Sidebar",
                    "If set true, the sidebar in Bottleneck Scan Window will be hidden. Good if you're not running tests often. Note that you will not be able to run tests."),
                PSData.SHOW_SIDEBAR);
            PSData.SHOW_RESULT_CATEGORIES = EditorGUILayout.Toggle(
                new GUIContent("Show Results' Categories",
                    "If set true, Category will be displayed alongside Results (Recommended)"),
                PSData.SHOW_RESULT_CATEGORIES);
            PSData.SHOW_RESULT_ID =
                EditorGUILayout.Toggle(
                    new GUIContent("Show Results' ID", "If set true, Results will show their Results ID"),
                    PSData.SHOW_RESULT_ID);
            PSData.OBJECT_WIDTH_PERCENTAGE = EditorGUILayout.Slider(
                new GUIContent("Objects List Width %",
                    "Clamped between 0 and 100, this value changes the width of Affected Objects List width\n0% - Width will be at its minimum\n100% - Width will be equal to window's width"),
                PSData.OBJECT_WIDTH_PERCENTAGE, 0, 100);
            PSData.OBJECT_HEIGHT_AMOUNT = EditorGUILayout.Slider(new GUIContent("Objects List Height", ""),
                PSData.OBJECT_HEIGHT_AMOUNT, 75, 500);

            EditorGUILayout.Space();
            PSData.SKIN_TYPE =
                (PS_Data_BottleneckSettings.SKINTYPE)EditorGUILayout.EnumPopup(new GUIContent("Skin Mode"),
                    PSData.SKIN_TYPE);

            if (PSData.SKIN_TYPE == PS_Data_BottleneckSettings.SKINTYPE.Dark)
            {
                if (!EditorGUIUtility.isProSkin)
                {
                    EditorGUILayout.HelpBox(
                        "Dark skin in a Light Skin Unity Editor can feel out of place and may be irritating for your eyes. It's advised to disable it or switch to Dark skin entirely.",
                        MessageType.Warning);
                }
            }

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void render_setting_Benchmarks()
        {
            GUILayout.BeginArea(new Rect(206, 3, position.width - 209, position.height - 6));
            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            EditorGUILayout.HelpBox(
                "These are benchmark settings for the Bottleneck Scanner. Specify limits that your project has. (E.g. max number of polygons per mesh)",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "If you don't care about a particular setting, you can set its value to 0 or lower and it'll be ignored by Bottleneck Scanner",
                MessageType.None);

            PSData.BENCHMARK_ENABLED = EditorGUILayout.Toggle("Enable Benchmark Settings", PSData.BENCHMARK_ENABLED);

            if (PSData.BENCHMARK_ENABLED)
            {
                EditorGUILayout.BeginVertical();

                scrollView_benchmarkSettings_sideBar = GUILayout.BeginScrollView(scrollView_benchmarkSettings_sideBar,
                    false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

                // commented out benchmark settings will be put back on when relevant benchmark settings will become available

                //benchmark_AUDIO();
                //benchmark_CODE();
                benchmark_EDITOR();
                //benchmark_LIGHT();
                benchmark_MESH();
                benchmark_PHYSICS();
                //benchmark_SHADER();
                benchmark_TEXTURE();
                benchmark_UI();

                GUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Benchmark Settings are disabled. Bottleneck Scanner will not run some of the tests.",
                    MessageType.Warning);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void render_setting_Filters()
        {
            GUILayout.BeginArea(new Rect(206, 3, position.width - 209, position.height - 6));
            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            GUILayout.Label("Assets", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Excluded File Extensions (Comma separated. Case-insensitive)");
            PSData.FILTER_EXCLUDE_EXTENSIONS = EditorGUILayout.TextField("", PSData.FILTER_EXCLUDE_EXTENSIONS);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Excluded File Prefixes (Comma separated. Case-insensitive)");
            PSData.FILTER_EXCLUDE_PREFIXES = EditorGUILayout.TextField("", PSData.FILTER_EXCLUDE_PREFIXES);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Excluded Files (Comma separated. Case-sensitive. Requires file extension)");
            PSData.FILTER_EXCLUDE_FILES = EditorGUILayout.TextField("", PSData.FILTER_EXCLUDE_FILES);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            GUILayout.Label("GameObjects", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Excluded GameObject Tags (Comma separated. Case-insensitive)");
            PSData.FILTER_EXCLUDE_TAGS = EditorGUILayout.TextField("", PSData.FILTER_EXCLUDE_TAGS);

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            GUILayout.Label("Project Directories", EditorStyles.boldLabel);


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Refresh")))
            {
                _treeEditor = new PS_DirectoryTreeEditor();
                _treeEditor.Initialize(new PS_Directory(_treeEditor, Application.dataPath), PSData);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(170), GUILayout.MaxHeight(position.height));
            scrollView_treemapDirectories = EditorGUILayout.BeginScrollView(scrollView_treemapDirectories);
            if (_treeEditor != null)
            {
                _treeEditor.Render();
            }
            else
            {
                _treeEditor = new PS_DirectoryTreeEditor();
                _treeEditor.Initialize(new PS_Directory(_treeEditor, Application.dataPath), PSData);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            GUILayout.Label("Scenes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            PSData.FILTER_SCENE_SCAN_MODE =
                (PS_Data_BottleneckSettings.SceneScanMode)EditorGUILayout.EnumPopup("Scene Mode",
                    PSData.FILTER_SCENE_SCAN_MODE);

            EditorGUI.indentLevel--; 
             
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void render_setting_Other()
        {
            GUILayout.BeginArea(new Rect(206, 3, position.width - 209, position.height - 6));

            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Project Scan Version", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(PS_Utils.PS_VERSION, EditorStyles.miniLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Release Date", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(PS_Utils.PS_RELEASE_DATE, EditorStyles.miniLabel);

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Reset Actions (Cannot be undone)", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset General", GUILayout.Height(25)))
            {
                bool isConfirmed = EditorUtility.DisplayDialog("Resetting General Settings",
                    "Are you sure you want to reset General Settings?\n\nThis action cannot be undone!", "Reset",
                    "Cancel");

                if (isConfirmed)
                {
                    ResetGeneral();
                }
            }

            if (GUILayout.Button("Reset Benchmarks", GUILayout.Height(25)))
            {
                bool isConfirmed = EditorUtility.DisplayDialog("Resetting Benchmarks",
                    "Are you sure you want to reset Benchmark Settings to default?\n\nThis action cannot be undone!",
                    "Reset", "Cancel");

                if (isConfirmed)
                {
                    ResetBenchmarks();
                }
            }

            if (GUILayout.Button("Reset Filters", GUILayout.Height(25)))
            {
                bool isConfirmed = EditorUtility.DisplayDialog("Resetting Filters",
                    "Are you sure you want to reset Filters?\n\nThis action cannot be undone!", "Reset", "Cancel");

                if (isConfirmed)
                {
                    ResetFilters();
                }
            }

            GUIStyle resetcolor = new GUIStyle(GUI.skin.button);
            resetcolor.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("Delete Data", resetcolor, GUILayout.Height(25)))
            {
                bool isConfirmed = EditorUtility.DisplayDialog("Delete Everything!",
                    "Are you sure you want to delete everything?\n\nAll Settings will be fully reset to default\nBottleneck Results will be deleted\n\nProceed?",
                    "Reset", "Cancel");

                if (isConfirmed)
                {
                    DeleteAll();
                }
            }

            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("For any inquiries, use one of the following links", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Official Website",
                "Official HardCode Lab website where you can keep track on latest news")))
            {
                System.Diagnostics.Process.Start("https://hardcodelab.com/");
            }

            if (GUILayout.Button(new GUIContent("Unity Forums",
                "Forum thread to Project Scan where you can ask for help!")))
            {
                System.Diagnostics.Process.Start(
                    "https://forum.unity3d.com/threads/beta-project-scan-scan-your-project-for-performance-bottlenecks-and-more.463331/");
            }

            if (GUILayout.Button(new GUIContent("Twitter",
                "HardCode Lab Twitter account to keep up to date with news or ask any question!")))
            {
                System.Diagnostics.Process.Start("https://twitter.com/HardCodeLab");
            }

            if (GUILayout.Button(new GUIContent("YouTube",
                "YouTube Channel where you can get familiar with using Project Scan")))
            {
                System.Diagnostics.Process.Start("https://www.youtube.com/c/HardCodeLab");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                new GUIContent("support@hardcodelab.com",
                    "Encountered a bug? Need help? Drop an email! Be sure to include your invoice number to speed things up!"),
                EditorStyles.miniBoldLabel);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void benchmark_AUDIO()
        {
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }

        private void benchmark_CODE()
        {
            EditorGUILayout.LabelField("Code", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }

        private void benchmark_LIGHT()
        {
            EditorGUILayout.LabelField("Light", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }

        private void benchmark_MESH()
        {
            EditorGUILayout.LabelField("Mesh", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            PSData.BENCHMARK_MESH_perMeshPolygonLimit = EditorGUILayout.IntField("Max Polygon Count per Mesh",
                PSData.BENCHMARK_MESH_perMeshPolygonLimit);

            EditorGUI.indentLevel--;
        }

        private void benchmark_PHYSICS()
        {
            EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            PSData.BENCHMARK_PHYSICS_maxRigidbodiesPerScene = EditorGUILayout.IntField("Max Rigidbodies per Scene",
                PSData.BENCHMARK_PHYSICS_maxRigidbodiesPerScene);

            EditorGUI.indentLevel--;
        }

        private void benchmark_EDITOR()
        {
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = 260;
            PSData.BENCHMARK_EDITOR_occlussionBakeDataAge = EditorGUILayout.DoubleField(
                new GUIContent("Max Occlusion Culling Bake Age (Hours)",
                    "Specify how old Occlusion Culling Bake Data can be acceptable for you"),
                PSData.BENCHMARK_EDITOR_occlussionBakeDataAge);
            PSData.BENCHMARK_EDITOR_maxFurthestDistance = EditorGUILayout.FloatField(
                new GUIContent("Maximum distance the furthest GameObject could be from other GameObjects"),
                PSData.BENCHMARK_EDITOR_maxFurthestDistance);
            EditorGUIUtility.labelWidth = 250;

            EditorGUI.indentLevel--;
        }

        private void benchmark_SHADER()
        {
            EditorGUILayout.LabelField("Shader", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }

        private void benchmark_TEXTURE()
        {
            EditorGUILayout.LabelField("Texture", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            PSData.BENCHMARK_TEXTURE_maxTextureResolution = EditorGUILayout.IntField("Max Texture Size Target",
                PSData.BENCHMARK_TEXTURE_maxTextureResolution);

            EditorGUI.indentLevel--;
        }

        private void benchmark_UI()
        {
            EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            PSData.BENCHMARK_UI_maxObjectsPerCanvas = EditorGUILayout.IntField("Max UI Components per Canvas",
                PSData.BENCHMARK_UI_maxObjectsPerCanvas);

            EditorGUI.indentLevel--;
        }

        private void TrackChanges()
        {
            if (GUI.changed)
            {
                Repaint();

                if (PSData != null)
                {
                    EditorUtility.SetDirty(PSData);
                }
            }
        }

        private void ResetBenchmarks()
        {
            PSData.BENCHMARK_ENABLED = false;
            PSData.BENCHMARK_MESH_perMeshPolygonLimit = 3000; // Polygon limit of a single MESH
            PSData.BENCHMARK_TEXTURE_maxTextureResolution = 1024; // Maximum texture resolution
            PSData.BENCHMARK_PHYSICS_maxRigidbodiesPerScene = 100; // Number of Rigidbodies in a single SCENE
            PSData.BENCHMARK_UI_maxObjectsPerCanvas = 20; // Max number of UI components per Canvas
            PSData.BENCHMARK_EDITOR_occlussionBakeDataAge = 1.0f; // How old can occlusion bake data can be?
            PSData.BENCHMARK_EDITOR_maxFurthestDistance =
                2000f; // How far away can GameObject be from other GameObjects?

            AssetDatabase.SaveAssets();
        }

        private void ResetGeneral()
        {
            PSData.AUDIO_TESTS_ENABLED = true;
            PSData.CODE_TESTS_ENABLED = true;
            PSData.LIGHT_TESTS_ENABLED = true;
            PSData.MESH_TESTS_ENABLED = true;
            PSData.PHYSICS_TESTS_ENABLED = true;
            PSData.EDITOR_TESTS_ENABLED = true;
            PSData.SHADER_TESTS_ENABLED = true;
            PSData.TEXTURE_TESTS_ENABLED = true;
            PSData.UI_TESTS_ENABLED = true;
            PSData.PARTICLE_TESTS_ENABLED = true;

            PSData.SHOW_AUDIO_RESULTS = true;
            PSData.SHOW_CODE_RESULTS = true;
            PSData.SHOW_LIGHT_RESULTS = true;
            PSData.SHOW_MESH_RESULTS = true;
            PSData.SHOW_PHYSICS_RESULTS = true;
            PSData.SHOW_EDITOR_RESULTS = true;
            PSData.SHOW_SHADER_RESULTS = true;
            PSData.SHOW_TEXTURE_RESULTS = true;
            PSData.SHOW_UI_RESULTS = true;
            PSData.SHOW_PARTICLE_RESULTS = true;

            PSData.SHOW_SIDEBAR = true;
            PSData.SHOW_RESULT_ID = false;
            PSData.SHOW_RESULT_CATEGORIES = true;
            PSData.OBJECT_WIDTH_PERCENTAGE = 30;
            PSData.OBJECT_HEIGHT_AMOUNT = 75;

            PSData.SKIN_TYPE = PS_Data_BottleneckSettings.SKINTYPE.Auto;
            PSData.GENERAL_PROJECT_TYPE_ID = 0;

            AssetDatabase.SaveAssets();
        }

        private void ResetFilters()
        {
            PSData.FILTER_EXCLUDE_EXTENSIONS = "tga, psd";
            PSData.FILTER_EXCLUDE_FILES = "";
            PSData.FILTER_EXCLUDE_PREFIXES = "PS_";
            PSData.FILTER_EXCLUDE_TAGS = "";
            PSData.FILTER_SCENE_SCAN_MODE = PS_Data_BottleneckSettings.SceneScanMode.AllOpenScenes;

            PSData.IGNORED_DIRECTORIES.Clear();
            _treeEditor = new PS_DirectoryTreeEditor();
            _treeEditor.Initialize(new PS_Directory(_treeEditor, Application.dataPath), PSData);

            AssetDatabase.SaveAssets();
        }

        private void DeleteAll()
        {
            PS_Utils.DeleteData_BottleneckSettings();
            PS_Utils.DeleteData_BottleneckResults();

            PS_Window_BottleneckScan[] windows = Resources.FindObjectsOfTypeAll<PS_Window_BottleneckScan>();

            if (windows.Length > 0)
            {
                var window = windows[0];

                window.ResultCards_ALL.Clear();
                window.ResultCards_AUDIO.Clear();
                window.ResultCards_CODE.Clear();
                window.ResultCards_LIGHT.Clear();
                window.ResultCards_MESH.Clear();
                window.ResultCards_PHYSICS.Clear();
                window.ResultCards_EDITOR.Clear();
                window.ResultCards_SHADER.Clear();
                window.ResultCards_TEXTURE.Clear();
                window.ResultCards_UI.Clear();
                window.ResultCards_PARTICLE.Clear();
            }

            AssetDatabase.SaveAssets();
        }
    }
}