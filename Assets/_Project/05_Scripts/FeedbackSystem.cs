using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI salesText;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip abilitySound;
    [SerializeField] private AudioClip comboSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnEnable()
    {
        EnergySystem.OnEnergyChanged += HandleEnergyChanged;
        AbilitySystem.OnAbilityActivated += HandleAbilityActivated;
        AbilitySystem.OnComboTriggered += HandleComboTriggered;
        SneakerComboMechanic.OnSaleCompleted += HandleSaleCompleted;
    }

    void OnDisable()
    {
        EnergySystem.OnEnergyChanged -= HandleEnergyChanged;
        AbilitySystem.OnAbilityActivated -= HandleAbilityActivated;
        AbilitySystem.OnComboTriggered -= HandleComboTriggered;
        SneakerComboMechanic.OnSaleCompleted -= HandleSaleCompleted;
    }

    private void HandleEnergyChanged(EnergySystem.EnergyType type, int amount)
    {
        if (energyText == null) return;

        string colorTag = type switch
        {
            EnergySystem.EnergyType.Red   => "<color=#FF4444>",
            EnergySystem.EnergyType.Blue  => "<color=#4488FF>",
            EnergySystem.EnergyType.Green => "<color=#44FF88>",
            _ => "<color=white>"
        };

        // Обновляем нужную строку в тексте
        UpdateEnergyLine(type, amount, colorTag);

        if (collectSound != null) audioSource.PlayOneShot(collectSound);
    }

    private void UpdateEnergyLine(EnergySystem.EnergyType type, int amount, string colorTag)
    {
        if (energyText == null) return;

        string typeName = type switch
        {
            EnergySystem.EnergyType.Red   => "Хайп (Красная)",
            EnergySystem.EnergyType.Blue  => "Качество (Синяя)",
            EnergySystem.EnergyType.Green => "Редкость (Зелёная)",
            _ => type.ToString()
        };

        // Простое обновление — перестраиваем весь текст через EnergySystem
        var es = FindFirstObjectByType<EnergySystem>();
        if (es == null) return;

        string display = "";
        foreach (EnergySystem.EnergyType t in System.Enum.GetValues(typeof(EnergySystem.EnergyType)))
        {
            string c = t switch
            {
                EnergySystem.EnergyType.Red   => "<color=#FF4444>",
                EnergySystem.EnergyType.Blue  => "<color=#4488FF>",
                EnergySystem.EnergyType.Green => "<color=#44FF88>",
                _ => "<color=white>"
            };
            string n = t switch
            {
                EnergySystem.EnergyType.Red   => "Хайп",
                EnergySystem.EnergyType.Blue  => "Качество",
                EnergySystem.EnergyType.Green => "Редкость",
                _ => t.ToString()
            };
            display += $"{c}{n}: {es.GetEnergy(t)}</color>\n";
        }
        energyText.text = display.TrimEnd();
    }

    private void HandleAbilityActivated(string abilityName, float cooldown)
    {
        if (abilityText != null)
        {
            abilityText.text = $"<color=yellow>{abilityName}</color>\nКД: {cooldown:F1}с";
            StartCoroutine(FlashText(abilityText, Color.yellow, Color.white));
        }
        if (abilitySound != null) audioSource.PlayOneShot(abilitySound);
    }

    private void HandleComboTriggered(int chainCount, float bonus)
    {
        if (comboText != null)
        {
            comboText.text = $"<color=orange>КОМБО x{chainCount}!\nБонус: {bonus:F1}x</color>";
            StartCoroutine(FlashText(comboText, Color.yellow, Color.clear, 2f));
        }
        if (comboSound != null) audioSource.PlayOneShot(comboSound);
        Debug.Log($"[FeedbackSystem] КОМБО x{chainCount} — бонус {bonus:F2}x!");
    }

    private IEnumerator FlashText(TextMeshProUGUI text, Color flashColor, Color returnColor, float duration = 0.5f)
    {
        text.color = flashColor;
        yield return new WaitForSeconds(duration);
        text.color = returnColor;
    }

    private void HandleSaleCompleted(float price, float total, int count)
    {
        if (salesText != null)
            salesText.text = $"<color=cyan>Продажа: ${price:F0}</color>\nИтого: ${total:F0}\nПродаж: {count}";
    }

    public void ShowWorldFeedback(Vector3 worldPos, string message, Color color)
    {
        Debug.Log($"[Feedback] {message} @ {worldPos:F1}");
    }
}
