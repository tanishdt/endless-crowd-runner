#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EndlessCrowdRunner.Editor
{
    /// <summary>
    /// Automated build pipeline for generating Android APK and AAB binaries.
    /// Can be executed from the Unity Editor menu or headlessly via command line.
    /// </summary>
    public static class BuildPipelineHelper
    {
        private const string ScenePath = "Assets/Scenes/MainScene.unity";
        private const string OutputDir = "Builds/Android";
        private const string ApkName = "EndlessCrowdRunner.apk";
        private const string AabName = "EndlessCrowdRunner.aab";

        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroidApk()
        {
            BuildPlayer(false);
        }

        [MenuItem("Build/Build Android AAB (Google Play)")]
        public static void BuildAndroidAab()
        {
            BuildPlayer(true);
        }

        private static void BuildPlayer(bool isAab)
        {
            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), OutputDir);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string filename = isAab ? AabName : ApkName;
            string outputPath = Path.Combine(outputDirectory, filename);

            Debug.Log($"[BuildPipeline] Starting {(isAab ? "AAB" : "APK")} build -> {outputPath}");

            // Configure Player Settings
            EditorUserBuildSettings.buildAppBundle = isAab;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline] SUCCESS! Output size: {summary.totalSize / (1024 * 1024):F1} MB at {outputPath}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.RevealInFinder(outputPath);
                }
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[BuildPipeline] FAILED with {summary.totalErrors} errors!");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
