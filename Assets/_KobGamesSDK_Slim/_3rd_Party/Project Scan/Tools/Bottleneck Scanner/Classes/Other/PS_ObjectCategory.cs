#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// Used for splitting GameObjects by their source Scene
    /// </summary>
    public class PS_ObjectCategory
    {
        /// <summary>
        /// Scene that GameObjects belong to
        /// </summary>
        public Scene categoryScene;

        /// <summary>
        /// List of GameObjects that belong to a specified scene
        /// </summary>
        public List<PS_Object> objectsList;

        public PS_ObjectCategory(Scene scene)
        {
            objectsList = new List<PS_Object>();
            categoryScene = scene;
        }
    }
}

#endif