using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AchievementSystem", menuName = "Game/Achievement System")]
public class AchievementSystem : ScriptableObject
{
    [Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;

        [Header("Требования")]
        public AchievementType type;
        public float targetValue;
        public int rewardXP;
        public int rewardCurrency;

        [Header("Состояние")]
        public float currentValue;
        public bool isUnlocked;
        public bool isHidden;
        public string unlockTimeStr;
    }

    public enum AchievementType
    {
        TotalSales,
        MaxCombo,
        PlayTime,
        AbilityUsed,
        UltimateDeal,
        ComboChain,
        TotalEarned
    }

    [Header("Достижения")]
    public List<Achievement> achievements = new List<Achievement>();

    [Header("Настройки")]
    public bool autoSave = true;
    public string saveKey = "AchievementsData";

    public event Action<Achievement> OnAchievementUnlocked;

    public void Initialize()
    {
        LoadAchievements();

        GameEvents.OnSaleCompleted    += amount => UpdateAchievement(AchievementType.TotalSales, 1);
        GameEvents.OnSaleCompleted    += amount => UpdateAchievement(AchievementType.TotalEarned, amount);
        GameEvents.OnAbilityUsed      += type   => UpdateAchievement(AchievementType.AbilityUsed, 1);
        GameEvents.OnAbilityUsed      += type   => { if (type == "Ультимативная Сделка") UpdateAchievement(AchievementType.UltimateDeal, 1); };
        GameEvents.OnMaxComboUpdated  += combo  => UpdateAchievement(AchievementType.MaxCombo, combo);
        GameEvents.OnComboChain       += chain  => UpdateAchievement(AchievementType.ComboChain, chain);
        GameEvents.OnPlayTimeUpdated  += t      => UpdateAchievement(AchievementType.PlayTime, t);
    }

    public void UpdateAchievement(AchievementType type, float increment)
    {
        foreach (var a in achievements)
        {
            if (a.type != type || a.isUnlocked) continue;

            bool isMax = type == AchievementType.MaxCombo;
            if (isMax)
                a.currentValue = Mathf.Max(a.currentValue, increment);
            else
                a.currentValue += increment;

            if (a.currentValue >= a.targetValue)
                UnlockAchievement(a);

            if (autoSave) SaveAchievements();
            break;
        }
    }

    private void UnlockAchievement(Achievement a)
    {
        a.isUnlocked = true;
        a.unlockTimeStr = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        OnAchievementUnlocked?.Invoke(a);
        Debug.Log($"[Achievement] РАЗБЛОКИРОВАНО: {a.title} | +{a.rewardXP}XP +{a.rewardCurrency}монет");
    }

    public AchievementStats GetStats()
    {
        int total = achievements.Count, unlocked = 0, xp = 0, cur = 0;
        foreach (var a in achievements)
            if (a.isUnlocked) { unlocked++; xp += a.rewardXP; cur += a.rewardCurrency; }
        return new AchievementStats
        {
            totalAchievements = total,
            unlockedAchievements = unlocked,
            completionPercentage = total > 0 ? (float)unlocked / total * 100f : 0f,
            totalRewardXP = xp,
            totalRewardCurrency = cur
        };
    }

    public void SaveAchievements()
    {
        var data = new SaveData();
        foreach (var a in achievements)
            data.entries.Add(new SaveEntry { id = a.id, currentValue = a.currentValue, isUnlocked = a.isUnlocked });
        PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void LoadAchievements()
    {
        if (!PlayerPrefs.HasKey(saveKey)) return;
        var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveKey));
        foreach (var a in achievements)
            foreach (var e in data.entries)
                if (e.id == a.id) { a.currentValue = e.currentValue; a.isUnlocked = e.isUnlocked; break; }
    }

    [Serializable] private class SaveData { public List<SaveEntry> entries = new(); }
    [Serializable] private class SaveEntry { public string id; public float currentValue; public bool isUnlocked; }

    [Serializable]
    public class AchievementStats
    {
        public int totalAchievements;
        public int unlockedAchievements;
        public float completionPercentage;
        public int totalRewardXP;
        public int totalRewardCurrency;
    }
}
