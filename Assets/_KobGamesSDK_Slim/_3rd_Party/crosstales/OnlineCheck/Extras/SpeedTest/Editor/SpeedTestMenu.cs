#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Tools"-menu.</summary>
   public static class SpeedTestMenu
   {
      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, false, EditorHelper.MENU_ID + 80)]
      private static void AddSpeedTest()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/SpeedTest/Resources/Prefabs/");
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.SPEEDTEST_SCENE_OBJECT_NAME, true)]
      private static bool AddSpeedTestValidator()
      {
         return !EditorHelper.isSpeedTestInScene;
      }
   }
}
#endif
// © 2021 crosstales LLC (https://www.crosstales.com)