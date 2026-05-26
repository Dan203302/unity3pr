using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class WebGLBuildSettings
{
    [MenuItem("Build/WebGL/Standard Build")]
    public static void BuildWebGLStandard()
    {
        string path = EditorUtility.SaveFolderPanel("Папка для WebGL сборки", "", "");
        if (string.IsNullOrEmpty(path)) return;

        ConfigureWebGL();

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = GetWebGLScenes(),
            locationPathName = path,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        });

        CreateCustomIndexHTML(path);
        Debug.Log($"[Build] WebGL готов: {path}");
    }

    [MenuItem("Build/WebGL/Оптимизация для WebGL")]
    public static void OptimizeForWebGL()
    {
        PlayerSettings.stripEngineCode           = true;
        PlayerSettings.stripUnusedMeshComponents = true;
        QualitySettings.SetQualityLevel(0);
        QualitySettings.antiAliasing    = 0;
        QualitySettings.shadows         = ShadowQuality.Disable;
        QualitySettings.globalTextureMipmapLimit = 2;
        Debug.Log("[Build] Оптимизация для WebGL применена.");
    }

    [MenuItem("Build/WebGL/Проверить размер сборки")]
    public static void CheckBuildSize()
    {
        string path = "Build/WebGL";
        if (!Directory.Exists(path)) { Debug.LogWarning("[Build] Папка Build/WebGL не найдена."); return; }

        long total = 0;
        foreach (var f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            total += new FileInfo(f).Length;

        double mb = total / (1024.0 * 1024.0);
        Debug.Log($"[Build] Размер WebGL сборки: {mb:F2} MB");
        if (mb > 50) Debug.LogWarning($"[Build] Сборка > 50MB ({mb:F2} MB). Рекомендуется оптимизировать.");
    }

    static void ConfigureWebGL()
    {
        PlayerSettings.WebGL.compressionFormat  = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.memorySize         = 256;
        PlayerSettings.WebGL.exceptionSupport   = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.dataCaching        = true;
        PlayerSettings.WebGL.threadsSupport     = false;
        PlayerSettings.defaultScreenWidth       = 1280;
        PlayerSettings.defaultScreenHeight      = 720;

        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.WebGL,
            "WEBGL_VERSION;NO_MULTITHREADING");
    }

    static void CreateCustomIndexHTML(string buildPath)
    {
        string file = Path.Combine(buildPath, "index.html");
        if (!File.Exists(file)) return;

        string html = File.ReadAllText(file);
        string css  = "<style>.webgl-logo{display:none}.progress{height:16px;background:#444}.progress-bar{background:#4CAF50}</style>";
        string info = "<div style='text-align:center;padding:12px;color:#eee'><b>Управление:</b> WASD — движение | Мышь — взгляд | Пробел — прыжок | E — взаимодействие<br>Tab — энергия | 1/2/3/4 — способности | F — продажа</div>";

        html = html.Replace("</head>", css + "</head>");
        html = html.Replace("</body>", info + "</body>");
        File.WriteAllText(file, html, Encoding.UTF8);
        Debug.Log("[Build] index.html кастомизирован.");
    }

    static string[] GetWebGLScenes()
    {
        var list = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (!s.enabled) continue;
            if (s.path.Contains("Editor") || s.path.Contains("Test")) continue;
            list.Add(s.path);
            if (list.Count >= 3) break;
        }
        return list.ToArray();
    }
}
