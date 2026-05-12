using UnityEngine;
using UnityEngine.InputSystem;

public class CoordinateLogger : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            Vector3 pos = transform.position;
            Debug.Log($"[{gameObject.name}] Координаты: X={pos.x:F2} Y={pos.y:F2} Z={pos.z:F2}");
        }
    }
}
