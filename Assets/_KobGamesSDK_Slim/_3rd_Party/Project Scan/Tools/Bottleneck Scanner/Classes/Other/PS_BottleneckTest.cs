#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HardCodeLab.Utils.ProjectScan
{
    [System.Serializable]
    public class PS_BottleneckTest
    {
        private bool isCanceled = false;

        //stores all project scenes
        public List<Scene> PROJECT_SCENES;

        public List<PS_Object> PROJECT_GAMEOBJECTS;                //all GameObjects across all project scenes

        [HideInInspector]
        public string[] projectNamesList;

        //variables to determine which kind of tests to run
        public bool
            INIT_AUDIO_TESTS,
            INIT_CODE_TESTS,
            INIT_LIGHT_TESTS,
            INIT_MESH_TESTS,
            INIT_PHYSICS_TESTS,
            INIT_EDITOR_TESTS,
            INIT_SHADER_TESTS,
            INIT_TEXTURE_TESTS,
            INIT_UI_TESTS,
            INIT_PARTICLE_TESTS;

        public bool IS_MOBILE;

        //List of results from specific categories
        public List<PS_Result>
            RESULTS_AUDIO,
            RESULTS_CODE,
            RESULTS_LIGHT,
            RESULTS_MESH,
            RESULTS_PHYSICS,
            RESULTS_EDITOR,
            RESULTS_SHADER,
            RESULTS_TEXTURE,
            RESULTS_UI,
            RESULTS_PARTICLE;

        //Test classes
        public PS_Test_Audio TEST_AUDIO;

        public PS_Test_Code TEST_CODE;
        public PS_Test_Editor TEST_EDITOR;
        public PS_Test_Lighting TEST_LIGHTING;
        public PS_Test_Mesh TEST_MESH;
        public PS_Test_Physics TEST_PHYSICS;
        public PS_Test_Shader TEST_SHADER;
        public PS_Test_Texture TEST_TEXTURE;
        public PS_Test_UI TEST_UI;
        public PS_Test_Particle TEST_PARTICLE;

        /// <summary>
        /// Initializes the Bottleneck Test with selective tests. Note that it must be run manually by calling Execute()
        /// </summary>
        /// <param name="TEST_PHYSICS">If true, will run Physics tests</param>
        /// <param name="TEST_TEXTURE">If true, will run Texture tests</param>
        /// <param name="TEST_MESH">If true, will run Mesh tests</param>
        /// <param name="TEST_AUDIO">If true, will run Audio tests</param>
        /// <param name="TEST_SCENE">If true, will run Scene tests</param>
        /// <param name="TEST_CODE">If true, will run Code tests</param>
        /// <param name="TEST_SHADER">If true, will run Shader tests</param>
        /// <param name="TEST_UI">If true, will run UI( tests</param>
        /// <param name="TEST_LIGHT">If true, will run Light tests</param>
        /// <param name="TEST_PARTICLE">If true, will run Particle tests</param>
        public PS_BottleneckTest(bool TEST_PHYSICS, bool TEST_TEXTURE, bool TEST_MESH, bool TEST_AUDIO, bool TEST_SCENE, bool TEST_CODE, bool TEST_SHADER, bool TEST_UI, bool TEST_LIGHT, bool TEST_PARTICLE)
        {
            PROJECT_GAMEOBJECTS = new List<PS_Object>();
            PROJECT_SCENES = new List<Scene>();

            //Initialize result lists
            RESULTS_AUDIO = new List<PS_Result>();
            RESULTS_CODE = new List<PS_Result>();
            RESULTS_LIGHT = new List<PS_Result>();
            RESULTS_MESH = new List<PS_Result>();
            RESULTS_PHYSICS = new List<PS_Result>();
            RESULTS_EDITOR = new List<PS_Result>();
            RESULTS_SHADER = new List<PS_Result>();
            RESULTS_TEXTURE = new List<PS_Result>();
            RESULTS_UI = new List<PS_Result>();
            RESULTS_PARTICLE = new List<PS_Result>();

            //Assign variables from settings
            INIT_AUDIO_TESTS = TEST_AUDIO;
            INIT_CODE_TESTS = TEST_CODE;
            INIT_LIGHT_TESTS = TEST_LIGHT;
            INIT_MESH_TESTS = TEST_MESH;
            INIT_PHYSICS_TESTS = TEST_PHYSICS;
            INIT_EDITOR_TESTS = TEST_SCENE;
            INIT_SHADER_TESTS = TEST_SHADER;
            INIT_TEXTURE_TESTS = TEST_TEXTURE;
            INIT_UI_TESTS = TEST_UI;
            INIT_PARTICLE_TESTS = TEST_PARTICLE;
        }

        /// <summary>
        /// Initializes the Bottleneck Test with all tests enabled by default. Must be run manually by calling Execute()
        /// </summary>
        public PS_BottleneckTest()
        {
            PROJECT_GAMEOBJECTS = new List<PS_Object>();
            PROJECT_SCENES = new List<Scene>();

            //Initialize result lists
            RESULTS_AUDIO = new List<PS_Result>();
            RESULTS_CODE = new List<PS_Result>();
            RESULTS_LIGHT = new List<PS_Result>();
            RESULTS_MESH = new List<PS_Result>();
            RESULTS_PHYSICS = new List<PS_Result>();
            RESULTS_EDITOR = new List<PS_Result>();
            RESULTS_SHADER = new List<PS_Result>();
            RESULTS_TEXTURE = new List<PS_Result>();
            RESULTS_UI = new List<PS_Result>();
            RESULTS_PARTICLE = new List<PS_Result>();

            //Assign variables from settings
            INIT_AUDIO_TESTS = true;
            INIT_CODE_TESTS = true;
            INIT_LIGHT_TESTS = true;
            INIT_MESH_TESTS = true;
            INIT_PHYSICS_TESTS = true;
            INIT_EDITOR_TESTS = true;
            INIT_SHADER_TESTS = true;
            INIT_TEXTURE_TESTS = true;
            INIT_UI_TESTS = true;
            INIT_PARTICLE_TESTS = true;
        }

        /// <summary>
        /// Executes specified tests
        /// </summary>
        public void Execute()
        {
            PS_Utils.GetData_FilePaths();

            if (!isCanceled)
            {
                if (PS_Utils.GetData_BottleneckSettings().FILTER_SCENE_SCAN_MODE == PS_Data_BottleneckSettings.SceneScanMode.AllOpenScenes)
                {
                    string[] fileDirectories = PS_Utils.ALL_FILE_PATHS;

                    for (int i = 0; i < fileDirectories.Length; i++)
                    {
                        string path = fileDirectories[i];

                        if (path.EndsWith(".unity"))
                        {
                            Scene scene = SceneManager.GetSceneByName(Path.GetFileNameWithoutExtension(path));

                            if (scene.isLoaded && scene.IsValid())
                            {
                                PS_Utils.CallProgressBar("Gathering Project Scenes", scene.name, i, fileDirectories.Length);

                                PROJECT_SCENES.Add(scene);
                            }
                        }
                    }
                }
                else
                {
                    Scene scene = SceneManager.GetActiveScene();

                    if (scene.isLoaded && scene.IsValid())
                    {
                        PROJECT_SCENES.Add(scene);
                    }
                }

                projectNamesList = new string[PROJECT_SCENES.Count];

                for (int i = 0; i < PROJECT_SCENES.Count; i++)
                {
                    projectNamesList[i] = PROJECT_SCENES[i].name;
                }
            }

            if (!isCanceled)
            {
                for (int i = 0; i < PROJECT_SCENES.Count; i++)
                {
                    Scene scene = PROJECT_SCENES[i];

                    string[] bannedTags = PS_Utils.GetData_BottleneckSettings().FILTER_EXCLUDE_TAGS.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    if (scene.isLoaded)
                    {
                        for (int j = 0; j < scene.GetRootGameObjects().Length; j++)
                        {
                            GameObject gObj = scene.GetRootGameObjects()[j];

                            if (!bannedTags.Contains(gObj.tag))
                            {
                                PS_Utils.CallProgressBar("Gathering GameObjects", gObj.name, j, scene.GetRootGameObjects().Length);

                                PROJECT_GAMEOBJECTS.Add(new PS_Object(gObj, PS_Object.TYPE.GAMEOBJECT));
                                PROJECT_GAMEOBJECTS[PROJECT_GAMEOBJECTS.Count - 1].sourceScene = scene;
                            }
                        }
                    }
                }
            }

            if (!isCanceled)
            {
                //determine whether the target platform is mobile or not
                IS_MOBILE = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
                             || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS
#if !UNITY_2017_3_OR_NEWER
                             || EditorUserBuildSettings.activeBuildTarget == BuildTarget.Tizen
#endif
                            );
            }

            if (!isCanceled)
            {
                //RUN AUDIO TESTS
                if (INIT_AUDIO_TESTS)
                {
                    TEST_AUDIO = new PS_Test_Audio(PROJECT_GAMEOBJECTS);
                    TEST_AUDIO.Run();
                    RESULTS_AUDIO = TEST_AUDIO.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN CODE TESTS
                if (INIT_CODE_TESTS)
                {
                    TEST_CODE = new PS_Test_Code();
                    TEST_CODE.Run();
                    RESULTS_CODE = TEST_CODE.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN LIGHTING TESTS
                if (INIT_LIGHT_TESTS)
                {
                    TEST_LIGHTING = new PS_Test_Lighting(IS_MOBILE, PROJECT_GAMEOBJECTS);
                    TEST_LIGHTING.Run();
                    RESULTS_LIGHT = TEST_LIGHTING.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN MESH TESTS
                if (INIT_MESH_TESTS)
                {
                    TEST_MESH = new PS_Test_Mesh();
                    TEST_MESH.Run();
                    RESULTS_MESH = TEST_MESH.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN PHYSICS TESTS
                if (INIT_PHYSICS_TESTS)
                {
                    TEST_PHYSICS = new PS_Test_Physics(PROJECT_GAMEOBJECTS);
                    TEST_PHYSICS.Run();
                    RESULTS_PHYSICS = TEST_PHYSICS.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN EDITOR TESTS
                if (INIT_EDITOR_TESTS)
                {
                    TEST_EDITOR = new PS_Test_Editor(PROJECT_GAMEOBJECTS);
                    TEST_EDITOR.Run();
                    RESULTS_EDITOR = TEST_EDITOR.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN SHADER TESTS
                if (INIT_SHADER_TESTS)
                {
                    TEST_SHADER = new PS_Test_Shader();
                    TEST_SHADER.Run();
                    RESULTS_SHADER = TEST_SHADER.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN TEXTURE TESTS
                if (INIT_TEXTURE_TESTS)
                {
                    TEST_TEXTURE = new PS_Test_Texture();
                    TEST_TEXTURE.Run();
                    RESULTS_TEXTURE = TEST_TEXTURE.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN UI TESTS
                if (INIT_UI_TESTS)
                {
                    TEST_UI = new PS_Test_UI(PROJECT_GAMEOBJECTS);
                    TEST_UI.Run();
                    RESULTS_UI = TEST_UI.RESULTS;
                }
            }

            if (!isCanceled)
            {
                //RUN PARTICLE TESTS
                if (INIT_PARTICLE_TESTS)
                {
                    TEST_PARTICLE = new PS_Test_Particle(PROJECT_GAMEOBJECTS);
                    TEST_PARTICLE.Run();
                    RESULTS_PARTICLE = TEST_PARTICLE.RESULTS;
                }
            }

            PS_Utils.RemoveProgressBar();

            EditorUtility.SetDirty(PS_Utils.GetData_BottleneckResults());
        }
    }
}

#endif