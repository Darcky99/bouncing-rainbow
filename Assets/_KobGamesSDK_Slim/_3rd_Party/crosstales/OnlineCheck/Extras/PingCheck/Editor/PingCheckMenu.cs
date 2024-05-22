#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Tools"-menu.</summary>
   public static class PingCheckMenu
   {
      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, false, EditorHelper.MENU_ID + 60)]
      private static void AddPing()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/PingCheck/Resources/Prefabs/");
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.PINGCHECK_SCENE_OBJECT_NAME, true)]
      private static bool AddPingValidator()
      {
         return !EditorHelper.isPingInScene;
      }
   }
}
#endif
// © 2021 crosstales LLC (https://www.crosstales.com)