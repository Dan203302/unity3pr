using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AdaptiveUI : MonoBehaviour
{
    [System.Serializable]
    public class PlatformUISettings
    {
        [Header("Размеры шрифта")]
        public int pcFontSize     = 16;
        public int mobileFontSize = 22;
        public int webGLFontSize  = 18;

        [Header("Размеры кнопок")]
        public Vector2 pcButtonSize     = new Vector2(160, 40);
        public Vector2 mobileButtonSize = new Vector2(200, 64);
        public Vector2 webGLButtonSize  = new Vector2(180, 50);

        [Header("Минимальный tap-размер (мобильные)")]
        public float minTouchableSize = 44f;
    }

    [SerializeField] private PlatformUISettings settings = new PlatformUISettings();

    [Header("Элементы для адаптации")]
    [SerializeField] private List<TextMeshProUGUI> textElements  = new List<TextMeshProUGUI>();
    [SerializeField] private List<Button>          buttons       = new List<Button>();

    [Header("Платформо-специфичные объекты")]
    [SerializeField] private GameObject pcOnlyElements;
    [SerializeField] private GameObject mobileOnlyElements;
    [SerializeField] private GameObject webGLOnlyElements;

    private CanvasScaler canvasScaler;

    void Start()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
        ApplyPlatformSettings();
    }

    public void ApplyPlatformSettings()
    {
#if UNITY_STANDALONE && !UNITY_EDITOR
        ApplyPCSettings();
#elif UNITY_ANDROID || UNITY_IOS
        ApplyMobileSettings();
#elif UNITY_WEBGL
        ApplyWebGLSettings();
#else
        ApplyPCSettings(); // редактор
#endif
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    void ApplyPCSettings()
    {
        ApplyFontSize(settings.pcFontSize, autoSize: false);
        ApplyButtonSizes(settings.pcButtonSize);
        SetPlatformObjects(true, false, false);
        SetCanvasScaler(new Vector2(1920, 1080), 0.5f);
        Debug.Log("[AdaptiveUI] PC настройки применены.");
    }

    void ApplyMobileSettings()
    {
        ApplyFontSize(settings.mobileFontSize, autoSize: true);
        ApplyButtonSizes(settings.mobileButtonSize, enforceMinSize: true);
        SetPlatformObjects(false, true, false);
        SetCanvasScaler(Screen.width > Screen.height
            ? new Vector2(1920, 1080)
            : new Vector2(1080, 1920),
            Screen.width > Screen.height ? 0.5f : 0f);
        ApplySafeArea();
        Debug.Log("[AdaptiveUI] Mobile настройки применены.");
    }

    void ApplyWebGLSettings()
    {
        ApplyFontSize(settings.webGLFontSize, autoSize: true);
        ApplyButtonSizes(settings.webGLButtonSize);
        SetPlatformObjects(false, false, true);
        SetCanvasScaler(new Vector2(1280, 720), 0.5f);
        Debug.Log("[AdaptiveUI] WebGL настройки применены.");
    }

    void ApplyFontSize(int size, bool autoSize)
    {
        foreach (var t in textElements)
        {
            if (t == null) continue;
            t.fontSize = size;
            t.enableAutoSizing = autoSize;
            if (autoSize) { t.fontSizeMin = 12; t.fontSizeMax = size; }
        }
    }

    void ApplyButtonSizes(Vector2 size, bool enforceMinSize = false)
    {
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            var rt = btn.GetComponent<RectTransform>();
            if (rt == null) continue;
            float w = enforceMinSize ? Mathf.Max(size.x, settings.minTouchableSize) : size.x;
            float h = enforceMinSize ? Mathf.Max(size.y, settings.minTouchableSize) : size.y;
            rt.sizeDelta = new Vector2(w, h);

            var nav = btn.navigation;
            nav.mode = enforceMinSize ? Navigation.Mode.None : Navigation.Mode.Automatic;
            btn.navigation = nav;
        }
    }

    void SetPlatformObjects(bool pc, bool mobile, bool webgl)
    {
        if (pcOnlyElements     != null) pcOnlyElements.SetActive(pc);
        if (mobileOnlyElements != null) mobileOnlyElements.SetActive(mobile);
        if (webGLOnlyElements  != null) webGLOnlyElements.SetActive(webgl);
    }

    void SetCanvasScaler(Vector2 refRes, float match)
    {
        if (canvasScaler == null) return;
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = refRes;
        canvasScaler.matchWidthOrHeight  = match;
    }

    void ApplySafeArea()
    {
        var sa = Screen.safeArea;
        var anchor = new Vector2(sa.x / Screen.width,  sa.y / Screen.height);
        var anchorMax = new Vector2((sa.x + sa.width) / Screen.width,
                                   (sa.y + sa.height) / Screen.height);
        var rt = GetComponent<RectTransform>();
        if (rt != null) { rt.anchorMin = anchor; rt.anchorMax = anchorMax; }
        Debug.Log($"[AdaptiveUI] Safe area applied: {sa}");
    }

    public void RefreshUI() => ApplyPlatformSettings();

#if UNITY_EDITOR
    void OnValidate() { if (Application.isPlaying) ApplyPlatformSettings(); }
#endif
}
