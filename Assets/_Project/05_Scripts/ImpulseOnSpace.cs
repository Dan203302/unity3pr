using UnityEngine;

public class ImpulseOnSpace : MonoBehaviour
{
    [SerializeField] private float impulseForce = 5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.AddForce(Vector3.up * impulseForce, ForceMode.Impulse);
            }
        }
    }
}
