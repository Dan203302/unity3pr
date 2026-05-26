using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupUIPanel
{
    [MenuItem("Tools/Setup UI Panel")]
    public static void Run()
    {
        // Find or create EventSystem with New Input System module
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        else
        {
            // Replace StandaloneInputModule if present
            var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                Object.DestroyImmediate(oldModule);
                if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                    es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("[UI] Replaced StandaloneInputModule with InputSystemUIInputModule.");
            }
        }

        // Find Canvas
        GameObject canvasGo = GameObject.Find("UICanvas");
        if (canvasGo == null) { Debug.LogError("UICanvas not found!"); return; }

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Setup Panel
        GameObject panel = canvasGo.transform.Find("Panel")?.gameObject;
        if (panel == null) { Debug.LogError("Panel not found inside UICanvas!"); return; }
        SetupRect(panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                  new Vector2(10, -10), new Vector2(320, 260));
        Image panelImg = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

        // Title Text — top of panel, full width, height 40
        GameObject title = SetupTMPText(panel, "TitleText",
            "Панель управления",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, -22), new Vector2(-20, 40), 22, FontStyles.Bold,
            new Color(1f, 0.85f, 0.2f));

        // Status Text — below title, full width, height 36
        GameObject status = SetupTMPText(panel, "StatusText",
            "Ожидание действия пользователя.",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, -68), new Vector2(-20, 36), 13, FontStyles.Normal,
            Color.white);

        // Buttons
        CreateButton(panel, "Button_ChangeColor",  "Изменить цвет",   new Vector2(10, -115), new Vector2(-10, -145));
        CreateButton(panel, "Button_MoveObject",   "Переместить объект", new Vector2(10, -155), new Vector2(-10, -185));
        CreateButton(panel, "Button_ShowMessage",  "Показать сообщение", new Vector2(10, -195), new Vector2(-10, -225));

        // UIController + UIActionHandler
        GameObject uiCtrl = GameObject.Find("UIController");
        if (uiCtrl == null) uiCtrl = new GameObject("UIController");

        UIActionHandler handler = uiCtrl.GetComponent<UIActionHandler>() ?? uiCtrl.AddComponent<UIActionHandler>();

        // Assign StatusText
        var tmpStatus = status.GetComponent<TextMeshProUGUI>();
        if (tmpStatus != null)
        {
            var so = new SerializedObject(handler);
            so.FindProperty("statusText").objectReferenceValue = tmpStatus;
            so.ApplyModifiedProperties();
        }

        // Assign PhysicsObject_Pract6 as target
        GameObject physObj = GameObject.Find("PhysicsObject_Pract6");
        if (physObj != null)
        {
            var so = new SerializedObject(handler);
            Renderer rend = physObj.GetComponent<Renderer>();
            if (rend) so.FindProperty("targetRenderer").objectReferenceValue = rend;
            so.ApplyModifiedProperties();
        }

        // Wire up buttons
        WireButton(panel, "Button_ChangeColor",  uiCtrl, handler, "ChangeColor");
        WireButton(panel, "Button_MoveObject",   uiCtrl, handler, "MoveObject");
        WireButton(panel, "Button_ShowMessage",  uiCtrl, handler, "ShowMessage");

        EditorUtility.SetDirty(canvasGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[UI] Setup complete!");
    }

    static void SetupRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                          Vector2 offsetMin, Vector2 sizeDelta)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = offsetMin;
        rt.sizeDelta = sizeDelta;
    }

    static TMP_FontAsset GetDefaultFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null)
        {
            var guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
            if (guids.Length > 0)
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        if (font == null) Debug.LogWarning("[UI] TMP Font not found!");
        else Debug.Log("[UI] Font loaded: " + font.name);
        return font;
    }

    static GameObject SetupTMPText(GameObject parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta, float size, FontStyles style, Color color)
    {
        Transform existing = parent.transform.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var font = GetDefaultFont();
        if (font != null) tmp.font = font;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        return go;
    }

    static void CreateButton(GameObject parent, string name, string label,
                             Vector2 offsetMin, Vector2 offsetMax)
    {
        Transform existing = parent.transform.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        Image img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = Color.white;

        Button btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = new Color(0.2f, 0.5f, 0.9f);
        colors.highlightedColor = new Color(0.35f, 0.65f, 1f);
        colors.pressedColor     = new Color(0.1f, 0.3f, 0.7f);
        colors.selectedColor    = new Color(0.2f, 0.5f, 0.9f);
        btn.colors = colors;
        btn.targetGraphic = img;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
        rt.offsetMax = new Vector2(offsetMax.x, offsetMin.y);

        // Label
        Transform lblT = go.transform.Find("Text");
        GameObject lblGo = lblT != null ? lblT.gameObject : new GameObject("Text");
        lblGo.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = lblGo.GetComponent<TextMeshProUGUI>() ?? lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var btnFont = GetDefaultFont();
        if (btnFont != null) tmp.font = btnFont;
        RectTransform lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;
    }

    static void WireButton(GameObject panel, string btnName, GameObject controller,
                           UIActionHandler handler, string methodName)
    {
        Transform t = panel.transform.Find(btnName);
        if (t == null) return;
        Button btn = t.GetComponent<Button>();
        if (btn == null) return;

        // Remove all persistent listeners first
        var so = new SerializedObject(btn);
        var onClickProp = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (onClickProp != null) { onClickProp.ClearArray(); so.ApplyModifiedProperties(); }

        btn.onClick.RemoveAllListeners();

        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            btn.onClick,
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction),
                handler, methodName) as UnityEngine.Events.UnityAction);
    }
}
