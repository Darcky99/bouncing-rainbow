using UnityEngine;
using System.Linq;
using System.Reflection;

#if ENABLE_GAMEANALYTICS
using GameAnalyticsSDK;
using GameAnalyticsSDK.Editor;
#endif

namespace KobGamesSDKSlim
{
    public static class GAHelper
    {
        public static void GALoginToGameAnalytics()
        {
#if ENABLE_GAMEANALYTICS
            GameAnalytics.SettingsGA.EmailGA = "kobgamesstudio@gmail.com";
            GameAnalytics.SettingsGA.PasswordGA = "KobGameAnal1!";//"KobGame1!";

            typeof(GameAnalyticsSDK.Editor.GA_SettingsInspector)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                              .Where(methodInfo => methodInfo.Name.Contains("LoginUser"))
                              .First()
                              .Invoke(GameAnalytics.SettingsGA, new object[] { GameAnalytics.SettingsGA });
#endif
        }

        public static void GAClearPlatformsSettings()
        {
#if ENABLE_GAMEANALYTICS
            foreach (var index in GameAnalytics.SettingsGA.GetAvailablePlatforms())
            {
                GameAnalytics.SettingsGA.RemovePlatformAtIndex(0);
            }
#endif
        }

        public static string[] GAGetStudiosNames()
        {
            string[] result = { "-" };

#if ENABLE_GAMEANALYTICS
            var organizationIndex = GAHelper.GAFindOrganizationIndex("KobGames");
            if (organizationIndex >= 0)
            {
                result = GameAnalyticsSDK.Setup.Studio.GetStudioNames(GameAnalytics.SettingsGA.Organizations[organizationIndex].Studios);
            }
#endif

            return result;
        }


        public static int GAFindOrganizationIndex(string i_OrganizationName)
        {
            var organizationIndex = -1;

#if ENABLE_GAMEANALYTICS
            if (GameAnalytics.SettingsGA.Organizations != null && GameAnalytics.SettingsGA.Organizations.Count > 0)
            {
                organizationIndex = GameAnalytics.SettingsGA.Organizations.FindIndex(x => x.Name.Contains(i_OrganizationName));
                if (organizationIndex < 0)
                {
                    Debug.LogError($"Couldn't find Organization: {i_OrganizationName} FoundIndex: {organizationIndex}");
                }
                //else
                //{
                //    Debug.LogError($"Found Organization: {GameAnalytics.SettingsGA.Organizations[organizationIndex].Name} Index: {organizationIndex}");
                //}
            }
#endif

            return organizationIndex;
        }

        public static int GAFindStudioIndex(string i_StudioName)
        {
            var studioIndex = -1;
            var organizationIndex = -1;

#if ENABLE_GAMEANALYTICS
            organizationIndex = GAFindOrganizationIndex(i_StudioName);
            if (organizationIndex >= 0)
            {
                //foreach (var x in GameAnalytics.SettingsGA.Organizations[organizationIndex].Studios)
                //{
                //    Debug.LogError(x.Name);
                //}
                studioIndex = GameAnalytics.SettingsGA.Organizations[organizationIndex].Studios.FindIndex(x => x.Name.Contains(i_StudioName));
                if (studioIndex < 0)
                {
                    Debug.LogError($"Couldn't find Studio: {i_StudioName} FoundIndex: {studioIndex}");
                }
                //else
                //{
                //    Debug.LogError($"Found Studio: {GameAnalytics.SettingsGA.Organizations[organizationIndex].Studios[studioIndex].Name} StudioIndex: {studioIndex} OrgIndex: {organizationIndex}");
                //}
            }
#endif

            return studioIndex;
        }

        public static int GAFindGameIndex(int i_OrganizationIndex, int i_StudioIndex, string i_GameName)
        {
            var gameIndex = -1;

#if ENABLE_GAMEANALYTICS
            //foreach (var x in GameAnalytics.SettingsGA.Organizations[i_OrganizationIndex].Studios[i_StudioIndex].Games)
            //{
            //    Debug.LogError(x.Name);
            //}
            gameIndex = 1 + GameAnalytics.SettingsGA.Organizations[i_OrganizationIndex].Studios[i_StudioIndex].Games.FindIndex(x => x.Name.Contains(i_GameName));
            if (gameIndex <= 0)
            {
                Debug.LogError($"Couldn't find Game: {i_GameName} FoundIndex: {gameIndex}");
            }
#endif
            return gameIndex;
        }

