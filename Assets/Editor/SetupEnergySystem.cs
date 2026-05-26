using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupEnergySystem
{
    [MenuItem("Tools/Setup Energy System")]
    public static void Run()
    {
        // 1. Создаём BalanceConfig ScriptableObject если нет
        const string configPath = "Assets/_Project/BalanceConfig.asset";
        BalanceConfig config = AssetDatabase.LoadAssetAtPath<BalanceConfig>(configPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<BalanceConfig>();
            if (!System.IO.Directory.Exists("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[SetupEnergy] BalanceConfig создан: " + configPath);
        }

        // 2. Находим или создаём EnergyManager GameObject
        GameObject em = GameObject.Find("EnergyManager");
        if (em == null) em = new GameObject("EnergyManager");

        // 3. Добавляем компоненты
        var es = em.GetComponent<EnergySystem>() ?? em.AddComponent<EnergySystem>();
        var ab = em.GetComponent<AbilitySystem>() ?? em.AddComponent<AbilitySystem>();
        var fb = em.GetComponent<FeedbackSystem>() ?? em.AddComponent<FeedbackSystem>();
        var bt = em.GetComponent<BalanceTester>() ?? em.AddComponent<BalanceTester>();

        // 4. Привязываем BalanceConfig через SerializedObject
        void AssignConfig(Object target, string propName)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propName);
            if (prop != null) { prop.objectReferenceValue = config; so.ApplyModifiedProperties(); }
        }
        AssignConfig(es, "balanceConfig");
        AssignConfig(ab, "balanceConfig");
        AssignConfig(bt, "balanceConfig");

        // Привязываем EnergySystem в AbilitySystem
        var abSo = new SerializedObject(ab);
        var esProp = abSo.FindProperty("energySystem");
        if (esProp != null) { esProp.objectReferenceValue = es; abSo.ApplyModifiedProperties(); }

        // 5. Добавляем SneakerComboMechanic на Player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            var combo = player.GetComponent<SneakerComboMechanic>() ?? player.AddComponent<SneakerComboMechanic>();
            var comboSo = new SerializedObject(combo);
            comboSo.FindProperty("energySystem")?.Let(p => { p.objectReferenceValue = es; });
            comboSo.FindProperty("abilitySystem")?.Let(p => { p.objectReferenceValue = ab; });
            comboSo.FindProperty("balanceConfig")?.Let(p => { p.objectReferenceValue = config; });
            comboSo.ApplyModifiedProperties();
            Debug.Log("[SetupEnergy] SneakerComboMechanic добавлен на Player.");
        }
        else
        {
            Debug.LogWarning("[SetupEnergy] Player не найден — добавь SneakerComboMechanic вручную.");
        }

        // 6. Создаём UI панель
        SetupEnergyUI(fb);

        EditorUtility.SetDirty(em);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SetupEnergy] Готово! Запусти BalanceTester в Play Mode для анализа баланса.");
    }

    static void SetupEnergyUI(FeedbackSystem fb)
    {
        // Ищем или создаём Canvas
        GameObject canvasGo = GameObject.Find("EnergyCanvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("EnergyCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        // Панель слева
        GameObject panel = canvasGo.transform.Find("EnergyPanel")?.gameObject;
        if (panel == null) panel = new GameObject("EnergyPanel");
        panel.transform.SetParent(canvasGo.transform, false);
        if (panel.GetComponent<RectTransform>() == null) panel.AddComponent<RectTransform>();
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(10, 0);
        rt.sizeDelta = new Vector2(220, 240);
        var img = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.75f);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // Заголовок
        MakeTMPLabel(panel, "Title", "=== ЭНЕРГИЯ ===", font,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -14), new Vector2(0, 24), 13, Color.cyan);

        // Энергия
        var energyGo = MakeTMPLabel(panel, "EnergyText", "Хайп: 0\nКачество: 0\nРедкость: 0", font,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -44), new Vector2(0, 56), 12, Color.white);

        // Способность
        var abilityGo = MakeTMPLabel(panel, "AbilityText", "[1] Хайп-Продажа\n[2] Питч\n[3] Редкая Находка\n[4] Ультимат", font,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -108), new Vector2(0, 60), 11, Color.yellow);

        // Продажи
        var salesGo = MakeTMPLabel(panel, "SalesText", "Продаж: 0\nИтого: $0", font,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -172), new Vector2(0, 40), 12, Color.cyan);

        // Комбо
        var comboGo = MakeTMPLabel(panel, "ComboText", "", font,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 8), new Vector2(0, 28), 12, Color.green);

        // Привязываем к FeedbackSystem
        if (fb != null)
        {
            var fbSo = new SerializedObject(fb);
            SetTMPRef(fbSo, "energyText",  energyGo.GetComponent<TMP_Text>());
            SetTMPRef(fbSo, "abilityText", abilityGo.GetComponent<TMP_Text>());
            SetTMPRef(fbSo, "comboText",   comboGo.GetComponent<TMP_Text>());
            SetTMPRef(fbSo, "salesText",   salesGo.GetComponent<TMP_Text>());
            fbSo.ApplyModifiedProperties();
        }

        Debug.Log("[SetupEnergy] Energy UI создан.");
    }

    static void SetTMPRef(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = value;
    }

    static GameObject MakeTMPLabel(GameObject parent, string name, string text, TMP_FontAsset font,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 sizeDelta, float size, Color color)
    {
        Transform t = parent.transform.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = ancPos; rt.sizeDelta = sizeDelta;
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (font != null) tmp.font = font;
        return go;
    }
}

// Хелпер для удобной цепочки
public static class SerializedPropertyExtensions
{
    public static SerializedProperty Let(this SerializedProperty prop, System.Action<SerializedProperty> action)
    {
        if (prop != null) action(prop);
        return prop;
    }
}
