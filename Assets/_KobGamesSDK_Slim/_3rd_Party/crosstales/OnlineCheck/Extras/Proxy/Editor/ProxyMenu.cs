#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Tools"-menu.</summary>
   public static class ProxyMenu
   {
      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.PROXY_SCENE_OBJECT_NAME, false, EditorHelper.MENU_ID + 110)]
      private static void AddProxy()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.PROXY_SCENE_OBJECT_NAME, $"{EditorConfig.ASSET_PATH}Extras/Proxy/Resources/Prefabs/");
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.PROXY_SCENE_OBJECT_NAME, true)]
      private static bool AddProxyValidator()
      {
         return !EditorHelper.isProxyInScene;
      }
   }
}
#endif
// © 2021 crosstales LLC (https://www.crosstales.com)