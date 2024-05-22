using UnityEngine;

namespace Crosstales.OnlineCheck.Util
{
   /// <summary>Setup the project to use OnlineCheck.</summary>
#if UNITY_EDITOR
   [UnityEditor.InitializeOnLoadAttribute]
#endif
   public class SetupProject
   {
      #region Constructor

      static SetupProject()
      {
         setup();
      }

      #endregion


      #region Public methods

      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
      private static void setup()
      {
         Crosstales.Common.Util.Singleton<OnlineCheck>.PrefabPath = "Prefabs/OnlineCheck";
         Crosstales.Common.Util.Singleton<OnlineCheck>.GameObjectName = "OnlineCheck";
         Crosstales.Common.Util.Singleton<Tool.PingCheck>.PrefabPath = "Prefabs/PingCheck";
         Crosstales.Common.Util.Singleton<Tool.PingCheck>.GameObjectName = "PingCheck";
         Crosstales.Common.Util.Singleton<Tool.SpeedTest>.PrefabPath = "Prefabs/SpeedTest";
         Crosstales.Common.Util.Singleton<Tool.SpeedTest>.GameObjectName = "SpeedTest";
      }

      #endregion
   }
}
// © 2020-2021 crosstales LLC (https://www.crosstales.com)