using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public interface IFocus
    {
        void OnFocus();
    }

    public class BaseData : IFocus
    {
        public virtual void OnFocus() { }

        [Button(size: ButtonSizes.Medium), PropertyOrder(-1)]
        public void SelectScriptableObject()
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeObject = GameSettingsData.Instance;
#endif
        }
    }

    [Serializable]
    public class AnalyticsData : BaseData
    {
        public override void OnFocus()
        {
            //Debug.LogError("FOCUS AnalyticsData");
        }
    }

    [Serializable]
    public class SyncData : BaseData
    {
        public string SDKPath = "/Users/kobyle/Games/KobGamesSDK_Slim";
        [SerializeField, InlineButton(nameof(SetPathToThisProject), "SetPath")] public string DestPath = string.Empty;
        [InlineButton(nameof(addFileToArray))]
        public UnityEngine.Object FileObject;
        [SerializeField] public List<string> FilesListPath;

        public override void OnFocus()
        {
            //Debug.LogError("FOCUS SyncData");

            SetPathToThisProject();
        }

        public void SetPathToThisProject()
        {
            DestPath = Application.dataPath.Replace("Assets", "");
        }

        private void addFileToArray()
        {
#if UNITY_EDITOR
            if (FileObject != null)
            {
                FilesListPath.Add(UnityEditor.AssetDatabase.GetAssetPath(FileObject));
            }
#endif
        }
    }

    [CreateAssetMenu(fileName = "GameSettingsData")]
    public class GameSettingsData : SingletonScriptableObject<GameSettingsData>, IFocus
    {
        public SyncData SyncData;
        public AnalyticsData AnalyticsData;

        public void OnFocus()
        {
            SyncData.OnFocus();
            AnalyticsData.OnFocus();
        }

#if UNITY_EDITOR
        private static void onSelectionChanged()
        {
            if (UnityEditor.Selection.activeObject != null &&
                (UnityEditor.Selection.activeObject.name == nameof(GameSettingsData) ||
                UnityEditor.Selection.activeObject.name == nameof(GameSettings)))
            {
                if (GameSettingsData.Instance != null)
                {
                    GameSettingsData.Instance.OnFocus();
                }
            }
        }

        //[UnityEditor.Callbacks.DidReloadScripts]
        //private static void OnScriptsReloaded()
        //{
        //    //Debug.LogError("OnScriptsReloaded");
        //    if (GameSettingsData.Instance != null)
        //    {
        //        UnityEditor.Selection.selectionChanged += onSelectionChanged;
        //    }
        //}
#endif
    }
}