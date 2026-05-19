using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -20f;
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
                playerCamera = camObj.GetComponent<Camera>();
        }
    }

    void Update()
    {
        // --- Read WASD directly, no subscriptions, no ghost input ---
        Vector3 input = Vector3.zero;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) input.z += 1f;
            if (keyboard.sKey.isPressed) input.z -= 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
        }

        // Movement relative to camera forward
        if (playerCamera != null)
        {
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 move = forward * input.z + right * input.x;
            controller.Move(move * moveSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 move = transform.forward * input.z + transform.right * input.x;
            controller.Move(move * moveSpeed * Time.deltaTime);
        }

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -0.5f;

        // Jump
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && controller.isGrounded)
            velocity.y = jumpForce;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
