#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Hierarchy"-menu.</summary>
   public static class OnlineCheckGameObject
   {
      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME, false, EditorHelper.GO_ID)]
      private static void AddOnlineCheck()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME);
      }

      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME, true)]
      private static bool AddOnlineCheckValidator()
      {
         return !EditorHelper.isOnlineCheckInScene;
      }
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)