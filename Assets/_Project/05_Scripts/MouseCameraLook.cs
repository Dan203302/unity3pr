using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCameraLook : MonoBehaviour
{
    public float lookSpeed = 2f;
    public float maxVerticalAngle = 80f;

    private float verticalRotation = 0f;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float mouseX = mouse.delta.x.ReadValue() * lookSpeed;
        float mouseY = mouse.delta.y.ReadValue() * lookSpeed;

        // Rotate parent (Player) horizontally
        transform.parent.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
