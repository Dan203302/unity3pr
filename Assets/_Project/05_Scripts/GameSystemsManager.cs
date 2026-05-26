using UnityEngine;
using UnityEngine.InputSystem;

public class GameSystemsManager : MonoBehaviour
{
    [Header("Ссылки на системы")]
    [SerializeField] public AchievementSystem  achievementSystem;
    [SerializeField] public DailyRewardSystem  dailyRewardSystem;
    [SerializeField] public SeasonalEventSystem seasonalEventSystem;

    [Header("Настройки")]
    [SerializeField] private bool enableAchievements = true;
    [SerializeField] private bool enableDailyRewards  = true;
    [SerializeField] private bool showDebugLogs       = true;

    public static GameSystemsManager Instance { get; private set; }

    private float playTime;

    // ── Клавиши (описание выводится в консоль при старте) ────────────────────
    // F1  — Список всех ачивок и их прогресс
    // F2  — Получить ежедневную награду
    // F3  — Сбросить ежедневные награды (тест)
    // F4  — Вывести всю статистику
    // F5  — Разблокировать следующую незакрытую ачивку
    // F6  — +50 очков сезонного события
    // F7  — Сбросить сезонное событие (тест)
    // F8  — Симулировать продажу на $100
    // F9  — Симулировать комбо x3

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (enableAchievements && achievementSystem != null)
        {
            achievementSystem.Initialize();
            achievementSystem.OnAchievementUnlocked += a =>
                Debug.Log($"[GameSystems] ★ АЧИВКА: «{a.title}» +{a.rewardXP}XP");
            Log("Achievement system OK");
        }

        if (enableDailyRewards && dailyRewardSystem != null)
        {
            dailyRewardSystem.Initialize();
            dailyRewardSystem.OnRewardClaimed += r => Log($"Ежедневная награда: День {r.day} ({r.rewardType})");
            dailyRewardSystem.OnStreakUpdated  += s => Log($"Серия: {s} дней");
            if (dailyRewardSystem.HasAvailableReward())
                Debug.Log("[GameSystems] Доступна ежедневная награда! [F2]");
        }

        if (seasonalEventSystem != null)
        {
            seasonalEventSystem.Initialize();
            seasonalEventSystem.OnTierReached += (pts, tier) =>
                Debug.Log($"[GameSystems] УРОВЕНЬ «{tier.tierName}»! Награда: {tier.reward}");
        }

        AbilitySystem.OnAbilityActivated     += (name, cd) => GameEvents.AbilityUsed(name);
        AbilitySystem.OnComboTriggered        += (chain, b) => { GameEvents.ComboChain(chain); GameEvents.UpdateMaxCombo(chain); };
        SneakerComboMechanic.OnSaleCompleted  += (price, total, count) => GameEvents.SaleCompleted((int)price);

        Debug.Log("[GameSystems] Debug: F1=Ачивки F2=Награда F3=СбросНаград F4=Стат F5=НаследующаяАчивка F6=+Очки F7=СбросСезон F8=Продажа F9=Комбо");
    }

    void Update()
    {
        playTime += Time.deltaTime;
        if (playTime % 60 < Time.deltaTime)
            GameEvents.UpdatePlayTime(playTime);

        HandleDebugInput();
    }

    void HandleDebugInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // F1 — список ачивок
        if (kb.f1Key.wasPressedThisFrame) PrintAchievements();

        // F2 — получить ежедневную награду
        if (kb.f2Key.wasPressedThisFrame)
        {
            if (dailyRewardSystem != null)
            {
                int day = (dailyRewardSystem.currentStreak % 7) + 1;
                bool ok = dailyRewardSystem.ClaimReward(day);
                if (!ok) Debug.Log("[Debug F2] Награда уже получена или недоступна.");
            }
        }

        // F3 — сброс ежедневных наград
        if (kb.f3Key.wasPressedThisFrame)
        {
            dailyRewardSystem?.ResetForTesting();
            Debug.Log("[Debug F3] Ежедневные награды сброшены.");
        }

        // F4 — полная статистика
        if (kb.f4Key.wasPressedThisFrame) PrintStats();

        // F5 — разблокировать следующую незакрытую ачивку
        if (kb.f5Key.wasPressedThisFrame) UnlockNextAchievement();

        // F6 — +50 очков сезона
        if (kb.f6Key.wasPressedThisFrame)
        {
            seasonalEventSystem?.AddPoints(50, "Debug F6");
            Debug.Log("[Debug F6] +50 очков сезона. " + seasonalEventSystem?.GetProgressString());
        }

        // F7 — сброс сезона
        if (kb.f7Key.wasPressedThisFrame)
        {
            seasonalEventSystem?.ResetForTesting();
            Debug.Log("[Debug F7] Сезонное событие сброшено.");
        }

        // F8 — симуляция продажи $100
        if (kb.f8Key.wasPressedThisFrame)
        {
            GameEvents.SaleCompleted(100);
            Debug.Log("[Debug F8] Симулирована продажа $100.");
        }

        // F9 — симуляция комбо x3
        if (kb.f9Key.wasPressedThisFrame)
        {
            GameEvents.ComboChain(3);
            GameEvents.UpdateMaxCombo(3);
            Debug.Log("[Debug F9] Симулировано комбо x3.");
        }
    }

    void PrintAchievements()
    {
        if (achievementSystem == null) return;
        Debug.Log("─── АЧИВКИ ─────────────────────────────────");
        foreach (var a in achievementSystem.achievements)
        {
            string status = a.isUnlocked
                ? $"✓ ({a.unlockTimeStr})"
                : $"{a.currentValue:F0}/{a.targetValue:F0}";
            Debug.Log($"  [{(a.isUnlocked ? "✓" : " ")}] {a.title} — {status}");
        }
    }

    void PrintStats()
    {
        Debug.Log("─── СТАТИСТИКА ──────────────────────────────");
        if (achievementSystem != null)
        {
            var s = achievementSystem.GetStats();
            Debug.Log($"  Ачивки: {s.unlockedAchievements}/{s.totalAchievements} ({s.completionPercentage:F0}%) | XP: {s.totalRewardXP} | Монет: {s.totalRewardCurrency}");
        }
        if (dailyRewardSystem != null)
        {
            var i = dailyRewardSystem.GetStreakInfo();
            Debug.Log($"  Серия: {i.currentStreak} дней | Бонус: x{i.bonusMultiplier:F2} | До спец: {i.daysUntilSpecialReward} дн. | Сброс: {dailyRewardSystem.GetFormattedTimeUntilReset()}");
        }
        if (seasonalEventSystem != null)
            Debug.Log($"  Сезон: {seasonalEventSystem.GetProgressString()}");

        Debug.Log($"  Время в игре: {playTime:F0}с");
    }

    void UnlockNextAchievement()
    {
        if (achievementSystem == null) return;
        foreach (var a in achievementSystem.achievements)
        {
            if (!a.isUnlocked)
            {
                achievementSystem.UpdateAchievement(a.type, a.targetValue);
                Debug.Log($"[Debug F5] Форсирована ачивка: «{a.title}»");
                return;
            }
        }
        Debug.Log("[Debug F5] Все ачивки уже разблокированы!");
    }

    private void Log(string msg) { if (showDebugLogs) Debug.Log($"[GameSystems] {msg}"); }
}
