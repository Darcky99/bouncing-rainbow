using System;
using System.Collections;
using System.Collections.Generic;
using HardCodeLab.Utils.ProjectScan;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    public class PS_DirectoryTreeEditor
    {
        private PS_Directory _root;
        private PS_Data_BottleneckSettings _data;

        public void Initialize(PS_Directory root, PS_Data_BottleneckSettings data)
        {
            _root = root;
            _data = data;

            UpdateDirectoryStats(_data.IGNORED_DIRECTORIES);
        }

        public void Render()
        {
            EditorGUILayout.BeginVertical();
            if (_root != null) _root.Render();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Returns all directories which are marked as ignored.
        /// </summary>
        /// <returns>List of paths of ignored directories.</returns>
        public List<string> GetIgnoredDirectories()
        {
            return _root.GetIgnoredDirectories();
        }

        /// <summary>
        /// Iterates through all directories and updates whether or not they're ignored.
        /// Use this function sparingly, it's expensive O(n2)
        /// </summary>
        /// <param name="paths">List of ignored paths</param>
        public void UpdateDirectoryStats(List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                UpdateDirectoryStats(paths[i]);
            }
        }

        /// <summary>
        /// Updates a directory path of a specific directory path.
        /// </summary>
        /// <param name="path">Path of a directory which should be ignored.</param>
        public void UpdateDirectoryStats(string path)
        {
            var resultDirectory = _root.FindDirectory(path);

            if (resultDirectory != null)
            {
                if (!resultDirectory.IsIgnored)
                {
                    resultDirectory.ToggleIgnore(true, true);
                    SaveDirectoryStat(resultDirectory);
                }
            }
        }

        /// <summary>
        /// Applies directory changes to Project Scan's save data.
        /// If a given directory is ignored but doesn't exist in the list, it gets added.
        /// If a given directory is not ignored but does exists in the list, it gets removed.
        /// </summary>
        /// <param name="directory">Directory whose changes to be saved</param>
        public void SaveDirectoryStat(PS_Directory directory)
        {
            if (_data == null) return;
            if (directory.IsIgnored)
            {
                if (!_data.IGNORED_DIRECTORIES.Contains(directory.Path))
                {
                    _data.IGNORED_DIRECTORIES.Add(directory.Path);
                }
            }
            else
            {
                if (_data.IGNORED_DIRECTORIES.Contains(directory.Path))
                {
                    _data.IGNORED_DIRECTORIES.Remove(directory.Path);
                }
            }
        }
    }
}