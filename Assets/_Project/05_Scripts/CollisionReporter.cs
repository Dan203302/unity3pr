using UnityEngine;

public class CollisionReporter : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning($"[{gameObject.name}] столкнулся с: {collision.gameObject.name} | " +
                         $"Сила удара: {collision.impulse.magnitude:F2}");
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log($"[{gameObject.name}] разделился с: {collision.gameObject.name}");
    }
}
