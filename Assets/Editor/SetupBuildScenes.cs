using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupBuildScenes
{
    private const string MAIN_SCENE = "Assets/_Project/01_Scenes/SneakerShop.unity";

    static EditorBuildSettingsScene[] SneakerScene => new[]
    {
        new EditorBuildSettingsScene(MAIN_SCENE, true)
    };

    // ── Общая настройка сцен ─────────────────────────────────────────────────
    [MenuItem("Build/Настроить сцены для сборки (SneakerShop)")]
    public static void SetupScenes()
    {
        EditorBuildSettings.scenes = SneakerScene;
        Debug.Log($"[Build] Build Settings: {MAIN_SCENE}");
        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor")).Show();
    }

    // ── PC ───────────────────────────────────────────────────────────────────
    [MenuItem("Build/PC/Собрать SneakerShop → Windows")]
    public static void BuildPC()
    {
        EditorBuildSettings.scenes = SneakerScene;
        string path = EditorUtility.SaveFolderPanel("Папка для Windows сборки", "", "");
        if (string.IsNullOrEmpty(path)) return;

        PlayerSettings.productName = "Sneaker Shop";
        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = new[] { MAIN_SCENE },
            locationPathName = Path.Combine(path, "SneakerShop.exe"),
            target           = BuildTarget.StandaloneWindows64,
            options          = BuildOptions.ShowBuiltPlayer
        });
        Debug.Log($"[Build] Windows готов: {path}");
    }

    // ── Android ──────────────────────────────────────────────────────────────
    [MenuItem("Build/Mobile/Собрать SneakerShop → Android APK")]
    public static void BuildAndroid()
    {
        EditorBuildSettings.scenes = SneakerScene;
        string path = EditorUtility.SaveFolderPanel("Папка для Android APK", "", "");
        if (string.IsNullOrEmpty(path)) return;

        PlayerSettings.productName = "Sneaker Shop";
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.Android, "MOBILE_VERSION;ANDROID");

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = new[] { MAIN_SCENE },
            locationPathName = Path.Combine(path, "SneakerShop.apk"),
            target           = BuildTarget.Android,
            options          = BuildOptions.None
        });
        Debug.Log($"[Build] Android APK готов: {path}");
    }

    // ── WebGL ────────────────────────────────────────────────────────────────
    [MenuItem("Build/WebGL/Собрать SneakerShop → WebGL")]
    public static void BuildWebGL()
    {
        EditorBuildSettings.scenes = SneakerScene;
        string path = EditorUtility.SaveFolderPanel("Папка для WebGL сборки", "", "");
        if (string.IsNullOrEmpty(path)) return;

        PlayerSettings.productName              = "Sneaker Shop";
        PlayerSettings.WebGL.compressionFormat  = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.memorySize         = 256;
        PlayerSettings.WebGL.dataCaching        = true;
        PlayerSettings.defaultScreenWidth       = 1280;
        PlayerSettings.defaultScreenHeight      = 720;

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.WebGL, "WEBGL_VERSION");

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = new[] { MAIN_SCENE },
            locationPathName = path,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        });
        Debug.Log($"[Build] WebGL готов: {path}");
    }
}
