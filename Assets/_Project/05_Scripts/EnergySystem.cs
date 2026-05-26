using UnityEngine;
using System.Collections.Generic;
using System;

public class EnergySystem : MonoBehaviour
{
    [SerializeField] private BalanceConfig balanceConfig;

    public enum EnergyType { Red, Blue, Green }

    [Serializable]
    public class EnergyStorage
    {
        public EnergyType type;
        public int currentAmount;
        public int maxCapacity;
        public float decayTimer;

        public bool CanAdd(int amount) => currentAmount + amount <= maxCapacity;

        public int Add(int amount)
        {
            int spaceLeft = maxCapacity - currentAmount;
            int added = Mathf.Min(amount, spaceLeft);
            currentAmount += added;
            return added;
        }

        public bool TryConsume(int amount)
        {
            if (currentAmount >= amount) { currentAmount -= amount; return true; }
            return false;
        }
    }

    public List<EnergyStorage> energyStorages = new List<EnergyStorage>();
    private Dictionary<EnergyType, EnergyStorage> energyDict = new Dictionary<EnergyType, EnergyStorage>();

    public static event Action<EnergyType, int> OnEnergyChanged;

    void Start()
    {
        InitializeEnergyTypes();
    }

    void Update()
    {
        HandleEnergyDecay();
    }

    void InitializeEnergyTypes()
    {
        foreach (EnergyType type in Enum.GetValues(typeof(EnergyType)))
        {
            var storage = new EnergyStorage
            {
                type = type,
                currentAmount = 0,
                maxCapacity = balanceConfig != null ? balanceConfig.baseEnergyPerCollect * 10 : 100
            };
            energyStorages.Add(storage);
            energyDict[type] = storage;
        }
    }

    // Вариант А: деградация энергии
    void HandleEnergyDecay()
    {
        if (balanceConfig == null) return;
        foreach (var storage in energyStorages)
        {
            if (storage.currentAmount <= 0) continue;
            storage.decayTimer += Time.deltaTime;
            if (storage.decayTimer >= balanceConfig.decayDelay)
            {
                int decay = Mathf.CeilToInt(balanceConfig.energyDecayRate * Time.deltaTime);
                storage.currentAmount = Mathf.Max(0, storage.currentAmount - decay);
                OnEnergyChanged?.Invoke(storage.type, storage.currentAmount);
            }
        }
    }

    public bool CollectEnergy(EnergyType type, int amount)
    {
        if (!energyDict.TryGetValue(type, out var storage)) return false;
        int collected = storage.Add(amount);
        if (collected <= 0) return false;
        storage.decayTimer = 0f;
        Debug.Log($"[EnergySystem] Собрано {collected} {type} энергии. Итого: {storage.currentAmount}");
        OnEnergyChanged?.Invoke(type, storage.currentAmount);
        return true;
    }

    public bool HasEnoughEnergy(EnergyType type, int amount)
        => energyDict.ContainsKey(type) && energyDict[type].currentAmount >= amount;

    public bool ConsumeEnergy(EnergyType type, int amount)
    {
        if (!HasEnoughEnergy(type, amount))
        {
            Debug.LogWarning($"[EnergySystem] Недостаточно {type} энергии: нужно {amount}");
            return false;
        }
        energyDict[type].TryConsume(amount);
        energyDict[type].decayTimer = 0f;
        OnEnergyChanged?.Invoke(type, energyDict[type].currentAmount);
        return true;
    }

    public int GetEnergy(EnergyType type)
        => energyDict.TryGetValue(type, out var s) ? s.currentAmount : 0;

    public void AutoBalanceCapacity(int playerLevel)
    {
        if (balanceConfig == null) return;
        float multiplier = balanceConfig.progressionCurve.Evaluate(playerLevel / 100f);
        foreach (var storage in energyStorages)
            storage.maxCapacity = Mathf.RoundToInt(balanceConfig.baseEnergyPerCollect * 10 * multiplier);
    }
}
