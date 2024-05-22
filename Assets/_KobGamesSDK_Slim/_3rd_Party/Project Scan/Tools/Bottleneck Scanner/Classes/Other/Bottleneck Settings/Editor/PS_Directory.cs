using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Directory class. Stores a path, its name, its parent and other subdirectories.
    /// </summary>
    [Serializable]
    public class PS_Directory
    {
        [SerializeField] private PS_DirectoryTreeEditor _tree;

        /// <summary>
        /// ID of this directory. Used to determine whether or not this directory is included for scanning process.
        /// </summary>
        public int ID;

        /// <summary>
        /// Directory name
        /// </summary>
        public string Name;

        /// <summary>
        /// Directory Path
        /// </summary>
        public string Path;

        /// <summary>
        /// Returns true if Directory is selected (meaning it's included for scanning)
        /// </summary>
        /// <summary>
        /// Parent of this Directory
        /// </summary>
        public PS_Directory Parent;

        /// <summary>
        /// Subdirectories of this Directory
        /// </summary>
        public PS_Directory[] SubDirectories;

        /// <summary>
        /// Returns true if this directory is being ignored.
        /// </summary>
        public bool IsIgnored;

        /// <summary>
        /// Changes the status of this Directory
        /// </summary>
        /// <param name="state">The new state. True means that it will be ignored.</param>
        /// <param name="affectChildren">If it's true, then subdirectories of this Directory will also have set a new state</param>
        public void ToggleIgnore(bool state, bool affectChildren = false)
        {
            IsIgnored = state;

            if (affectChildren && SubDirectories != null)
            {
                for (int i = 0; i < SubDirectories.Length; i++)
                {
                    SubDirectories[i].ToggleIgnore(state, true);
                }
            }

            if (!IsIgnored)
            {
                if (Parent != null)
                {
                    if (Parent.IsIgnored)
                    {
                        Parent.ToggleIgnore(false);
                    }
                }
            }

            if (_tree != null) _tree.SaveDirectoryStat(this);
        }

        /// <summary>
        /// Initializes a <seealso cref="PS_Directory"/> class
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="path"></param>
        /// <param name="parent">Parent of this directory</param>
        public PS_Directory(PS_DirectoryTreeEditor tree, string path, PS_Directory parent = null)
        {
            _tree = tree;
            Path = path;
            Parent = parent;
            Name = System.IO.Path.GetFileName(Path);

            FetchSubdirectories();
        }

        /// <summary>
        /// Renders the GUI.
        /// </summary>
        public void Render()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            var labelContent = new GUIContent(Name, EditorGUIUtility.IconContent("Folder Icon", null).image);

            bool isIgnoredTemp = IsIgnored;
            isIgnoredTemp = EditorGUILayout.ToggleLeft(labelContent, isIgnoredTemp);

            if (EditorGUI.EndChangeCheck())
                ToggleIgnore(isIgnoredTemp, true);

            EditorGUILayout.EndHorizontal();

            if (SubDirectories != null)
            {
                if (SubDirectories.Length > 0)
                {
                    EditorGUI.indentLevel++;

                    for (int i = 0; i < SubDirectories.Length; i++)
                    {
                        SubDirectories[i].Render();
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Fetches subdirectories of this directory.
        /// </summary>
        private void FetchSubdirectories()
        {
            string[] children = Directory.GetDirectories(Path, "*", SearchOption.TopDirectoryOnly);

            if (children.Length > 0)
            {
                SubDirectories = new PS_Directory[children.Length];

                for (int i = 0; i < children.Length; i++)
                {
                    var newItem = new PS_Directory(_tree, children[i], this);
                    SubDirectories[i] = newItem;
                }
            }
            else
            {
                SubDirectories = new PS_Directory[] { };
            }
        }

        /// <summary>
        /// Returns ignored directories.
        /// </summary>
        /// <returns>Returns an array of ignored directories. Returns null if there are no ignored subdirectories.</returns>
        public List<string> GetIgnoredDirectories()
        {
            var subDirs = new List<string>();

            if (IsIgnored)
            {
                subDirs.Add(Path);
            }

            for (int i = 0; i < SubDirectories.Length; i++)
            {
                subDirs.AddRange(SubDirectories[i].GetIgnoredDirectories());
            }

            return subDirs;
        }

        /// <summary>
        /// Returns a directory that contains exact path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public PS_Directory FindDirectory(string path)
        {
            if (Path.Equals(path))
                return this;

            for (int i = 0; i < SubDirectories.Length; i++)
            {
                var psDirectory = SubDirectories[i].FindDirectory(path);

                if (psDirectory != null)
                {
                    return psDirectory;
                }
            }

            return null;
        }
    }
}