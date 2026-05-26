using UnityEngine;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "Game/Balance Config")]
public class BalanceConfig : ScriptableObject
{
    [Header("=== СБОР ЭНЕРГИИ ===")]
    [Tooltip("Базовое количество энергии за сбор")]
    public int baseEnergyPerCollect = 10;

    [Tooltip("Множитель роста стоимости способностей")]
    public float abilityCostGrowth = 1.2f;

    [Tooltip("Максимальный уровень способности")]
    public int maxAbilityLevel = 10;

    [Header("=== ДЕГРАДАЦИЯ ЭНЕРГИИ (Вариант А) ===")]
    [Tooltip("Энергия теряется в секунду если не используется")]
    public float energyDecayRate = 0.5f;

    [Tooltip("Задержка перед началом деградации (сек)")]
    public float decayDelay = 15f;

    [Header("=== СПОСОБНОСТИ ===")]
    [Tooltip("Стоимость способностей 1 уровня")]
    public int[] abilityBaseCosts = new int[] { 10, 20, 30, 50 };

    [Tooltip("Время перезарядки в секундах")]
    public float[] abilityCooldowns = new float[] { 2f, 3f, 5f, 8f };

    [Header("=== КОМБО (Вариант В) ===")]
    [Tooltip("Базовый бонус комбо")]
    public float baseComboBonus = 1.5f;

    [Tooltip("Время для выполнения комбо (сек)")]
    public float comboWindow = 3f;

    [Header("=== КРИВЫЕ БАЛАНСА ===")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 1, 1, 3);
    public AnimationCurve rewardCurve = AnimationCurve.EaseInOut(0, 1, 1, 5);
    public AnimationCurve progressionCurve = AnimationCurve.Linear(0, 1, 1, 2);

    public int CalculateAbilityCost(int abilityIndex, int level)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityBaseCosts.Length) return 0;
        return Mathf.RoundToInt(abilityBaseCosts[abilityIndex] * Mathf.Pow(abilityCostGrowth, level - 1));
    }

    public float GetCooldown(int abilityIndex, int level)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityCooldowns.Length) return 1f;
        return abilityCooldowns[abilityIndex] * (1f - (level * 0.05f));
    }

    // Формула бонуса комбо: Бонус = BaseBonusCombo × (1 + N × 0.3)
    public float CalculateComboBonus(int chainCount)
    {
        return baseComboBonus * (1f + chainCount * 0.3f);
    }

    // Эффективность комбо: Бонус / Время
    public float CalculateComboEfficiency(int chainCount)
    {
        float bonus = CalculateComboBonus(chainCount);
        float time = 2f + chainCount * 0.5f;
        return bonus / time;
    }
}
