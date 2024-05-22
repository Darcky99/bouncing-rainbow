﻿#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Hierarchy"-menu.</summary>
   public static class ProxyameObject
   {
      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.PROXY_SCENE_OBJECT_NAME, false, EditorHelper.GO_ID + 5)]
      private static void AddProxy()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.PROXY_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/Proxy/Resources/Prefabs/");
      }

      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.PROXY_SCENE_OBJECT_NAME, true)]
      private static bool AddProxyValidator()
      {
         return !EditorHelper.isProxyInScene;
      }
   }
}
#endif
// © 2021 crosstales LLC (https://www.crosstales.com)