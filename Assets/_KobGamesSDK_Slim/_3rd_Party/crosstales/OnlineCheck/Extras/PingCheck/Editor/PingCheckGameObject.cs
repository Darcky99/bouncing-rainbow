#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Hierarchy"-menu.</summary>
   public static class PingCheckGameObject
   {
      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, false, EditorHelper.GO_ID + 2)]
      private static void AddPing()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/PingCheck/Resources/Prefabs/");
      }

      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, true)]
      private static bool AddPingValidator()
      {
         return !EditorHelper.isPingInScene;
      }
   }
}
#endif
// © 2021 crosstales LLC (https://www.crosstales.com)