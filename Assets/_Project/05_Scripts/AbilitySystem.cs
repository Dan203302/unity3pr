using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class AbilitySystem : MonoBehaviour
{
    [SerializeField] private BalanceConfig balanceConfig;
    [SerializeField] private EnergySystem energySystem;

    [Serializable]
    public class Ability
    {
        public string abilityName;
        public string description;
        public EnergySystem.EnergyType[] requiredEnergies;
        public int[] requiredAmounts;
        public float cooldown;
        public int level = 1;

        [HideInInspector] public float currentCooldown;

        public bool IsReady => currentCooldown <= 0f;
        public float CooldownProgress => currentCooldown > 0 ? currentCooldown / cooldown : 0f;

        public void UpdateCooldown(float deltaTime)
        {
            if (currentCooldown > 0f) currentCooldown -= deltaTime;
        }

        public bool CanActivate(EnergySystem es)
        {
            if (!IsReady) return false;
            for (int i = 0; i < requiredEnergies.Length; i++)
                if (!es.HasEnoughEnergy(requiredEnergies[i], requiredAmounts[i])) return false;
            return true;
        }

        public void Activate(EnergySystem es)
        {
            if (!CanActivate(es)) return;
            for (int i = 0; i < requiredEnergies.Length; i++)
                es.ConsumeEnergy(requiredEnergies[i], requiredAmounts[i]);
            currentCooldown = cooldown;
            Debug.Log($"[AbilitySystem] Активирована: {abilityName} (уровень {level})");
        }

        public void LevelUp() { level++; cooldown *= 0.95f; }
    }

    public List<Ability> abilities = new List<Ability>();

    // Вариант В: цепная реакция
    private List<int> comboChain = new List<int>();
    private float comboTimer;

    public static event Action<string, float> OnAbilityActivated;
    public static event Action<int, float> OnComboTriggered;

    void Awake()
    {
        if (energySystem == null)
            energySystem = GetComponent<EnergySystem>();
    }

    void Start()
    {
        if (balanceConfig != null) CreateBalancedAbilities();
    }

    void Update()
    {
        foreach (var a in abilities) a.UpdateCooldown(Time.deltaTime);
        UpdateComboTimer();
        TestInput();
    }

    void UpdateComboTimer()
    {
        if (comboChain.Count > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                Debug.Log("[AbilitySystem] Окно комбо истекло.");
                comboChain.Clear();
            }
        }
    }

    void TestInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame && abilities.Count > 0) TryActivateAbility(0);
        if (kb.digit2Key.wasPressedThisFrame && abilities.Count > 1) TryActivateAbility(1);
        if (kb.digit3Key.wasPressedThisFrame && abilities.Count > 2) TryActivateAbility(2);
        if (kb.digit4Key.wasPressedThisFrame && abilities.Count > 3) TryActivateAbility(3);
    }

    public void TryActivateAbility(int index)
    {
        if (index >= abilities.Count) return;
        var ability = abilities[index];
        if (!ability.CanActivate(energySystem)) { Debug.LogWarning($"[AbilitySystem] Нельзя активировать: {ability.abilityName}"); return; }

        ability.Activate(energySystem);
        OnAbilityActivated?.Invoke(ability.abilityName, ability.cooldown);

        // Комбо-цепь
        comboChain.Add(index);
        comboTimer = balanceConfig != null ? balanceConfig.comboWindow : 3f;

        if (comboChain.Count >= 2)
        {
            float bonus = balanceConfig != null ? balanceConfig.CalculateComboBonus(comboChain.Count) : 1f;
            float efficiency = balanceConfig != null ? balanceConfig.CalculateComboEfficiency(comboChain.Count) : 1f;
            Debug.Log($"[AbilitySystem] КОМБО x{comboChain.Count}! Бонус: {bonus:F2}x, Эффективность: {efficiency:F2}");
            OnComboTriggered?.Invoke(comboChain.Count, bonus);
        }
    }

    public void CreateBalancedAbilities()
    {
        abilities.Clear();

        abilities.Add(new Ability
        {
            abilityName = "Хайп-Продажа",
            description = "Повышает цену продажи на 50%",
            requiredEnergies = new[] { EnergySystem.EnergyType.Red },
            requiredAmounts = new[] { balanceConfig.abilityBaseCosts[0] },
            cooldown = balanceConfig.abilityCooldowns[0]
        });

        abilities.Add(new Ability
        {
            abilityName = "Качественный Питч",
            description = "Убеждает NPC купить",
            requiredEnergies = new[] { EnergySystem.EnergyType.Blue },
            requiredAmounts = new[] { balanceConfig.abilityBaseCosts[1] },
            cooldown = balanceConfig.abilityCooldowns[1]
        });

        abilities.Add(new Ability
        {
            abilityName = "Редкая Находка",
            description = "Спавнит редкий кроссовок",
            requiredEnergies = new[] { EnergySystem.EnergyType.Green },
            requiredAmounts = new[] { balanceConfig.abilityBaseCosts[2] },
            cooldown = balanceConfig.abilityCooldowns[2]
        });

        abilities.Add(new Ability
        {
            abilityName = "Ультимативная Сделка",
            description = "Мгновенная продажа по макс. цене",
            requiredEnergies = new[] { EnergySystem.EnergyType.Red, EnergySystem.EnergyType.Blue, EnergySystem.EnergyType.Green },
            requiredAmounts = new[] { balanceConfig.abilityBaseCosts[0], balanceConfig.abilityBaseCosts[1], 5 },
            cooldown = balanceConfig.abilityBaseCosts.Length > 3 ? balanceConfig.abilityCooldowns[3] : 8f
        });
    }
}
