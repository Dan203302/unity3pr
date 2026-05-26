using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Самостоятельная работа: Система сезонных событий.
/// Тема: "Сезон Распродаж" в магазине кроссовок.
///
/// Условия:
/// - Длительность: 7-14 дней (настраивается)
/// - 3 уровня наград: Бронза, Серебро, Золото
/// - Прогрессия по активности (продажи, комбо), не по времени
/// - Лидерборд (локальный топ-5)
/// - Временные ачивки только во время события
/// </summary>
[CreateAssetMenu(fileName = "SeasonalEventSystem", menuName = "Game/Seasonal Event System")]
public class SeasonalEventSystem : ScriptableObject
{
    // ─── Конфиг ────────────────────────────────────────────────────────────────
    [Header("=== СОБЫТИЕ ===")]
    public string eventName = "Сезон Распродаж";
    [Tooltip("Дата начала события (ISO 8601)")]
    public string startDateStr = "2025-06-01T00:00:00";
    [Tooltip("Длительность в днях (7-14)")]
    [Range(7, 14)] public int durationDays = 10;

    [Header("=== УРОВНИ НАГРАД ===")]
    public RewardTier bronzeTier  = new RewardTier { tierName = "Бронза",  minPoints = 50,   bonusMultiplier = 1.2f, reward = "x1.2 к цене навсегда" };
    public RewardTier silverTier  = new RewardTier { tierName = "Серебро", minPoints = 200,  bonusMultiplier = 1.5f, reward = "x1.5 к цене навсегда" };
    public RewardTier goldTier    = new RewardTier { tierName = "Золото",  minPoints = 500,  bonusMultiplier = 2.0f, reward = "x2.0 к цене навсегда" };

    [Header("=== ВРЕМЕННЫЕ АЧИВКИ ===")]
    public List<SeasonalAchievement> seasonalAchievements = new List<SeasonalAchievement>();

    [Header("=== ЛИДЕРБОРД ===")]
    public int leaderboardSize = 5;

    // ─── Состояние ─────────────────────────────────────────────────────────────
    [HideInInspector] public int   playerPoints;
    [HideInInspector] public float earnedPermanentBonus = 1f;
    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
    private bool initialized;

    // ─── События ───────────────────────────────────────────────────────────────
    public event Action<int, RewardTier> OnTierReached;
    public event Action<SeasonalAchievement> OnSeasonalAchievementUnlocked;

    // ─── Типы ──────────────────────────────────────────────────────────────────
    [Serializable]
    public class RewardTier
    {
        public string tierName;
        public int    minPoints;
        public float  bonusMultiplier;
        public string reward;
        [HideInInspector] public bool claimed;
    }

    [Serializable]
    public class SeasonalAchievement
    {
        public string id;
        public string title;
        public string description;
        public int    pointReward;
        public float  targetValue;
        [HideInInspector] public float  currentValue;
        [HideInInspector] public bool   isUnlocked;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int    score;
        public string tier;
    }

    // ─── Инициализация ─────────────────────────────────────────────────────────
    public void Initialize()
    {
        if (initialized) return;
        LoadData();
        initialized = true;

        if (!IsEventActive())
        {
            Debug.Log($"[SeasonalEvent] «{eventName}» — сейчас неактивно.");
            return;
        }

        Debug.Log($"[SeasonalEvent] «{eventName}» активно! Дней осталось: {DaysLeft}. Твои очки: {playerPoints}");

        // Наполняем дефолтные ачивки если список пустой
        if (seasonalAchievements.Count == 0) CreateDefaultAchievements();

        // Подписываемся на игровые события
        GameEvents.OnSaleCompleted   += OnSale;
        GameEvents.OnComboChain      += OnCombo;
        GameEvents.OnAbilityUsed     += OnAbility;
    }

    // ─── Логика ────────────────────────────────────────────────────────────────
    public bool IsEventActive()
    {
        if (!DateTime.TryParse(startDateStr, out var start)) return false;
        var end = start.AddDays(durationDays);
        return DateTime.Now >= start && DateTime.Now <= end;
    }

    public int DaysLeft
    {
        get
        {
            if (!DateTime.TryParse(startDateStr, out var start)) return 0;
            var end = start.AddDays(durationDays);
            return Mathf.Max(0, (int)(end - DateTime.Now).TotalDays);
        }
    }

    // Очки за продажу = сумма / 10
    private void OnSale(int amount)
    {
        if (!IsEventActive()) return;
        AddPoints(Mathf.Max(1, amount / 10), "Продажа");
    }

    // Очки за комбо = цепь * 5
    private void OnCombo(int chain)
    {
        if (!IsEventActive()) return;
        AddPoints(chain * 5, $"Комбо x{chain}");
        UpdateSeasonalAchievement("combo_chain", chain);
    }

    // Очки за каждую способность = 2
    private void OnAbility(string name)
    {
        if (!IsEventActive()) return;
        AddPoints(2, $"Способность: {name}");
        UpdateSeasonalAchievement("ability_count", 1);
        if (name == "Ультимативная Сделка")
            UpdateSeasonalAchievement("ultimate_count", 1);
    }

    public void AddPoints(int pts, string reason = "")
    {
        playerPoints += pts;
        Debug.Log($"[SeasonalEvent] +{pts} очков ({reason}) → Итого: {playerPoints}");

        CheckTiers();
        CheckSeasonalAchievements();
        AddToLeaderboard("Вы", playerPoints);
        SaveData();
    }

    // ─── Уровни наград ─────────────────────────────────────────────────────────
    void CheckTiers()
    {
        TryClaimTier(goldTier);
        TryClaimTier(silverTier);
        TryClaimTier(bronzeTier);
    }

