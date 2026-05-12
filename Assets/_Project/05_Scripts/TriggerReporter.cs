using UnityEngine;

public class TriggerReporter : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{gameObject.name}] ВХОД в триггерную зону: {other.gameObject.name}");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[{gameObject.name}] ВЫХОД из триггерной зоны: {other.gameObject.name}");
    }
}
