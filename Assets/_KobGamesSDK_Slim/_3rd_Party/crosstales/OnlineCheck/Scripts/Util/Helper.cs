using UnityEngine;

namespace Crosstales.OnlineCheck.Util
{
   /// <summary>Various helper functions.</summary>
   public abstract class Helper : Common.Util.BaseHelper
   {
      /// <summary>Creates a custom check asset.</summary>
      public static void CreateCustomCheck()
      {
#if UNITY_EDITOR
         string guid;
         int index = 0;

         do
         {
            guid = UnityEditor.AssetDatabase.AssetPathToGUID($"Assets/New CustomCheck{(index == 0 ? "" : " " + index)}.asset");
            index++;
         } while (!string.IsNullOrEmpty(guid));

         index--;

         Data.CustomCheck asset = ScriptableObject.CreateInstance<Data.CustomCheck>();

         UnityEditor.AssetDatabase.CreateAsset(asset, $"Assets/New CustomCheck{(index == 0 ? "" : " " + index)}.asset");
         UnityEditor.AssetDatabase.SaveAssets();

         UnityEditor.EditorUtility.FocusProjectWindow();

         UnityEditor.Selection.activeObject = asset;
#endif
      }
   }
}
// © 2017-2021 crosstales LLC (https://www.crosstales.com)