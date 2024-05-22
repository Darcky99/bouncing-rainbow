using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// A ScriptableObject used to store Bottleneck Scanner's Settings
    /// </summary>
    public class PS_Data_BottleneckSettings : ScriptableObject
    {
        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Audio" Category
        /// </summary>
        [Header("Tests Categories")]
        public bool AUDIO_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Code" Category
        /// </summary>
        public bool CODE_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Light" Category
        /// </summary>
        public bool LIGHT_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Mesh" Category
        /// </summary>
        public bool MESH_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Physics" Category
        /// </summary>
        public bool PHYSICS_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Editor" Category
        /// </summary>
        public bool EDITOR_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Shader" Category
        /// </summary>
        public bool SHADER_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Texture" Category
        /// </summary>
        public bool TEXTURE_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "UI" Category
        /// </summary>
        public bool UI_TESTS_ENABLED = true;

        /// <summary>
        /// Whether or not should Bottleneck Scanner run tests from "Particle" Category
        /// </summary>
        public bool PARTICLE_TESTS_ENABLED = true;

        /// <summary>
        /// If true, displays results from "Audio" Category in the Results List
        /// </summary>
        [Header("Result Filters")]
        public bool SHOW_AUDIO_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Code" Category in the Results List
        /// </summary>
        public bool SHOW_CODE_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Light" Category in the Results List
        /// </summary>
        public bool SHOW_LIGHT_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Mesh" Category in the Results List
        /// </summary>
        public bool SHOW_MESH_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Physics" Category in the Results List
        /// </summary>
        public bool SHOW_PHYSICS_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Editor" Category in the Results List
        /// </summary>
        public bool SHOW_EDITOR_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Shader" Category in the Results List
        /// </summary>
        public bool SHOW_SHADER_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Texture" Category in the Results List
        /// </summary>
        public bool SHOW_TEXTURE_RESULTS = true;

        /// <summary>
        /// If true, displays results from "UI" Category in the Results List
        /// </summary>
        public bool SHOW_UI_RESULTS = true;

        /// <summary>
        /// If true, displays results from "Particle" Category in the Results List
        /// </summary>
        public bool SHOW_PARTICLE_RESULTS = true;

        /// <summary>
        /// If true, Bottleneck Scanner's Sidebar becomes visible
        /// </summary>
        [Header("General Settings")]
        public bool SHOW_SIDEBAR = true;

        /// <summary>
        /// If true, shows Result ID on the side in Results List
        /// </summary>
        public bool SHOW_RESULT_ID = false;

        /// <summary>
        /// If true, shows the Category to which a given result belongs to in Results List
        /// </summary>
        public bool SHOW_RESULT_CATEGORIES = true;

        /// <summary>
        /// Width of the "Affected Objects" List in percentage
        /// </summary>
        public float OBJECT_WIDTH_PERCENTAGE = 30;

        /// <summary>
        /// Width of the "Affected Objects" List in pixels
        /// </summary>
        public float OBJECT_HEIGHT_AMOUNT = 75;

        /// <summary>
        /// Which skin should Project Scan GUI Windows use?
        /// Auto (Recommended) - Skin changes based on user's Unity Skin
        /// Dark - forces the Dark skin
        /// Light - forces the Light skin
        /// </summary>
        public SKINTYPE SKIN_TYPE = SKINTYPE.Auto;

        public enum SKINTYPE
        { Auto, Dark, Light }

        /// <summary>
        /// What type is the Project
        /// Auto - this setting is based on Default Behavior Mode in Editor Settings
        /// 3D - the project is 3D
        /// 2D - the project is 2D
        /// Mixed - the project has a mix of both 2D and 3D elements
        /// </summary>
        public int GENERAL_PROJECT_TYPE_ID;

        public string[] ProjectType = new string[] { "Auto", "3D", "2D", "Mixed" };

        /// <summary>
        /// Files of given extensions will be ignored
        /// </summary>
        [Header("Filter Settings")]
        public string FILTER_EXCLUDE_EXTENSIONS = "tga, psd";

        /// <summary>
        /// Files of given prefixes will be ignored
        /// </summary>
        public string FILTER_EXCLUDE_PREFIXES = "PS_";

        /// <summary>
        /// GameObjects with specified tags will be ignored
        /// </summary>
        public string FILTER_EXCLUDE_TAGS = "";

        /// <summary>
        /// Files to exclude
        /// </summary>
        public string FILTER_EXCLUDE_FILES = "";

        /// <summary>
        /// Specify what scenes to scan. "All Open Scenes" will scan all currently loaded scenes. "Active Scene" will scan only active scene
        /// </summary>
        public SceneScanMode FILTER_SCENE_SCAN_MODE = SceneScanMode.AllOpenScenes;

        /// <summary>
        /// Directories which will be ignored.
        /// </summary>
        public List<string> IGNORED_DIRECTORIES;

        public enum SceneScanMode
        { AllOpenScenes, ActiveScene }

        /// <summary>
        /// If true, Benchmarks will be enforced during the scan
        /// </summary>
        [Header("Benchmark Settings")]
        public bool BENCHMARK_ENABLED = false;

        /// <summary>
        /// Polygon limit of a single MESH
        /// </summary>
        public int BENCHMARK_MESH_perMeshPolygonLimit = 3000;

        /// <summary>
        /// Maximum texture resolution
        /// </summary>
        public int BENCHMARK_TEXTURE_maxTextureResolution = 1024;

        /// <summary>
        /// Maximum number of rigidbodies a single scene can have before it's considered "too much"
        /// </summary>
        public int BENCHMARK_PHYSICS_maxRigidbodiesPerScene = 100;

        /// <summary>
        /// Maximum number of UI components a single Canvas can have before it's considered "too much"
        /// </summary>
        public int BENCHMARK_UI_maxObjectsPerCanvas = 20;

        /// <summary>
        /// Maximum age of occlusion bake data can be before it's considered "too old"
        /// </summary>
        public double BENCHMARK_EDITOR_occlussionBakeDataAge = 1.0f;

        /// <summary>
        /// Maximum distance a GameObject can be from other GameObjects before it's considered "lost in the scene"
        /// </summary>
        public float BENCHMARK_EDITOR_maxFurthestDistance = 2000f;

        public PS_Data_BottleneckSettings()
        {
            IGNORED_DIRECTORIES = new List<string>();
        }
    }
}