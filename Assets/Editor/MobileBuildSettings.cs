using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MobileBuildSettings
{
    [MenuItem("Build/Mobile/Android APK")]
    public static void BuildAndroidAPK()
    {
        string path = EditorUtility.SaveFolderPanel("Папка для сборки Android", "", "");
        if (string.IsNullOrEmpty(path)) return;

        ConfigureAndroid();

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetMobileScenes(),
            locationPathName = Path.Combine(path, "SneakerShop.apk"),
            target           = BuildTarget.Android,
            options          = BuildOptions.None
        });
        Debug.Log($"[Build] Android APK готов: {path}");
    }

    [MenuItem("Build/Mobile/iOS")]
    public static void BuildiOS()
    {
        string path = EditorUtility.SaveFolderPanel("Папка для Xcode проекта", "", "");
        if (string.IsNullOrEmpty(path)) return;

        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.defaultInterfaceOrientation     = UIOrientation.LandscapeLeft;

        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS,
            new[] { UnityEngine.Rendering.GraphicsDeviceType.Metal });

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.iOS,
            "MOBILE_VERSION;TOUCH_INPUT;IOS");

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetMobileScenes(),
            locationPathName = path,
            target           = BuildTarget.iOS,
            options          = BuildOptions.None
        });
        Debug.Log($"[Build] iOS Xcode проект готов: {path}");
    }

    [MenuItem("Build/Mobile/Оптимизация для мобильных")]
    public static void OptimizeForMobile()
    {
        PlayerSettings.stripEngineCode            = true;
        PlayerSettings.stripUnusedMeshComponents  = true;
        QualitySettings.SetQualityLevel(1);
        QualitySettings.pixelLightCount  = 1;
        QualitySettings.shadows          = ShadowQuality.Disable;
        QualitySettings.shadowDistance   = 20f;
        QualitySettings.antiAliasing     = 0;
        QualitySettings.globalTextureMipmapLimit = 2;
        Debug.Log("[Build] Оптимизация для мобильных применена.");
    }

    static void ConfigureAndroid()
    {
        PlayerSettings.productName = "Sneaker Shop";
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
        {
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
            UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        });

        PlayerSettings.defaultInterfaceOrientation          = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft     = true;
        PlayerSettings.allowedAutorotateToLandscapeRight    = true;
        PlayerSettings.allowedAutorotateToPortrait          = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.Android,
            "MOBILE_VERSION;TOUCH_INPUT;ANDROID");
    }

    static string[] GetMobileScenes()
    {
        var list = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list.ToArray();
    }
}
