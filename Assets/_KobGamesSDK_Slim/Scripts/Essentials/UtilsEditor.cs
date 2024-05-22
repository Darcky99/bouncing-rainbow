#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;

namespace KobGamesSDKSlim
{
    public static class UtilsEditor
    {
        private static readonly BuildTargetGroup[] sr_BuildTargetGroups = { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone };

        public static bool RemoveDefineDirective(string i_DefineDirective)
        {
            bool isRemoved = false;
            foreach (var buildTraget in sr_BuildTargetGroups)
            {
                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTraget);

                if (Regex.IsMatch(defineSymbols, $"\\b{i_DefineDirective}\\b"))
                {
                    defineSymbols = defineSymbols.Replace($";{i_DefineDirective}", "");
                    defineSymbols = defineSymbols.Replace(i_DefineDirective, "");

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTraget, defineSymbols);

                    isRemoved = true;
                }
            }

            return isRemoved;
        }

        public static bool SetDefineDirective(string i_DefineDirective)
        {
            bool isSet = false;
            foreach (var buildTraget in sr_BuildTargetGroups)
            {
                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTraget);

                if (!Regex.IsMatch(defineSymbols, $"\\b{i_DefineDirective}\\b"))
                {
                    defineSymbols += $";{i_DefineDirective}";

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTraget, defineSymbols);

                    isSet = true;
                }
            }

            return isSet;
        }

        public static bool IsDefineDirectiveExists(string i_DefineDirective)
        {
            bool result = true;

            // Check all symbols per each build target
            foreach (var buildTraget in sr_BuildTargetGroups)
            {
                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTraget);

                // If we don't find our define directive in at least one of the build target symbols,
                // we set result to false and break out
                if (!Regex.IsMatch(defineSymbols, $"\\b{i_DefineDirective}\\b"))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
#endif