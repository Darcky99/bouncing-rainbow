using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor.Build.Reporting;
using System.Linq;

public class BatchBuild : MonoBehaviour
{
    public static BuildTarget BuildTarget_iOS = BuildTarget.iOS;
    public static BuildTargetGroup BuildTargetGroup_iOS = BuildTargetGroup.iOS;

    public static void Build(string appName, string packageId, string version, BuildTarget target, BuildOptions options)
    {
        //string ProjPath = Application.dataPath.Replace("/Assets", "");
        //string target_dir = ProjPath + BatchBuildConfig.TARGET_DIR;
        string target_dir = BatchBuildConfig.TARGET_DIR + "/" + BatchBuildConfig.APP_NAME;
        string appVersion = BatchBuildConfig.APP_VERSION;
        int bundleVersionCode = BatchBuildConfig.APP_BUNDLEVERSIONCODE;

        //PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);

        //string fileName = $"{appName}.apk";
        //string finalFileName = $"{appName}_{appVersion}_{bundleVersionCode}.apk";

        //string locationPath = $"{target_dir}/{fileName}";
        //string finalLocationPath = $"{target_dir}/{finalFileName}";

        string fileName = $"{appName}_{appVersion}_{bundleVersionCode}_IL2CPP.apk";
        string locationPath = $"{target_dir}/{fileName}";

        BuildTargetGroup targetGroup = BuildTargetGroup.Unknown;

        if (target == BuildTarget.Android)
        {
            locationPath = $"{target_dir}/{fileName}";

            while (File.Exists(locationPath))
            {
                bundleVersionCode++;

                fileName = $"{appName}_{appVersion}_{bundleVersionCode}_IL2CPP.apk";
                locationPath = $"{target_dir}/{fileName}";
            }

            targetGroup = BuildTargetGroup.Android;
        }
        else if (target == BatchBuild.BuildTarget_iOS)
        {
            locationPath = target_dir;
            targetGroup = BatchBuild.BuildTargetGroup_iOS;

        }
        else
        {
            Debug.LogError("No plan to support this platform yet.");
            return;
        }

        // Create if doesnt exists
        try
        {
            if (!Directory.Exists(target_dir))
            {
                Directory.CreateDirectory(target_dir);
            }

            //if (Directory.Exists(target_dir))
            //{
            //    Directory.Delete(target_dir, true);
            //}

            //Directory.CreateDirectory(target_dir);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        // switch active build target
        string strTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        if (!strTarget.Equals(target.ToString()))
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup, target);
        }

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        DateTime timeStart = DateTime.Now;

        // build pipeline
        BuildReport res = BuildPipeline.BuildPlayer(scenes, locationPath, target, options);

        if (res.summary.totalErrors > 0)
        {
            string summary = "BuildPlayer failure: " + res.summary + @"\n";

            if (res.steps.Count() > 0)
            {
                summary += "Last Step: " + res.steps[res.steps.Count() - 1].name;
            }

            throw new Exception(summary);
        }

        DateTime timeEnd = DateTime.Now;

        double totalTime = (timeEnd - timeStart).TotalMinutes;

        Debug.Log(Environment.NewLine);
        Debug.Log("#############################");
        Debug.LogFormat("Total Time: {0} mins", totalTime);

        //// We always build same file to reduce build time (so it'll use same symbol files)
        //if (File.Exists(finalLocationPath))
        //{
        //    while (File.Exists(finalLocationPath))
        //    {
        //        bundleVersionCode++;

        //        finalFileName = $"{appName}_{appVersion}_{bundleVersionCode}.apk";
        //        finalLocationPath = $"{target_dir}/{finalFileName}";
        //    }
        //}

        //Debug.Log("Moving: " + locationPath + " To: " + finalLocationPath);

        //// Rename the create apk, adding appVersion and bundleVersionCode to the filename
        //File.Move(locationPath, finalLocationPath);
    }
}
