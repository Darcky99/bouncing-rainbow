using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace KobGamesSDKSlim
{
	[ExecutionOrder(eExecutionOrderBase.CultureInfoManager)]
	public class CultureInfoManager : MonoBehaviour
	{
		private void Awake()
		{
			if (!GameSettings.Instance.General.SetSpecificCulture)
			{
				Debug.LogWarning("Specific Culture is not Set. This might create Big Lag Spike or Missing Glyphs if they don't exist in the font. Consider setting it to true");
				return;
			}

			string cultureName = GameSettings.Instance.General.CultureName;
			
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(cultureName) || !CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().Select(x => x.Name).Contains(cultureName))
			{
				Debug.LogError("System doesn't contain this culture - " + cultureName + " OR it is null. Please select appropriate Culture at GameSettings -> General -> Culture");
				return;
			}
#endif
			CultureInfo culture = CultureInfo.CreateSpecificCulture(cultureName);

			Thread.CurrentThread.CurrentCulture   = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}
	}
}

