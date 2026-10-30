using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAndroidApk
{
    [InitializeOnLoadMethod]
    private static void CheckAutoBuildTrigger()
    {
        EditorApplication.delayCall += () =>
        {
            string triggerFile = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "trigger_build_apk.flag");
            if (File.Exists(triggerFile))
            {
                try { File.Delete(triggerFile); } catch { }
                Debug.Log("[BuildAndroidApk] Trigger file detected! Executing BuildApk()...");
                BuildApk();
            }
        };
    }

    [MenuItem("SlimeFoodDrop/Build Android APK")]
    public static void BuildApk()
    {
        // Rebuild MainGameScene first so the latest Splash Screen, Home Screen, Difficulty, and Themes are baked in
        BuildMainGameScene.BuildAll();

        Debug.Log("[BuildAndroidApk] Configuring Android Portrait PlayerSettings...");

        // Ensure Android SDK/NDK/JDK paths are explicitly bound in batchmode
        string playbackEngine = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer");
        string sdkPath = Path.Combine(playbackEngine, "SDK");
        string ndkPath = Path.Combine(playbackEngine, "NDK");
        string jdkPath = Path.Combine(playbackEngine, "OpenJDK");
        if (Directory.Exists(sdkPath)) EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        if (Directory.Exists(ndkPath)) EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
        if (Directory.Exists(jdkPath)) EditorPrefs.SetString("JdkPath", jdkPath);

        PlayerSettings.companyName = "SlimeStudio";
        PlayerSettings.productName = "Slime Food Drop";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.slimestudio.slimefooddrop");

        // Enforce Portrait Mobile Orientation & Full Ultra-Tall Aspect Ratio Support (20:9, 21:9, 22:9)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.defaultScreenWidth = 1080;
        PlayerSettings.defaultScreenHeight = 2400;
        PlayerSettings.Android.maxAspectRatio = 2.6f;
        PlayerSettings.Android.renderOutsideSafeArea = true;
        PlayerSettings.Android.startInFullscreen = true;

        // Set app icon to our Slime Idle sprite
        Texture2D iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Slime/01-Idle/FR_Slime4_Idle_000.png");
        if (iconTex != null)
        {
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { iconTex }, IconKind.Any);
        }

        // Android target settings
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;

        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
        Directory.CreateDirectory(buildDir);
        string apkPath = Path.Combine(buildDir, "SlimeFoodDrop.apk");

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = new string[] { "Assets/Scenes/MainGameScene.unity" },
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("[BuildAndroidApk] Starting Android APK build to: " + apkPath);
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildAndroidApk] APK BUILD SUCCEEDED! Size: {summary.totalSize} bytes at {apkPath}");
        }
        else
        {
            Debug.LogError($"[BuildAndroidApk] APK BUILD FAILED: {summary.result} (Errors: {summary.totalErrors})");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
