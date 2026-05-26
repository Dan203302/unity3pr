using UnityEditor;
using UnityEngine;
using System.IO;

public class PCBuildSettings
{
    [MenuItem("Build/PC/Windows 64-bit")]
    public static void BuildWindows64()
    {
        string path = EditorUtility.SaveFolderPanel("Папка для сборки Windows", "", "");
        if (string.IsNullOrEmpty(path)) return;

        ConfigurePC();

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetScenePaths(),
            locationPathName = Path.Combine(path, "SneakerShop.exe"),
            target           = BuildTarget.StandaloneWindows64,
            options          = BuildOptions.ShowBuiltPlayer
        });
        Debug.Log($"[Build] Windows готов: {path}");
    }

    [MenuItem("Build/PC/MacOS")]
    public static void BuildMacOS()
    {
        string path = EditorUtility.SaveFolderPanel("Папка для сборки Mac", "", "");
        if (string.IsNullOrEmpty(path)) return;

        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetScenePaths(),
            locationPathName = Path.Combine(path, "SneakerShop.app"),
            target           = BuildTarget.StandaloneOSX,
            options          = BuildOptions.ShowBuiltPlayer
        });
        Debug.Log($"[Build] MacOS готов: {path}");
    }

    [MenuItem("Build/PC/Linux")]
    public static void BuildLinux()
    {
        string path = EditorUtility.SaveFolderPanel("Папка для сборки Linux", "", "");
        if (string.IsNullOrEmpty(path)) return;

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetScenePaths(),
            locationPathName = Path.Combine(path, "SneakerShop.x86_64"),
            target           = BuildTarget.StandaloneLinux64,
            options          = BuildOptions.ShowBuiltPlayer
        });
        Debug.Log($"[Build] Linux готов: {path}");
    }

    [MenuItem("Build/PC/Оптимизация для ПК")]
    public static void OptimizeForPC()
    {
        ConfigurePC();
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.MTRendering     = true;
        PlayerSettings.graphicsJobs    = true;
        PlayerSettings.stripUnusedMeshComponents = true;
        QualitySettings.SetQualityLevel(2);
        QualitySettings.vSyncCount  = 1;
        QualitySettings.antiAliasing = 4;
        Debug.Log("[Build] Оптимизация для ПК применена.");
    }

    static void ConfigurePC()
    {
        PlayerSettings.productName          = "Sneaker Shop";
        PlayerSettings.companyName          = "GameDev";
        PlayerSettings.defaultScreenWidth   = 1920;
        PlayerSettings.defaultScreenHeight  = 1080;
        PlayerSettings.fullScreenMode       = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow      = true;
        PlayerSettings.allowFullscreenSwitch = true;

        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[]
        {
            UnityEngine.Rendering.GraphicsDeviceType.Direct3D11,
            UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        });

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.Standalone,
            "PC_VERSION;ENABLE_PROFILER");
    }

    static string[] GetScenePaths()
    {
        var list = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list.ToArray();
    }
}
