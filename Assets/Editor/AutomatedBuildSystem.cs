using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class AutomatedBuildSystem
{
    [MenuItem("Build/Auto Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        string root = "Builds/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        Directory.CreateDirectory(root);

        BuildWindows(root);
        BuildAndroid(root);
        BuildWebGL(root);

        Process.Start(Path.GetFullPath(root));
        UnityEngine.Debug.Log($"[AutoBuild] Все сборки готовы: {root}");
    }

    [MenuItem("Build/Run Performance Tests")]
    public static void RunPerformanceTests()
    {
        TestPCPerformance();
        TestMobilePerformance();
        TestWebGLPerformance();
    }

    static void BuildWindows(string root)
    {
        string path = Path.Combine(root, "Windows");
        Directory.CreateDirectory(path);

        PlayerSettings.productName = "Sneaker Shop";
        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.Standalone, "PC_VERSION");

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetScenes(),
            locationPathName = Path.Combine(path, "SneakerShop.exe"),
            target           = BuildTarget.StandaloneWindows64,
            options          = BuildOptions.CompressWithLz4HC
        });

        File.WriteAllText(Path.Combine(path, "README.txt"),
            "Sneaker Shop — Windows Build\n\n" +
            "Системные требования:\n" +
            "  - Windows 10/11 64-bit\n  - 4 GB RAM\n  - DirectX 11 GPU\n\n" +
            "Управление:\n" +
            "  WASD — движение, Мышь — взгляд, E — взаимодействие\n" +
            "  Tab — энергия, 1/2/3/4 — способности, F — продажа\n" +
            "  F1-F9 — debug команды");
        UnityEngine.Debug.Log($"[AutoBuild] Windows: {path}");
    }

    static void BuildAndroid(string root)
    {
        string path = Path.Combine(root, "Android");
        Directory.CreateDirectory(path);

        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.Android, "MOBILE_VERSION;ANDROID");

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetScenes(),
            locationPathName = Path.Combine(path, "SneakerShop.apk"),
            target           = BuildTarget.Android,
            options          = BuildOptions.CompressWithLz4HC
        });

        File.WriteAllText(Path.Combine(path, "INSTALL.txt"),
            "Android Installation:\n" +
            "1. Включить 'Установка из неизвестных источников'\n" +
            "2. Скопировать APK на устройство\n" +
            "3. Открыть APK и установить\n\n" +
            "Минимальные требования: Android 5.0+, 2GB RAM");
        UnityEngine.Debug.Log($"[AutoBuild] Android: {path}");
    }

    static void BuildWebGL(string root)
    {
        string path = Path.Combine(root, "WebGL");
        Directory.CreateDirectory(path);

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.memorySize        = 256;
        PlayerSettings.WebGL.dataCaching       = true;

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.WebGL, "WEBGL_VERSION");

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetScenes(),
            locationPathName = path,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.CompressWithLz4HC
        });

        CheckWebGLSize(path);
        UnityEngine.Debug.Log($"[AutoBuild] WebGL: {path}");
    }

    static void CheckWebGLSize(string path)
    {
        long total = 0;
        if (!Directory.Exists(path)) return;
        foreach (var f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            total += new FileInfo(f).Length;
        double mb = total / (1024.0 * 1024.0);
        UnityEngine.Debug.Log($"[AutoBuild] WebGL размер: {mb:F2} MB" + (mb > 50 ? " — ПРЕВЫШАЕТ 50MB!" : " — OK"));
    }

    static string[] GetScenes()
    {
        var list = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list.ToArray();
    }

    static void TestPCPerformance()
    {
        UnityEngine.Debug.Log("=== PC Performance ===");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long sum = 0; for (int i = 0; i < 1_000_000; i++) sum += i;
        sw.Stop();
        UnityEngine.Debug.Log($"  CPU: {sw.ElapsedMilliseconds}ms | RAM: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
        UnityEngine.Debug.Log($"  GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize}MB)");
    }

    static void TestMobilePerformance()
    {
        UnityEngine.Debug.Log("=== Mobile Performance ===");
        UnityEngine.Debug.Log($"  Touch: {Input.touchSupported} | Gyro: {SystemInfo.supportsGyroscope}");
        UnityEngine.Debug.Log($"  RAM: {SystemInfo.systemMemorySize}MB | CPU: {SystemInfo.processorCount} cores");
    }

    static void TestWebGLPerformance()
    {
        UnityEngine.Debug.Log("=== WebGL Performance ===");
        UnityEngine.Debug.Log($"  Memory: {PlayerSettings.WebGL.memorySize}MB");
        UnityEngine.Debug.Log($"  Compression: {PlayerSettings.WebGL.compressionFormat}");
        UnityEngine.Debug.Log($"  Threads: {PlayerSettings.WebGL.threadsSupport}");
    }
}
