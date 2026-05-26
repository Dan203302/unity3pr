using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DailyRewardSystem", menuName = "Game/Daily Reward System")]
public class DailyRewardSystem : ScriptableObject
{
    [Serializable]
    public class DailyReward
    {
        public int day;
        public RewardType rewardType;
        public int rewardAmount;
        public bool isSpecial;

        [HideInInspector] public bool isClaimed;
        [HideInInspector] public bool isAvailable;
    }

    public enum RewardType { Currency, EnergyRed, EnergyBlue, EnergyGreen, BoostSale, SpecialItem }

    [Header("Награды за 7 дней")]
    public List<DailyReward> weeklyRewards = new List<DailyReward>();

    [Header("Настройки")]
    public int streakBonus = 10;
    public int maxStreakDays = 28;

    public int currentStreak { get; private set; }
    private DateTime lastClaimDate;
    private bool initialized;

    public event Action<DailyReward> OnRewardClaimed;
    public event Action<int>         OnStreakUpdated;

    public void Initialize()
    {
        if (initialized) return;
        LoadData();
        CheckStreak();
        UpdateAvailableRewards();
        initialized = true;
        Debug.Log($"[DailyReward] Инициализирован. Серия: {currentStreak} дней.");
    }

    private void CheckStreak()
    {
        if (lastClaimDate == default) return;
        double hours = (DateTime.Now - lastClaimDate).TotalHours;
        if (hours > 48) { currentStreak = 0; Debug.Log("[DailyReward] Серия сброшена (прошло >48ч)"); }
        SaveData();
    }

    private void UpdateAvailableRewards()
    {
        int targetDay = (currentStreak % 7) + 1;
        foreach (var r in weeklyRewards)
        {
            r.isAvailable = r.day == targetDay && !r.isClaimed;
            if (r.day < targetDay) r.isClaimed = false;
        }
    }

    public bool ClaimReward(int day)
    {
        if (day < 1 || day > weeklyRewards.Count) return false;
        var reward = weeklyRewards[day - 1];
        if (!reward.isAvailable || reward.isClaimed)
        {
            Debug.LogWarning($"[DailyReward] День {day} недоступен.");
            return false;
        }

        int finalAmount = Mathf.RoundToInt(reward.rewardAmount * (1f + currentStreak * streakBonus / 100f));

        switch (reward.rewardType)
        {
            case RewardType.EnergyRed:
                Debug.Log($"[DailyReward] +{finalAmount} Хайп-энергии (Красная)");
                var es = FindEnergySystem();
                es?.CollectEnergy(EnergySystem.EnergyType.Red, finalAmount);
                break;
            case RewardType.EnergyBlue:
                Debug.Log($"[DailyReward] +{finalAmount} Качество-энергии (Синяя)");
                FindEnergySystem()?.CollectEnergy(EnergySystem.EnergyType.Blue, finalAmount);
                break;
            case RewardType.EnergyGreen:
                Debug.Log($"[DailyReward] +{finalAmount} Редкость-энергии (Зелёная)");
                FindEnergySystem()?.CollectEnergy(EnergySystem.EnergyType.Green, finalAmount);
                break;
            case RewardType.Currency:
                Debug.Log($"[DailyReward] +{finalAmount} монет!");
                break;
            case RewardType.BoostSale:
                Debug.Log($"[DailyReward] Буст продаж x{finalAmount} на 60 сек!");
                break;
            case RewardType.SpecialItem:
                Debug.Log($"[DailyReward] ОСОБЫЙ ПРИЗ: редкий кроссовок!");
                break;
        }

        reward.isClaimed = true;
        reward.isAvailable = false;
        currentStreak++;
        lastClaimDate = DateTime.Now;
        if (currentStreak > maxStreakDays) currentStreak = 1;

        OnRewardClaimed?.Invoke(reward);
        OnStreakUpdated?.Invoke(currentStreak);

        SaveData();
        UpdateAvailableRewards();

        Debug.Log($"[DailyReward] День {day} получен. Серия: {currentStreak}");
        return true;
    }

    private EnergySystem FindEnergySystem()
    {
#if UNITY_EDITOR
        return UnityEngine.Object.FindFirstObjectByType<EnergySystem>();
#else
        return UnityEngine.Object.FindFirstObjectByType<EnergySystem>();
#endif
    }

    public StreakInfo GetStreakInfo()
    {
        DateTime nextReset = DateTime.Now.Date.AddDays(1);
        return new StreakInfo
        {
            currentStreak = currentStreak,
            maxStreak = maxStreakDays,
            nextResetTime = nextReset,
            streakBonus = streakBonus,
            bonusMultiplier = 1f + currentStreak * streakBonus / 100f,
            daysUntilSpecialReward = 7 - (currentStreak % 7)
        };
    }

    public string GetFormattedTimeUntilReset()
    {
        var ts = DateTime.Now.Date.AddDays(1) - DateTime.Now;
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public bool HasAvailableReward()
    {
        foreach (var r in weeklyRewards)
            if (r.isAvailable && !r.isClaimed) return true;
        return false;
    }

    private void SaveData()
    {
        var data = new RewardSaveData { currentStreak = currentStreak, lastClaimDate = lastClaimDate.ToString("o") };
        foreach (var r in weeklyRewards) data.claimed.Add(r.isClaimed);
        PlayerPrefs.SetString("DailyRewardsData", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (!PlayerPrefs.HasKey("DailyRewardsData")) return;
        var data = JsonUtility.FromJson<RewardSaveData>(PlayerPrefs.GetString("DailyRewardsData"));
        currentStreak = data.currentStreak;
        if (DateTime.TryParse(data.lastClaimDate, out var d)) lastClaimDate = d;
        for (int i = 0; i < Mathf.Min(data.claimed.Count, weeklyRewards.Count); i++)
            weeklyRewards[i].isClaimed = data.claimed[i];
    }

    public void ResetForTesting()
    {
        PlayerPrefs.DeleteKey("DailyRewardsData");
        currentStreak = 0;
        lastClaimDate = default;
        initialized = false;
        foreach (var r in weeklyRewards) { r.isClaimed = false; r.isAvailable = false; }
        Initialize();
        Debug.Log("[DailyReward] Данные сброшены для теста.");
    }

    [Serializable] private class RewardSaveData { public int currentStreak; public string lastClaimDate; public List<bool> claimed = new(); }

    [Serializable]
    public class StreakInfo
    {
        public int currentStreak;
        public int maxStreak;
        public DateTime nextResetTime;
        public int streakBonus;
        public float bonusMultiplier;
        public int daysUntilSpecialReward;
    }
}
