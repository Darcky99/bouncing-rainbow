#if UNITY_EDITOR
using UnityEditor;
using Crosstales.OnlineCheck.EditorUtil;

namespace Crosstales.OnlineCheck.EditorIntegration
{
   /// <summary>Editor component for the "Tools"-menu.</summary>
   public static class OnlineCheckMenu
   {
      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME, false, EditorHelper.MENU_ID + 40)]
      private static void AddOnlineCheck()
      {
         EditorHelper.InstantiatePrefab(Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/" + Util.Constants.ONLINECHECK_SCENE_OBJECT_NAME, true)]
      private static bool AddOnlineCheckValidator()
      {
         return !EditorHelper.isOnlineCheckInScene;
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Create/CustomCheck", false, EditorHelper.MENU_ID + 300)]
      public static void CreateCustomCheck()
      {
         Util.Helper.CreateCustomCheck();
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/Manual", false, EditorHelper.MENU_ID + 600)]
      private static void ShowManual()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_MANUAL_URL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/API", false, EditorHelper.MENU_ID + 610)]
      private static void ShowAPI()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_API_URL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/Forum", false, EditorHelper.MENU_ID + 620)]
      private static void ShowForum()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_FORUM_URL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/Product", false, EditorHelper.MENU_ID + 630)]
      private static void ShowProduct()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_WEB_URL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/Videos/Promo", false, EditorHelper.MENU_ID + 650)]
      private static void ShowVideoPromo()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_VIDEO_PROMO);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/Videos/Tutorial", false, EditorHelper.MENU_ID + 660)]
      private static void ShowVideoTutorial()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_VIDEO_TUTORIAL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/Videos/All Videos", false, EditorHelper.MENU_ID + 680)]
      private static void ShowAllVideos()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_SOCIAL_YOUTUBE);
      }

      //      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Help/3rd Party Assets", false, EditorHelper.MENU_ID + 700)]
      //      private static void Show3rdPartyAV()
      //      {
      //          Util.Helper.OpenURL(Util.Constants.ASSET_3P_PLAYMAKER);
      //      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/About/Unity AssetStore", false, EditorHelper.MENU_ID + 800)]
      private static void ShowUAS()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_CT_URL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/About/" + Util.Constants.ASSET_AUTHOR, false, EditorHelper.MENU_ID + 820)]
      private static void ShowCT()
      {
         Crosstales.Common.Util.NetworkHelper.OpenURL(Util.Constants.ASSET_AUTHOR_URL);
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/About/Info", false, EditorHelper.MENU_ID + 840)]
      private static void ShowInfo()
      {
         EditorUtility.DisplayDialog($"{Util.Constants.ASSET_NAME} - About",
            $"Version: {Util.Constants.ASSET_VERSION}{System.Environment.NewLine}{System.Environment.NewLine}© 2017-2021 by {Util.Constants.ASSET_AUTHOR}{System.Environment.NewLine}{System.Environment.NewLine}{Util.Constants.ASSET_AUTHOR_URL}{System.Environment.NewLine}", "Ok");
      }
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)