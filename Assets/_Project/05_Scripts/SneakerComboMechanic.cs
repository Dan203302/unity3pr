using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Часть 4: Кастомная механика для магазина кроссовок.
/// 
/// ЯДРО МЕХАНИКИ: Игрок собирает энергию трёх типов (Хайп, Качество, Редкость)
/// взаимодействуя с кроссовками. Накопленная энергия активирует торговые способности.
/// 
/// ЦИКЛ ИГРЫ:
///   Осмотр кроссовок → Сбор энергии → Активация способности → Продажа → Награда
/// 
/// ВАРИАНТ А: Деградация — энергия теряется со временем если её не использовать.
/// ВАРИАНТ В: Цепная реакция — комбо из способностей даёт множитель к продаже.
/// 
/// ВХОДНЫЕ ДАННЫЕ:
///   - Тип кроссовка (влияет на тип собираемой энергии)
///   - Нажатия клавиш 1/2/3/4 для активации способностей
/// 
/// ВЫХОДНЫЕ ДАННЫЕ:
///   - Бонус к цене продажи
///   - Анимация NPC покупки
///   - Очки продаж
/// </summary>
public class SneakerComboMechanic : MonoBehaviour
{
    [Header("Зависимости")]
    [SerializeField] private EnergySystem energySystem;
    [SerializeField] private AbilitySystem abilitySystem;
    [SerializeField] private BalanceConfig balanceConfig;

    [Header("Настройки механики")]
    [SerializeField] private float saleBasePriceMin = 50f;
    [SerializeField] private float saleBasePriceMax = 200f;
    [SerializeField] private float hypeSaleBonus    = 0.5f;
    [SerializeField] private float pitchSaleChance  = 0.8f;
    [SerializeField] private float ultimatePriceMultiplier = 3f;

    [Header("Сбор энергии — радиус и коллайдеры")]
    [SerializeField] private float collectRadius = 2f;

    private float totalEarned;
    private int salesCount;
    private float currentSaleMultiplier = 1f;
    private bool pitchActive;

    public static event System.Action<float, float, int> OnSaleCompleted;

    void OnEnable()
    {
        AbilitySystem.OnAbilityActivated += HandleAbilityActivated;
        AbilitySystem.OnComboTriggered   += HandleComboTriggered;
    }

    void OnDisable()
    {
        AbilitySystem.OnAbilityActivated -= HandleAbilityActivated;
        AbilitySystem.OnComboTriggered   -= HandleComboTriggered;
    }

    void Update()
    {
        TryAutoCollectEnergy();

        var kb = Keyboard.current;
        if (kb == null) return;

        // Tab — добавить тестовую энергию всех типов
        if (kb.tabKey.wasPressedThisFrame)
        {
            int amount = balanceConfig?.baseEnergyPerCollect ?? 10;
            energySystem.CollectEnergy(EnergySystem.EnergyType.Red,   amount * 3);
            energySystem.CollectEnergy(EnergySystem.EnergyType.Blue,  amount * 3);
            energySystem.CollectEnergy(EnergySystem.EnergyType.Green, amount * 3);
            Debug.Log("[SneakerCombo] Тест: добавлена энергия всех типов (Tab)");
        }

        // F — продажа
        if (kb.fKey.wasPressedThisFrame) TrySell();
    }

