#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Hierarchy"-menu.</summary>
   public static class SpeedTestGameObject
   {
      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, false, EditorHelper.GO_ID + 3)]
      private static void AddSpeedTest()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/SpeedTest/Resources/Prefabs/");
      }

      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, true)]
      private static bool AddSpeedTestValidator()
      {
         return !EditorHelper.isSpeedTestInScene;
      }
   }
}
#endif
// © 2021 crosstales LLC (https://www.crosstales.com)