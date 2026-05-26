using System;

public static class GameEvents
{
    public static event Action<int>    OnSaleCompleted;
    public static event Action<string> OnAbilityUsed;
    public static event Action<float>  OnPlayTimeUpdated;
    public static event Action<int>    OnMaxComboUpdated;
    public static event Action<int>    OnComboChain;

    public static void SaleCompleted(int amount)      => OnSaleCompleted?.Invoke(amount);
    public static void AbilityUsed(string type)       => OnAbilityUsed?.Invoke(type);
    public static void UpdatePlayTime(float t)        => OnPlayTimeUpdated?.Invoke(t);
    public static void UpdateMaxCombo(int combo)      => OnMaxComboUpdated?.Invoke(combo);
    public static void ComboChain(int chain)          => OnComboChain?.Invoke(chain);
}
