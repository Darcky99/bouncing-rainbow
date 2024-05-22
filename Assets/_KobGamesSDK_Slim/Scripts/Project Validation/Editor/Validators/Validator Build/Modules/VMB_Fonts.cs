using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KobGamesSDKSlim.ProjectValidator.ValidatorUtils;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Build
{
    public class VMB_Fonts : ValidatorModuleBuild
    {
        private const string c_FontsWarnings = "Using Unity Text instead of TMP. Consider changing for performance reasons and use KobGames pipeline.";

        private const string c_FontsErrors =
            "Either the Text Component doesn't have a Font reference or the TMP Font Asset name doesn't have a correspondence with a font in the project (ex: 'Font Name SDF').";

        private string c_MissingLicenseError(Font i_Font) => "Couldn't find License for Font - " + i_Font.name +
                                                             "\n it should be called - 'Font Name_license' - and be inside Resources Folder";

        private string c_LicenseOutsideOfResourcesError(TextAsset i_License) => "Font Licenses should be in Resources folder - " + i_License.name;


        // private static readonly string[] c_WarningsIgnoreFolders = new string[] {"_3rd_Party", "MaxSdk"};


        public override eBuildValidatorResult Validate()
        {
            //Get Assets
            var fonts      = GetAssets<Font>();
            var textAssets = GetAssets<TextAsset>();

            //Find Text Components
            var evaluatedObject = ValidatorBuild.EvaluatedObjectsList;
            var textList        = evaluatedObject.Select(x => x.GetComponent<Text>()).Where(x => x            != null).ToList();
            var textMeshList    = evaluatedObject.Select(x => x.GetComponent<TextMesh>()).Where(x => x        != null).ToList();
            var TMPUIList       = evaluatedObject.Select(x => x.GetComponent<TextMeshProUGUI>()).Where(x => x != null).ToList();
            var TMPList         = evaluatedObject.Select(x => x.GetComponent<TextMeshPro>()).Where(x => x     != null).ToList();

        #region Warnings

            // Set Warnings List
            var warningList = convertToListOfObjects(textList)
                             .Concat(convertToListOfObjects(textMeshList)).ToList();
            //Log
            warningList.ForEach(gameObject => ValidatorBuild.WarningsList.Add(showLogWithHierarchyPath(c_FontsWarnings, gameObject.gameObject, false)));

        #endregion

        #region Errors

            //List of Components with Errors
            var textMissingFonts     = textList.FindAll(text => text.font     == null || text.font.name == "Arial");
            var textMeshMissingFonts = textMeshList.FindAll(text => text.font == null || text.font.name == "Arial");
            //We weren't able to look for a Font based on TMP SDF Asset so we go by the name instead
            var TMPUIMissingFonts = TMPUIList.FindAll(text => text.font == null || !fonts.Find(font => font.name == SDFNameToFontName(text.font.name)));
            var TMPMissingFonts   = TMPList.FindAll(text => text.font   == null || !fonts.Find(font => font.name == SDFNameToFontName(text.font.name)));

            //Set Errors List
            var errorList = convertToListOfObjects(textMissingFonts)
                           .Concat(convertToListOfObjects(textMeshMissingFonts))
                           .Concat(convertToListOfObjects(TMPUIMissingFonts))
                           .Concat(convertToListOfObjects(TMPMissingFonts)).ToList();
            //Log
            errorList.ForEach(x => showLogWithHierarchyPath(c_FontsErrors, x.gameObject, true));

        #endregion

        #region License

            //Selecting only fonts that are being used in the project. We don't need license if the font is not being used
            var usedFonts = new List<Font>();
            usedFonts.AddRange(textList.Where(text => fonts.Contains(text.font)).Select(text => text.font));
            usedFonts.AddRange(textMeshList.Where(text => fonts.Contains(text.font)).Select(text => text.font));
            usedFonts.AddRange(TMPUIList.Where(text => fonts.Find(font => font.name == SDFNameToFontName(text.font.name)) != null).Select(text => fonts.Find(font => font.name == SDFNameToFontName(text.font.name))));
            usedFonts.AddRange(TMPList.Where(text => fonts.Find(font => font.name   == SDFNameToFontName(text.font.name)) != null).Select(text => fonts.Find(font => font.name == SDFNameToFontName(text.font.name))));
            usedFonts = usedFonts.Distinct().ToList();

            //Find Fonts with License
            var fontsWithLicense    = usedFonts.FindAll(font => textAssets.Find(textAsset => textAsset.name == fontNameToLicenseName(font))).ToList();
            var fontsWithoutLicense = usedFonts.Except(fontsWithLicense).ToList();

            //Find Licenses
            var licenses                 = fontsWithLicense.Select(font => textAssets.Find(textAsset => textAsset.name == fontNameToLicenseName(font))).ToList();
            var licensesOutsideResources = licenses.Where(license => !isObjectInFolder(license, "Resources")).ToList();

            //Show Errors
            fontsWithoutLicense.ForEach(font => Debug.LogError(c_MissingLicenseError(font),                       font));
            licensesOutsideResources.ForEach(license => Debug.LogError(c_LicenseOutsideOfResourcesError(license), license));

        #endregion

            if (errorList.Count                != 0
             || fontsWithoutLicense.Count      != 0
             || licensesOutsideResources.Count != 0)
                return eBuildValidatorResult.ErrorsNeedFixing;

            if (warningList.Count > 0)
                return eBuildValidatorResult.WarningsOnly;

            return eBuildValidatorResult.AllGood;
        }


        /// <summary>
        /// Converts IEnumerable of Components to IEnumerable of GameObjects 
        /// </summary>
        /// <param name="i_List"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<GameObject> convertToListOfObjects<T>(IEnumerable<T> i_List) where T : Component => i_List.Select(x => x.gameObject).ToList();

        /// <summary>
        /// Converts TMP SDF Asset Name to Font name.
        /// </summary>
        /// <param name="i_SDFName"></param>
        /// <returns></returns>
        private string SDFNameToFontName(string i_SDFName) => i_SDFName.Substring(0, i_SDFName.Length - 4);

        /// <summary>
        /// Converts Font Name to de desired License Name
        /// </summary>
        /// <param name="i_Font"></param>
        /// <returns></returns>
        string fontNameToLicenseName(Font i_Font) => i_Font.name + "_license";
    }
}