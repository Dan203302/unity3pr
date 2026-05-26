using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Отображает достижения, ежедневные награды и сезонные события на экране.
/// Кнопка [▶] / [◀] сворачивает/разворачивает панель.
/// </summary>
public class AchievementHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI achievementsText;
    [SerializeField] private TextMeshProUGUI dailyRewardText;
    [SerializeField] private TextMeshProUGUI seasonalText;
    [SerializeField] private GameObject      contentPanel;   // контейнер с текстами
    [SerializeField] private TextMeshProUGUI toggleButtonText;

    [Header("Ссылки")]
    [SerializeField] private AchievementSystem  achievementSystem;
    [SerializeField] private DailyRewardSystem   dailyRewardSystem;
    [SerializeField] private SeasonalEventSystem seasonalEventSystem;

    private float updateTimer;
    private const float UPDATE_INTERVAL = 1f;
    private bool isExpanded = false; // по умолчанию свёрнута

    void Start()
    {
        // Подписка на событие разблокировки ачивки
        if (achievementSystem != null)
            achievementSystem.OnAchievementUnlocked += ShowAchievementPopup;

        if (seasonalEventSystem != null)
        {
            seasonalEventSystem.OnTierReached += (pts, tier) =>
                Debug.Log($"[HUD] Уровень {tier.tierName} достигнут!");
            seasonalEventSystem.OnSeasonalAchievementUnlocked += a =>
                Debug.Log($"[HUD] Сезонная ачивка: {a.title}");
        }

        // Находим кнопку по имени и подключаем
        var btnGo = GameObject.Find("ToggleButton");
        if (btnGo != null)
        {
            var btn = btnGo.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(TogglePanel);
                Debug.Log("[AchievementHUD] Кнопка TogglePanel подключена.");
            }
        }
        else Debug.LogWarning("[AchievementHUD] ToggleButton не найден!");

        // Fallback — ищем contentPanel если не привязан
        if (contentPanel == null)
        {
            var cp = GameObject.Find("ContentPanel");
            if (cp != null) contentPanel = cp;
        }

        // Стартуем свёрнутыми
        SetExpanded(false);
        RefreshHUD();
    }

    public void TogglePanel()
    {
        SetExpanded(!isExpanded);
    }

    void SetExpanded(bool expanded)
    {
        isExpanded = expanded;
        if (contentPanel != null)   contentPanel.SetActive(expanded);
        if (toggleButtonText != null)
            toggleButtonText.text = expanded ? "[<] Стат" : "[>] Стат";
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;
            RefreshHUD();
        }
    }

    void RefreshHUD()
    {
        UpdateAchievementsText();
        UpdateDailyRewardText();
        UpdateSeasonalText();
    }

    void UpdateAchievementsText()
    {
        if (achievementsText == null || achievementSystem == null) return;
        var s = achievementSystem.GetStats();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=cyan>=== АЧИВКИ ===</color>");
        sb.AppendLine($"Открыто: {s.unlockedAchievements}/{s.totalAchievements} ({s.completionPercentage:F0}%)");

        foreach (var a in achievementSystem.achievements)
        {
            if (a.isHidden && !a.isUnlocked) continue;
            string status = a.isUnlocked ? "<color=green>✓</color>" : $"{a.currentValue:F0}/{a.targetValue:F0}";
            sb.AppendLine($"{(a.isUnlocked ? "<color=white>" : "<color=grey>")}{a.title}: {status}</color>");
        }

        achievementsText.text = sb.ToString().TrimEnd();
    }

    void UpdateDailyRewardText()
    {
        if (dailyRewardText == null || dailyRewardSystem == null) return;
        var info = dailyRewardSystem.GetStreakInfo();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=yellow>=== ЕЖЕДНЕВНЫЕ НАГРАДЫ ===</color>");
        sb.AppendLine($"Серия: <color=orange>{info.currentStreak}</color> дней");
        sb.AppendLine($"Бонус: x{info.bonusMultiplier:F2}");
        sb.AppendLine($"До особого дня: {info.daysUntilSpecialReward} дн.");
        sb.AppendLine($"Сброс через: {dailyRewardSystem.GetFormattedTimeUntilReset()}");

        bool hasReward = dailyRewardSystem.HasAvailableReward();
        sb.AppendLine(hasReward
            ? "<color=lime>Нажми F2 — получи награду!</color>"
            : "<color=grey>Уже получено сегодня</color>");

        dailyRewardText.text = sb.ToString().TrimEnd();
    }

    void UpdateSeasonalText()
    {
        if (seasonalText == null || seasonalEventSystem == null) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=orange>=== СЕЗОН РАСПРОДАЖ ===</color>");
        sb.AppendLine(seasonalEventSystem.GetProgressString());

        // Список сезонных ачивок
        foreach (var a in seasonalEventSystem.seasonalAchievements)
        {
            string status = a.isUnlocked
                ? "<color=green>✓</color>"
                : $"{a.currentValue:F0}/{a.targetValue:F0}";
            sb.AppendLine($"{(a.isUnlocked ? "<color=white>" : "<color=grey>")}{a.title}: {status}</color>");
        }

        sb.AppendLine("\n<color=cyan>Лидерборд:</color>");
        sb.AppendLine(seasonalEventSystem.GetLeaderboardString());

        seasonalText.text = sb.ToString().TrimEnd();
    }

    void ShowAchievementPopup(AchievementSystem.Achievement a)
    {
        Debug.Log($"[HUD] POPUP: Достижение «{a.title}» разблокировано!");
    }
}

/// <summary>
/// Вешается на кнопку — находит AchievementHUD и подключает TogglePanel в рантайме.
/// </summary>
public class HUDToggleButton : MonoBehaviour
{
    void Start()
    {
        var btn = GetComponent<UnityEngine.UI.Button>();
        var hud = FindFirstObjectByType<AchievementHUD>();
        if (btn != null && hud != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(hud.TogglePanel);
        }
    }
}
