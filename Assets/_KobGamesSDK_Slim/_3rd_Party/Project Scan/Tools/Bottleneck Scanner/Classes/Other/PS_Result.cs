#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HardCodeLab.Utils.ProjectScan
{
    [Serializable]
    public struct PS_Result
    {
        public int ID;

        /// <summary>
        /// Returns false if the given test has failed
        /// </summary>
        public bool hasPassed;

        public bool showScenesDropdown;

        public string TITLE;
        public string DESCRIPTION;
        public string SOLUTION;

        public string URL;

        public List<PS_Object> affectedObjects;

        public CATEGORY ResultCategory;

        public Scene AssociatedScene;

        [SerializeField]
        public enum CATEGORY
        {
            AUDIO, CODE, EDITOR, LIGHTING, MESH, PARTICLE, PHYSICS, SHADER, TEXTURE, UI
        }

        [SerializeField]
        public enum SEVERITY
        { LOW, MEDIUM, HIGH, CIRITCAL }

        public void Populate(int id, List<PS_Object> affectedObjects, string title, string description, string solution, bool showScenesDropdown = true, string url = "")
        {
            ID = id;
            TITLE = title;
            DESCRIPTION = description;
            SOLUTION = solution;
            URL = url;

            this.showScenesDropdown = showScenesDropdown;
            this.affectedObjects = affectedObjects;
        }

        public void Populate(int id, string title, string description, string solution, bool showScenesDropdown = true, string url = "")
        {
            ID = id;
            TITLE = title;
            DESCRIPTION = description;
            SOLUTION = solution;
            URL = url;

            this.showScenesDropdown = showScenesDropdown;
            affectedObjects = new List<PS_Object>() { };
        }
    }
}

#endif