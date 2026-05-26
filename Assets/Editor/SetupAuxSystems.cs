using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupAuxSystems
{
    [MenuItem("Tools/Setup Aux Systems (Achievements + Daily + Season)")]
    public static void Run()
    {
        // ── 1. Создаём ScriptableObject ассеты ──────────────────────────────
        var achievement = GetOrCreateAsset<AchievementSystem>(
            "Assets/_Project/AchievementSystem.asset");
        var daily = GetOrCreateAsset<DailyRewardSystem>(
            "Assets/_Project/DailyRewardSystem.asset");
        var seasonal = GetOrCreateAsset<SeasonalEventSystem>(
            "Assets/_Project/SeasonalEventSystem.asset");

        // Заполняем дефолтные данные если пустые
        FillAchievements(achievement);
        FillDailyRewards(daily);
        FillSeasonalEvent(seasonal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 2. GameSystemsManager ────────────────────────────────────────────
        GameObject managerGo = GameObject.Find("GameSystemsManager");
        if (managerGo == null) managerGo = new GameObject("GameSystemsManager");

        var manager = managerGo.GetComponent<GameSystemsManager>()
                   ?? managerGo.AddComponent<GameSystemsManager>();

        var so = new SerializedObject(manager);
        so.FindProperty("achievementSystem")?.Let(p => p.objectReferenceValue = achievement);
        so.FindProperty("dailyRewardSystem")?.Let(p => p.objectReferenceValue = daily);
        so.FindProperty("seasonalEventSystem")?.Let(p => p.objectReferenceValue = seasonal);
        so.ApplyModifiedProperties();

        // ── 3. SeasonalEventSystem на отдельном GO ───────────────────────────
        GameObject seasonGo = GameObject.Find("SeasonalEventManager");
        if (seasonGo == null) seasonGo = new GameObject("SeasonalEventManager");
        var seasonRunner = seasonGo.GetComponent<SeasonalEventRunner>()
                        ?? seasonGo.AddComponent<SeasonalEventRunner>();
        var soSeason = new SerializedObject(seasonRunner);
        soSeason.FindProperty("seasonalEventSystem")?.Let(p => p.objectReferenceValue = seasonal);
        soSeason.ApplyModifiedProperties();

        // ── 4. Canvas + HUD ──────────────────────────────────────────────────
        SetupHUD(achievement, daily, seasonal);

        // ── 5. Сохраняем сцену ───────────────────────────────────────────────
        EditorUtility.SetDirty(managerGo);
        EditorUtility.SetDirty(seasonGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SetupAux] Готово! Все системы настроены. Нажми Play.");
    }

    // ── ScriptableObject helper ──────────────────────────────────────────────
    static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        Debug.Log($"[SetupAux] Создан: {path}");
        return asset;
    }

    // ── Ачивки ───────────────────────────────────────────────────────────────
    static void FillAchievements(AchievementSystem sys)
    {
        if (sys.achievements != null && sys.achievements.Count > 0) return;
        sys.achievements = new System.Collections.Generic.List<AchievementSystem.Achievement>
        {
            new AchievementSystem.Achievement { id="first_sale",   title="Первая продажа",      description="Продай первый кроссовок",      type=AchievementSystem.AchievementType.TotalSales,   targetValue=1,   rewardXP=50,  rewardCurrency=10 },
            new AchievementSystem.Achievement { id="sales_10",     title="Торговец",             description="Сделай 10 продаж",             type=AchievementSystem.AchievementType.TotalSales,   targetValue=10,  rewardXP=100, rewardCurrency=25 },
            new AchievementSystem.Achievement { id="earned_500",   title="Полтысячи",            description="Заработай $500",               type=AchievementSystem.AchievementType.TotalEarned,  targetValue=500, rewardXP=150, rewardCurrency=50 },
            new AchievementSystem.Achievement { id="combo_3",      title="Комбо-боец",           description="Выполни комбо x3",             type=AchievementSystem.AchievementType.ComboChain,   targetValue=3,   rewardXP=75,  rewardCurrency=15 },
            new AchievementSystem.Achievement { id="ability_5",    title="Способный",            description="Используй 5 способностей",     type=AchievementSystem.AchievementType.AbilityUsed,  targetValue=5,   rewardXP=60,  rewardCurrency=10 },
            new AchievementSystem.Achievement { id="ultimate",     title="Ультиматор",           description="Используй Ультимат 1 раз",     type=AchievementSystem.AchievementType.UltimateDeal, targetValue=1,   rewardXP=200, rewardCurrency=100 },
            new AchievementSystem.Achievement { id="playtime_5",   title="Завсегдатай",          description="Проведи 5 минут в магазине",   type=AchievementSystem.AchievementType.PlayTime,     targetValue=300, rewardXP=50,  rewardCurrency=20 },
        };
        EditorUtility.SetDirty(sys);
    }

    // ── Ежедневные награды ───────────────────────────────────────────────────
    static void FillDailyRewards(DailyRewardSystem sys)
    {
        if (sys.weeklyRewards != null && sys.weeklyRewards.Count > 0) return;
        sys.weeklyRewards = new System.Collections.Generic.List<DailyRewardSystem.DailyReward>
        {
            new DailyRewardSystem.DailyReward { day=1, rewardType=DailyRewardSystem.RewardType.EnergyRed,   rewardAmount=30 },
            new DailyRewardSystem.DailyReward { day=2, rewardType=DailyRewardSystem.RewardType.EnergyBlue,  rewardAmount=30 },
            new DailyRewardSystem.DailyReward { day=3, rewardType=DailyRewardSystem.RewardType.Currency,    rewardAmount=50 },
            new DailyRewardSystem.DailyReward { day=4, rewardType=DailyRewardSystem.RewardType.EnergyGreen, rewardAmount=30 },
            new DailyRewardSystem.DailyReward { day=5, rewardType=DailyRewardSystem.RewardType.BoostSale,   rewardAmount=2  },
            new DailyRewardSystem.DailyReward { day=6, rewardType=DailyRewardSystem.RewardType.Currency,    rewardAmount=100 },
            new DailyRewardSystem.DailyReward { day=7, rewardType=DailyRewardSystem.RewardType.SpecialItem, rewardAmount=1, isSpecial=true },
        };
        EditorUtility.SetDirty(sys);
    }

    // ── Сезонное событие ─────────────────────────────────────────────────────
    static void FillSeasonalEvent(SeasonalEventSystem sys)
    {
        sys.eventName    = "Сезон Распродаж";
        sys.startDateStr = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        sys.durationDays = 10;
        EditorUtility.SetDirty(sys);
    }

    // ── HUD Canvas ───────────────────────────────────────────────────────────
    static void SetupHUD(AchievementSystem ach, DailyRewardSystem daily, SeasonalEventSystem seasonal)
    {
        // Удаляем старые объекты полностью перед пересозданием
        var oldCanvas = GameObject.Find("AuxHUDCanvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        var oldHud = GameObject.Find("AchievementHUD");
        if (oldHud != null) Object.DestroyImmediate(oldHud);

        // Canvas — создаём с нуля
        var canvasGo = new GameObject("AuxHUDCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // Корневая панель (вся правая колонка)
        var root = GetOrCreateChild(canvasGo, "AuxHUDPanel");
        var rootRt = EnsureRect(root);
        rootRt.anchorMin = new Vector2(1, 1);
        rootRt.anchorMax = new Vector2(1, 1);
        rootRt.pivot     = new Vector2(1, 1);
        rootRt.anchoredPosition = new Vector2(-8, -8);
        rootRt.sizeDelta = new Vector2(220, 32); // высота только кнопки пока свёрнута

        var rootImg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        rootImg.color = new Color(0, 0, 0, 0.0f);
        rootImg.raycastTarget = false; // не блокируем клики за панелью

        // Кнопка-заголовок [▶ Стат]
        var btnGo = GetOrCreateChild(root, "ToggleButton");
        var btnRt = EnsureRect(btnGo);
        btnRt.anchorMin = new Vector2(0, 1); btnRt.anchorMax = new Vector2(1, 1);
        btnRt.pivot = new Vector2(0.5f, 1);
        btnRt.anchoredPosition = Vector2.zero;
        btnRt.sizeDelta = new Vector2(0, 28);
        var btnImg = btnGo.GetComponent<Image>() ?? btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.1f, 0.3f, 0.9f);
        var btn = btnGo.GetComponent<Button>() ?? btnGo.AddComponent<Button>();
        if (btnGo.GetComponent<HUDToggleButton>() == null) btnGo.AddComponent<HUDToggleButton>();

        var btnLabelGo = GetOrCreateChild(btnGo, "Label");
        EnsureRect(btnLabelGo);
        var btnTmp = btnLabelGo.GetComponent<TextMeshProUGUI>() ?? btnLabelGo.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "[>] Стат"; btnTmp.fontSize = 11; btnTmp.color = Color.cyan;
        btnTmp.alignment = TMPro.TextAlignmentOptions.Center;
        btnTmp.raycastTarget = false;
        if (font != null) btnTmp.font = font;
        var btnLabelRt = EnsureRect(btnLabelGo);
        btnLabelRt.anchorMin = Vector2.zero; btnLabelRt.anchorMax = Vector2.one;
        btnLabelRt.offsetMin = btnLabelRt.offsetMax = Vector2.zero;

        // Контент-панель (скрыта по умолчанию)
        var content = GetOrCreateChild(root, "ContentPanel");
        var contentRt = EnsureRect(content);
        contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = new Vector2(0, -30);
        contentRt.sizeDelta = new Vector2(0, 480);
        var contentImg = content.GetComponent<Image>() ?? content.AddComponent<Image>();
        contentImg.color = new Color(0, 0, 0, 0.80f);
        contentImg.raycastTarget = false;

        // Три текста внутри контента
        var achGo   = MakeScrollText(content, "AchievementsText", font, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-4),   new Vector2(0,160));
        var dailyGo = MakeScrollText(content, "DailyRewardText",  font, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-170), new Vector2(0,130));
        var seasGo  = MakeScrollText(content, "SeasonalText",     font, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-308), new Vector2(0,170));

        // AchievementHUD
        var hudGo = GetOrCreateChild(canvasGo, "AchievementHUD");
        var hud   = hudGo.GetComponent<AchievementHUD>() ?? hudGo.AddComponent<AchievementHUD>();
        var hudSo = new SerializedObject(hud);
        hudSo.FindProperty("achievementsText")?.Let(p => p.objectReferenceValue = achGo.GetComponent<TMP_Text>());
        hudSo.FindProperty("dailyRewardText")?.Let(p => p.objectReferenceValue  = dailyGo.GetComponent<TMP_Text>());
        hudSo.FindProperty("seasonalText")?.Let(p => p.objectReferenceValue     = seasGo.GetComponent<TMP_Text>());
        hudSo.FindProperty("contentPanel")?.Let(p => p.objectReferenceValue     = content);
        hudSo.FindProperty("toggleButtonText")?.Let(p => p.objectReferenceValue = btnTmp);
        hudSo.FindProperty("achievementSystem")?.Let(p => p.objectReferenceValue  = ach);
        hudSo.FindProperty("dailyRewardSystem")?.Let(p => p.objectReferenceValue  = daily);
        hudSo.FindProperty("seasonalEventSystem")?.Let(p => p.objectReferenceValue = seasonal);
        hudSo.ApplyModifiedProperties();

        // Привязываем кнопку к TogglePanel
        var btnSo = new SerializedObject(btn);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(hud.TogglePanel);

        content.SetActive(false); // свёрнута по умолчанию
        Debug.Log("[SetupAux] HUD создан (свёрнут). Кнопка '▶ Стат' в правом верхнем углу.");
    }

    static GameObject MakeScrollText(GameObject parent, string name, TMP_FontAsset font,
        Vector2 ancMin, Vector2 ancMax, Vector2 pos, Vector2 size)
    {
        var go = GetOrCreateChild(parent, name);
        var rt = EnsureRect(go);
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 10; tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return go;
    }

    static GameObject GetOrCreateChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        // Добавляем RectTransform ДО SetParent
        if (go.GetComponent<RectTransform>() == null)
            go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static RectTransform EnsureRect(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        return rt;
    }
}

// Простой runner для SeasonalEventSystem (нужен MonoBehaviour)
public class SeasonalEventRunner : MonoBehaviour
{
    [SerializeField] public SeasonalEventSystem seasonalEventSystem;
    void Start() => seasonalEventSystem?.Initialize();
}