        public static int GAGetPlatformIndex(RuntimePlatform i_Platform, bool i_CreateIfMissing = true)
        {
            var platfromIndex = -1;

#if ENABLE_GAMEANALYTICS
            platfromIndex = GameAnalytics.SettingsGA.Platforms.FindIndex(x => x == i_Platform);

            if (i_CreateIfMissing)
            {
                if (platfromIndex < 0)
                {
                    GameAnalytics.SettingsGA.AddPlatform(i_Platform);
                    platfromIndex = GameAnalytics.SettingsGA.Platforms.Count - 1;
                }
            }
#endif
            return platfromIndex;
        }

        public static int GAGetStudioGames(string i_StudioName)
        {
            var count = 0;
#if ENABLE_GAMEANALYTICS
            var organizationIndex = GAFindOrganizationIndex(i_StudioName);
            if (organizationIndex >= 0)
            {
                var studioIndex = GAFindStudioIndex(i_StudioName);
                if (studioIndex < 0)
                    return 0;

                var platfromIndexIOS = GAGetPlatformIndex(RuntimePlatform.IPhonePlayer, false);
                var platfromIndexAndroid = GAGetPlatformIndex(RuntimePlatform.Android, false);

                if (platfromIndexIOS >= 0)
                    count += GameAnalytics.SettingsGA.SelectedGame[platfromIndexIOS];

                if (platfromIndexAndroid >= 0)
                    count += GameAnalytics.SettingsGA.SelectedGame[platfromIndexAndroid];

                //Debug.LogError("IOS: " + GameAnalyticsSDK.GameAnalytics.SettingsGA.Studios[platfromIndexIOS].Games.Count);
                //Debug.LogError("Android: " + GameAnalyticsSDK.GameAnalytics.SettingsGA.Studios[platfromIndexAndroid].Games.Count);
            }
#endif
            return count;
        }


        public static bool GASelectGame(string i_StudioName, string i_GameName, RuntimePlatform i_Platform)
        {
#if ENABLE_GAMEANALYTICS
            var studioName = i_StudioName;
            var gameName = i_GameName;
            var organizationIndex = -1;
            var studioIndex = -1;
            var gameIndex = -1;
            var platfromIndex = -1;

            organizationIndex = GAFindOrganizationIndex(i_StudioName);
            //Debug.LogError($"organizationIndex: {organizationIndex}");
            if (organizationIndex < 0)
                return false;

            studioIndex = GAFindStudioIndex(i_StudioName);
            //Debug.LogError($"studioIndex: {studioIndex}");
            if (studioIndex < 0)
                return false;

            gameIndex = GAFindGameIndex(organizationIndex, studioIndex, i_GameName);
            if (gameIndex <= 0)
                return false;

            platfromIndex = GAGetPlatformIndex(i_Platform);

            GameAnalytics.SettingsGA.SelectedOrganization[platfromIndex] = 1 + organizationIndex;
            GameAnalytics.SettingsGA.SelectedStudio[platfromIndex] = 1 + studioIndex;
            GameAnalytics.SettingsGA.SelectedGame[platfromIndex] = gameIndex;

            typeof(GA_SettingsInspector)
                        .GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                        .Where(methodInfo => methodInfo.Name.Contains("SelectGame"))
                        .First()
                        .Invoke(GameAnalytics.SettingsGA, new object[] { gameIndex, GameAnalytics.SettingsGA, platfromIndex });

#endif
            return true;
        }

        public static bool GACreateGame(string i_StudioName, string i_GameName, RuntimePlatform i_Platform)
        {
#if ENABLE_GAMEANALYTICS
            //m_GACreationSuccessCallback = i_CreationSuccessCallback;

            //GA_SignUp dummyGA_SignUp = new GA_SignUp();
            GA_SignUp dummyGA_SignUp = ScriptableObject.CreateInstance(typeof(GA_SignUp)) as GA_SignUp;

            int selectedOrganizationIndex = GAFindOrganizationIndex(i_StudioName);
            int selectedStudioIndex = GAFindStudioIndex(i_StudioName);

            if (selectedStudioIndex >= 0)
            {
                //m_GAPendingCreationOfNewGame = true;

                GA_SettingsInspector.CreateGame
                    (GameAnalytics.SettingsGA,
                    dummyGA_SignUp,
                    selectedOrganizationIndex,
                    selectedStudioIndex,
                    i_GameName,
                    string.Empty,
                    i_Platform
                    ,
                    null);

                return true;
            }
            else
            {
                Debug.LogError($"gaCreatePlatforms(): Error couldn't find selectedStudioIndex, found index: {selectedStudioIndex} GAStudioName: {i_StudioName} StudiosCount: {GameAnalytics.SettingsGA.Organizations[selectedOrganizationIndex].Studios.Count}");
            }
#endif

            return false;
        }
    }
}