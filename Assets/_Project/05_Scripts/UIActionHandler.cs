using UnityEngine;
using TMPro;

public class UIActionHandler : MonoBehaviour
{
    public Renderer targetRenderer;
    public TMP_Text statusText;

    private bool isGreen = false;

    public void ChangeColor()
    {
        if (targetRenderer == null)
            return;

        isGreen = !isGreen;
        targetRenderer.material.color = isGreen ? Color.green : Color.white;

        if (statusText != null)
            statusText.text = "Цвет объекта изменён.";
    }

    public void MoveObject()
    {
        // Find PlayerPickup via SendMessage to avoid compile-time type dependency
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            // Try finding by component name
            var allGOs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in allGOs)
                if (mb.GetType().Name == "PlayerPickup") { player = mb.gameObject; break; }
        }

        if (player != null)
        {
            player.SendMessage("PickupOrThrow", SendMessageOptions.DontRequireReceiver);
            if (statusText != null)
                statusText.text = "Объект поднят / брошен.";
        }
        else
        {
            if (statusText != null)
                statusText.text = "PlayerPickup не найден!";
            Debug.LogWarning("[UIActionHandler] PlayerPickup not found. Add it to Player object.");
        }
    }

    public void ShowMessage()
    {
        Debug.Log("Кнопка интерфейса вызвала метод ShowMessage().");

        if (statusText != null)
            statusText.text = "Сообщение выведено в Console.";
    }
}
