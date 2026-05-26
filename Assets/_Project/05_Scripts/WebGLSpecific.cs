using UnityEngine;
using System.Runtime.InteropServices;

public class WebGLSpecific : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void JS_OpenNewTab(string url);
    [DllImport("__Internal")] private static extern void JS_CopyToClipboard(string text);
    [DllImport("__Internal")] private static extern void JS_SetFullscreen(int fs);
    [DllImport("__Internal")] private static extern void JS_DisableContextMenu();
#endif

    [Header("WebGL Settings")]
#pragma warning disable 0414
    [SerializeField] private bool enableWebGLFeatures = true;
    [SerializeField] private bool pauseOnFocusLoss    = true;
#pragma warning restore 0414

    public static WebGLSpecific Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_WEBGL && !UNITY_EDITOR
        if (enableWebGLFeatures) InitializeWebGL();
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    void InitializeWebGL()
    {
        Cursor.lockState = CursorLockMode.Confined;
        JS_DisableContextMenu();
        Debug.Log("[WebGL] Инициализирован.");
    }
#endif

    // ── Публичное API ────────────────────────────────────────────────────────
    public void OpenURL(string url)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_OpenNewTab(url);
#else
        Application.OpenURL(url);
#endif
    }

    public void CopyToClipboard(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_CopyToClipboard(text);
#else
        GUIUtility.systemCopyBuffer = text;
        Debug.Log($"[WebGL] Скопировано: {text}");
#endif
    }

    public void ToggleFullscreen(bool fs)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_SetFullscreen(fs ? 1 : 0);
#else
        Screen.fullScreen = fs;
#endif
    }

    public void CopyGameLink()
    {
        string link = Application.absoluteURL;
        CopyToClipboard(link);
        Debug.Log($"[WebGL] Ссылка: {link}");
    }

    // ── Сохранение ──────────────────────────────────────────────────────────
    public void SaveProgress(string data)
    {
        PlayerPrefs.SetString("GameProgress", data);
        PlayerPrefs.Save();
        Debug.Log("[WebGL] Прогресс сохранён.");
    }

    public string LoadProgress()
    {
        return PlayerPrefs.GetString("GameProgress", "");
    }

    // ── Пауза при потере фокуса ──────────────────────────────────────────────
    void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_WEBGL
        if (!pauseOnFocusLoss) return;
        Time.timeScale = hasFocus ? 1f : 0f;
        AudioListener.pause = !hasFocus;
        if (!hasFocus) Debug.Log("[WebGL] Фокус потерян — пауза.");
#endif
    }
}
