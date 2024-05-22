#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class iOSBuildPostProcessorSecond : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 9999; } }
        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.LogError("iOSBuildPostProcessor Start (Order: 9999)");

            string path = report.summary.outputPath;
            BuildTarget target = report.summary.platform;
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(path);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));

                string targetGUID = project.GetUnityMainTargetGuid();

                // Remove SWIFT 5.0
                project.UpdateBuildProperty(targetGUID, "SWIFT_VERSION", new string[] { "5.1" }, new string[] { "5.0" });

                // Write
                File.WriteAllText(projectPath, project.WriteToString());
            }
        }
    }

    public class iOSBuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.LogError("iOSBuildPostProcessor Start (Order: 0)");

            string path = report.summary.outputPath;
            BuildTarget target = report.summary.platform;
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(path);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));

                string targetGUID = project.GetUnityMainTargetGuid();
                string targetFrameworkGUID = project.GetUnityFrameworkTargetGuid();

                // Add `-ObjC` to "Other Linker Flags".
                project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

                if (GameSettings.Instance.General.LinkerAll)
                {
                    project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-all_load");
                }

                // ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES to NO
                project.SetBuildProperty(targetFrameworkGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

                // Add frameworks
                project.AddFrameworkToProject(targetGUID, "AdSupport.framework", false);
                project.AddFrameworkToProject(targetGUID, "CoreTelephony.framework", false);
                project.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
                project.AddFrameworkToProject(targetGUID, "WebKit.framework", false);
                project.AddFrameworkToProject(targetGUID, "CoreData.framework", false);
                project.AddFrameworkToProject(targetGUID, "SystemConfiguration.framework", false);

                project.UpdateBuildProperty(targetGUID, "SWIFT_VERSION", new string[] { "5.1" }, new string[] { "5.0" });

                // Write
                File.WriteAllText(projectPath, project.WriteToString());

                // Read plist
                string plistPath = Path.Combine(path, "Info.plist");
                PlistDocument plist = new PlistDocument();
                plist.ReadFromFile(plistPath);

                // Update value
                PlistElementDict newDict = plist.root.CreateDict("NSAppTransportSecurity");
                newDict.SetBoolean("NSAllowsArbitraryLoads", true); //https://developers.ironsrc.com/ironsource-mobile/unity/unity-plugin/#step-3
                newDict.SetBoolean("ITSAppUsesNonExemptEncryption", false); //Fix for testflight "Missing Compliance", https://stackoverflow.com/questions/35841117/missing-compliance-in-status-when-i-add-built-for-internal-testing-in-test-fligh
                                                                            //newDict.SetBoolean("NSAllowsArbitraryLoadsInWebContent", true);
                                                                            //newDict.SetBoolean("NSAllowsArbitraryLoadsForMedia", true);

                if (plist.root["NSCalendarsUsageDescription"] == null)
                    plist.root["NSCalendarsUsageDescription"] = new PlistElementString("Advertisement would like to create a calendar event.");

                if (plist.root["GADApplicationIdentifier"] == null)
                    plist.root["GADApplicationIdentifier"] = new PlistElementString(GameSettings.Instance.AdsMediation.AdMobAppIdIos);

                if (plist.root["UIApplicationExitsOnSuspend"] != null)
                    plist.root.values.Remove("UIApplicationExitsOnSuspend");

                UpdateUIRequiredDeviceCapabilities(ref plist);

                // Write plist
                File.WriteAllText(plistPath, plist.WriteToString());
            }

            Debug.LogError("iOSBuildPostProcessor End");
        }

        public static void UpdateUIRequiredDeviceCapabilities(ref PlistDocument i_Plist)
        {
            string removeKey = GameSettings.Instance.General.RemoveMetal ? "metal" : "";

            if (i_Plist.root["UIRequiredDeviceCapabilities"] != null)
            {
                var array = i_Plist.root["UIRequiredDeviceCapabilities"].AsArray();
                var newArray = i_Plist.root.CreateArray("UIRequiredDeviceCapabilities");

                foreach (PlistElement arrayValue in array.values)
                {
                    if (arrayValue.AsString() == removeKey)
                    {
                        Debug.LogError($"iOSPostBuildProcess: Removing {arrayValue.AsString()}");
                        continue;
                    }

                    newArray.AddString(arrayValue.AsString());
                    Debug.LogError($"iOSPostBuildProcess: Adding {arrayValue.AsString()}");
                }

                i_Plist.root["UIRequiredDeviceCapabilities"] = newArray;
            }
        }
    }
}
#endif