    void TryAutoCollectEnergy()
    {
        if (energySystem == null) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, collectRadius);
        foreach (var hit in hits)
        {
            var item = hit.GetComponent<SneakerItem>();
            if (item != null && !item.collected)
            {
                energySystem.CollectEnergy(item.energyType, balanceConfig?.baseEnergyPerCollect ?? 10);
                item.collected = true;
                Debug.Log($"[SneakerCombo] Собрана энергия {item.energyType} с {hit.name}");
                Destroy(hit.gameObject, 0.1f);
            }
        }
    }

    void TrySell()
    {
        float finalPrice = Random.Range(saleBasePriceMin, saleBasePriceMax) * currentSaleMultiplier;

        bool sold = pitchActive || Random.value < 0.5f;

        if (sold)
        {
            totalEarned += finalPrice;
            salesCount++;
            Debug.Log($"[SneakerCombo] ПРОДАЖА! Цена: ${finalPrice:F0} | Итого: ${totalEarned:F0} | Продаж: {salesCount}");
            OnSaleCompleted?.Invoke(finalPrice, totalEarned, salesCount);
            currentSaleMultiplier = 1f;
            pitchActive = false;
        }
        else
        {
            Debug.Log("[SneakerCombo] Клиент отказался. Используй Качественный Питч!");
        }
    }

    CustomerNPC FindWaitingNPC()
    {
        foreach (var npc in Object.FindObjectsByType<CustomerNPC>(FindObjectsSortMode.None))
            if (npc.IsWaitingAtCashier) return npc;
        return null;
    }

    void GiveBoxToNPC(CustomerNPC npc, float priceMultiplier)
    {
        if (npc == null) return;
        currentSaleMultiplier = priceMultiplier;

        // Ищем ближайший физический объект как «коробку»
        GameObject box = null;
        var spawner = Object.FindFirstObjectByType<ObjectSpawner>();
        if (spawner != null && spawner.prefabToSpawn != null)
        {
            box = Object.Instantiate(spawner.prefabToSpawn,
                npc.transform.position + Vector3.up * 1.2f,
                Quaternion.identity);
        }

        npc.ReceiveBox(box);
        TrySell();
    }

    private void HandleAbilityActivated(string abilityName, float cooldown)
    {
        var npc = FindWaitingNPC();

        switch (abilityName)
        {
            case "Хайп-Продажа":
                if (npc != null)
                {
                    Debug.Log($"[SneakerCombo] Хайп! Продаём с бонусом x{1f + hypeSaleBonus:F1}");
                    GiveBoxToNPC(npc, 1f + hypeSaleBonus);
                }
                else
                {
                    currentSaleMultiplier = 1f + hypeSaleBonus;
                    Debug.Log("[SneakerCombo] Хайп активирован (NPC не ждёт)");
                }
                break;

            case "Качественный Питч":
                pitchActive = true;
                if (npc != null)
                {
                    Debug.Log("[SneakerCombo] Питч! Гарантированная продажа.");
                    GiveBoxToNPC(npc, 1f);
                }
                else Debug.Log("[SneakerCombo] Питч активирован (NPC не ждёт)");
                break;

            case "Редкая Находка":
                SpawnRareSneaker();
                break;

            case "Ультимативная Сделка":
                if (npc != null)
                {
                    pitchActive = true;
                    Debug.Log($"[SneakerCombo] Ультимат! x{ultimatePriceMultiplier}");
                    GiveBoxToNPC(npc, ultimatePriceMultiplier);
                }
                else Debug.Log("[SneakerCombo] Ультимат — NPC не ждёт у кассы!");
                break;
        }
    }

    private void HandleComboTriggered(int chainCount, float bonus)
    {
        currentSaleMultiplier *= bonus;
        Debug.Log($"[SneakerCombo] Комбо бонус применён! Новый множитель продажи: x{currentSaleMultiplier:F2}");
    }

    void SpawnRareSneaker()
    {
        // Создаём тестовый "редкий кроссовок" рядом с игроком
        var sneaker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sneaker.name = "RareSneaker";
        sneaker.transform.position = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
        sneaker.transform.localScale = Vector3.one * 0.3f;
        sneaker.AddComponent<SneakerItem>().energyType = EnergySystem.EnergyType.Green;

        var rend = sneaker.GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.green;

        Destroy(sneaker, 10f);
        Debug.Log("[SneakerCombo] Редкий кроссовок заспавнен!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}

/// <summary>
/// Компонент-метка на объектах кроссовок для сбора энергии
/// </summary>
public class SneakerItem : MonoBehaviour
{
    public EnergySystem.EnergyType energyType = EnergySystem.EnergyType.Red;
    public bool collected;
}
