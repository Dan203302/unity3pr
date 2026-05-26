using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public float holdDistance = 1.5f;
    public float throwForce = 8f;
    public LayerMask pickupMask = ~0;

    private Camera playerCamera;
    private GameObject heldObject;
    private Rigidbody heldRb;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (heldObject != null)
            UpdateHeldPosition();
    }

    public void PickupOrThrow()
    {
        if (heldObject != null)
        {
            ThrowObject();
        }
        else
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        Vector3 center = transform.position;
        Collider[] cols = Physics.OverlapSphere(center, pickupRange, pickupMask);

        GameObject nearest = null;
        Rigidbody nearestRb = null;
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            if (col.transform.IsChildOf(transform)) continue;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null || rb.isKinematic) continue;
            float dist = Vector3.Distance(center, col.transform.position);
            if (dist < minDist) { minDist = dist; nearest = col.gameObject; nearestRb = rb; }
        }

        if (nearest != null)
        {
            heldObject = nearest;
            heldRb = nearestRb;
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
            heldObject.transform.SetParent(playerCamera != null ? playerCamera.transform : transform);
            heldObject.transform.localPosition = new Vector3(0, -0.2f, holdDistance);
            heldObject.transform.localRotation = Quaternion.identity;
            Debug.Log("[Pickup] Поднят: " + heldObject.name);
        }
        else
        {
            Debug.Log("[Pickup] Нет объектов с Rigidbody в радиусе " + pickupRange);
        }
    }

    void ThrowObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        heldRb.isKinematic = false;
        heldRb.useGravity = true;
        Vector3 throwDir = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        heldRb.AddForce(throwDir * throwForce, ForceMode.Impulse);
        Debug.Log("[Pickup] Брошен объект: " + heldObject.name);
        heldObject = null;
        heldRb = null;
    }

    void UpdateHeldPosition()
    {
        // Smoothly hold in front of camera
        if (heldObject == null) return;
        heldObject.transform.localPosition = Vector3.Lerp(
            heldObject.transform.localPosition,
            new Vector3(0, -0.2f, holdDistance),
            Time.deltaTime * 10f);
    }
}
