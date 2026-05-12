using UnityEngine;

public class StartLogger : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"[{gameObject.name}] готов к работе. Позиция: {transform.position}");
    }
}