    void TryClaimTier(RewardTier tier)
    {
        if (tier.claimed || playerPoints < tier.minPoints) return;
        tier.claimed = true;
        earnedPermanentBonus = Mathf.Max(earnedPermanentBonus, tier.bonusMultiplier);
        Debug.Log($"[SeasonalEvent] УРОВЕНЬ {tier.tierName.ToUpper()} достигнут! Награда: {tier.reward}");
        OnTierReached?.Invoke(playerPoints, tier);
    }

    // ─── Временные ачивки ──────────────────────────────────────────────────────
    void UpdateSeasonalAchievement(string id, float inc)
    {
        foreach (var a in seasonalAchievements)
        {
            if (a.id != id || a.isUnlocked) continue;
            a.currentValue += inc;
            if (a.currentValue >= a.targetValue)
            {
                a.isUnlocked = true;
                AddPoints(a.pointReward, $"Сезонная ачивка: {a.title}");
                Debug.Log($"[SeasonalEvent] СЕЗОННАЯ АЧИВКА: {a.title} (+{a.pointReward} очков)");
                OnSeasonalAchievementUnlocked?.Invoke(a);
            }
            break;
        }
    }

    void CheckSeasonalAchievements()
    {
        UpdateSeasonalAchievement("points_100",  playerPoints >= 100  ? 100  : 0);
        UpdateSeasonalAchievement("points_300",  playerPoints >= 300  ? 300  : 0);
    }

    // ─── Лидерборд ─────────────────────────────────────────────────────────────
    public void AddToLeaderboard(string name, int score)
    {
        var existing = leaderboard.Find(e => e.playerName == name);
        if (existing != null) existing.score = score;
        else leaderboard.Add(new LeaderboardEntry { playerName = name, score = score, tier = GetCurrentTier() });

        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        if (leaderboard.Count > leaderboardSize)
            leaderboard.RemoveRange(leaderboardSize, leaderboard.Count - leaderboardSize);
    }

    public string GetLeaderboardString()
    {
        if (leaderboard.Count == 0) return "Пусто";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < leaderboard.Count; i++)
            sb.AppendLine($"{i + 1}. {leaderboard[i].playerName} — {leaderboard[i].score} очков [{leaderboard[i].tier}]");
        return sb.ToString();
    }

    public string GetCurrentTier()
    {
        if (playerPoints >= goldTier.minPoints)   return "Золото";
        if (playerPoints >= silverTier.minPoints) return "Серебро";
        if (playerPoints >= bronzeTier.minPoints) return "Бронза";
        return "Без награды";
    }

    public string GetProgressString()
    {
        if (!IsEventActive()) return $"«{eventName}» неактивно";
        int nextGoal = playerPoints < bronzeTier.minPoints  ? bronzeTier.minPoints  :
                       playerPoints < silverTier.minPoints  ? silverTier.minPoints  :
                       playerPoints < goldTier.minPoints    ? goldTier.minPoints    : -1;
        string next = nextGoal > 0 ? $" → До след. уровня: {nextGoal - playerPoints}" : " (Максимум!)";
        return $"[{eventName}] {GetCurrentTier()} | Очки: {playerPoints}{next} | Дней: {DaysLeft}";
    }

    // ─── Создание дефолтных ачивок ─────────────────────────────────────────────
    void CreateDefaultAchievements()
    {
        seasonalAchievements.Add(new SeasonalAchievement { id = "combo_chain",    title = "Комбо-Мастер",       description = "Выполни комбо x3",           targetValue = 3,   pointReward = 30  });
        seasonalAchievements.Add(new SeasonalAchievement { id = "ability_count",  title = "Способный Продавец", description = "Используй 10 способностей",   targetValue = 10,  pointReward = 20  });
        seasonalAchievements.Add(new SeasonalAchievement { id = "ultimate_count", title = "Ультимативный",      description = "Используй Ультимат 3 раза",   targetValue = 3,   pointReward = 50  });
        seasonalAchievements.Add(new SeasonalAchievement { id = "points_100",     title = "Начинающий",         description = "Набери 100 очков события",    targetValue = 100, pointReward = 10  });
        seasonalAchievements.Add(new SeasonalAchievement { id = "points_300",     title = "Опытный",            description = "Набери 300 очков события",    targetValue = 300, pointReward = 25  });
    }

    // ─── Сохранение ────────────────────────────────────────────────────────────
    void SaveData()
    {
        PlayerPrefs.SetInt("SeasonPoints", playerPoints);
        PlayerPrefs.SetFloat("SeasonBonus", earnedPermanentBonus);
        PlayerPrefs.SetInt("SeasonBronze",  bronzeTier.claimed  ? 1 : 0);
        PlayerPrefs.SetInt("SeasonSilver",  silverTier.claimed  ? 1 : 0);
        PlayerPrefs.SetInt("SeasonGold",    goldTier.claimed    ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        playerPoints          = PlayerPrefs.GetInt("SeasonPoints", 0);
        earnedPermanentBonus  = PlayerPrefs.GetFloat("SeasonBonus", 1f);
        bronzeTier.claimed    = PlayerPrefs.GetInt("SeasonBronze", 0) == 1;
        silverTier.claimed    = PlayerPrefs.GetInt("SeasonSilver", 0) == 1;
        goldTier.claimed      = PlayerPrefs.GetInt("SeasonGold",   0) == 1;
    }

    public void ResetForTesting()
    {
        playerPoints = 0; earnedPermanentBonus = 1f;
        bronzeTier.claimed = silverTier.claimed = goldTier.claimed = false;
        foreach (var a in seasonalAchievements) { a.currentValue = 0; a.isUnlocked = false; }
        leaderboard.Clear();
        SaveData();
        initialized = false;
        Debug.Log("[SeasonalEvent] Сброс для теста.");
        Initialize();
    }
}
