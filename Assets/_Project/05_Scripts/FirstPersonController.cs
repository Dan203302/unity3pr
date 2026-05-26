using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float maxLookAngle = 90f;

    private CharacterController characterController;
    private Camera playerCamera;

    private Vector3 moveDirection;
    private float verticalRotation = 0f;
    private float currentSpeed;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();

        if (!characterController.isGrounded)
            moveDirection.y += gravity * Time.deltaTime;
        else if (moveDirection.y < 0)
            moveDirection.y = -2f;

        characterController.Move(moveDirection * Time.deltaTime);

        // Unlock cursor with Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float horizontal = 0f;
        float vertical = 0f;

        if (kb.aKey.isPressed) horizontal = -1f;
        if (kb.dKey.isPressed) horizontal = 1f;
        if (kb.wKey.isPressed) vertical = 1f;
        if (kb.sKey.isPressed) vertical = -1f;

        currentSpeed = kb.leftShiftKey.isPressed ? runSpeed : walkSpeed;

        Vector3 forward = transform.forward * vertical;
        Vector3 right = transform.right * horizontal;
        Vector3 desiredMove = (forward + right).normalized * currentSpeed;

        moveDirection.x = desiredMove.x;
        moveDirection.z = desiredMove.z;

        if (characterController.isGrounded && kb.spaceKey.wasPressedThisFrame)
            moveDirection.y = jumpForce;
    }

    void HandleMouseLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity;
        float mouseY = mouse.delta.y.ReadValue() * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
