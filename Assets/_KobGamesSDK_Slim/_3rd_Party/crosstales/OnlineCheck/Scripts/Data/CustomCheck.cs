using UnityEngine;

namespace Crosstales.OnlineCheck.Data
{
   /// <summary>Data definition of a custom check.</summary>
   [System.Serializable]
   [HelpURL("https://www.crosstales.com/media/data/assets/OnlineCheck/api/class_crosstales_1_1_online_check_1_1_data_1_1_custom_check.html")]
   [CreateAssetMenu(fileName = "New CustomCheck", menuName = Util.Constants.ASSET_NAME + "/CustomCheck", order = 1000)]
   public class CustomCheck : ScriptableObject
   {
      #region Variables

      /// <summary>Custom URL to perform the Internet availability tests e.g. https://mydomain.com/connect.txt. The host should be https-based and provide an 'Access-Control-Allow-Origin' header.</summary>
      [Tooltip("Custom URL to perform the Internet availability tests e.g. https://mydomain.com/connect.txt. The host should be https-based and provide an 'Access-Control-Allow-Origin' header.")]
      public string URL = string.Empty;

      /// <summary>Expected data from the custom URL (as string).</summary>
      [Tooltip("Expected data from the custom URL (as string).")] public string ExpectedData = "success";

      /// <summary>Compares the custom data with 'equals' to the expected data (default: false, false uses 'contains' as match).</summary>
      [Tooltip("Compares the custom data with 'equals' to the expected data (default: false, false uses 'contains' as match).")]
      public bool DataMustBeEquals;

      /// <summary>Use only the custom url for Internet availability tests and ignores all built-in checks (default: false).</summary>
      [Tooltip("Use only the custom url for Internet availability tests and ignores all built-in checks (default: false).")]
      public bool UseOnlyCustom;

      /// <summary>Displays all connection errors (default: false).</summary>
      [Tooltip("Displays all connection errors (default: false).")] public bool ShowErrors;

      /// <summary>Size of the request header (default: 0).</summary>
      [Tooltip("Size of the request header (default: 0).")] public int HeaderSize;

      #endregion


      #region Overridden methods

      public override string ToString()
      {
         System.Text.StringBuilder result = new System.Text.StringBuilder();

         result.Append(GetType().Name);
         result.Append(Util.Constants.TEXT_TOSTRING_START);

         result.Append("URL='");
         result.Append(URL);
         result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

         result.Append("ExpectedData='");
         result.Append(ExpectedData);
         result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

         result.Append("DataMustBeEquals='");
         result.Append(DataMustBeEquals);
         result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

         result.Append("UseOnlyCustom='");
         result.Append(UseOnlyCustom);
         result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

         result.Append("ShowErrors='");
         result.Append(ShowErrors);
         result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER_END);

         result.Append(Util.Constants.TEXT_TOSTRING_END);

         return result.ToString();
      }

      public override bool Equals(object obj)
      {
         if (obj == null || GetType() != obj.GetType())
            return false;

         CustomCheck o = (CustomCheck)obj;

         return URL == o.URL &&
                ExpectedData == o.ExpectedData &&
                DataMustBeEquals == o.DataMustBeEquals &&
                UseOnlyCustom == o.UseOnlyCustom &&
                ShowErrors == o.ShowErrors;
      }

      public override int GetHashCode()
      {
         int hash = 0;

         if (URL != null)
            hash += URL.GetHashCode();
         if (ExpectedData != null)
            hash += ExpectedData.GetHashCode();

         return hash;
      }

      #endregion
   }
}
// © 2018-2021 crosstales LLC (https://www.crosstales.com)