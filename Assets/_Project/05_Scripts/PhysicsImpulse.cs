using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsImpulse : MonoBehaviour
{
    [SerializeField] private float impulseForce = 5f;

    private Rigidbody rb;
    private bool impulseRequested;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log($"[{gameObject.name}] PhysicsImpulse готов. Сила импульса: {impulseForce}");
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            impulseRequested = true;
    }

    void FixedUpdate()
    {
        if (impulseRequested)
        {
            rb.AddForce(Vector3.up * impulseForce, ForceMode.Impulse);
            Debug.Log($"[{gameObject.name}] Применён импульс вверх. Сила: {impulseForce}");
            impulseRequested = false;
        }
    }
}
