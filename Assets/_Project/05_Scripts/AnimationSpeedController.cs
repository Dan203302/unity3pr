using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationSpeedController : MonoBehaviour
{
    private Animator animator;

    private Vector2 moveInput;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Transform body = transform.Find("AnimatedBody");
            if (body != null) animator = body.GetComponent<Animator>();
        }
        if (animator == null || animator.runtimeAnimatorController == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            moveInput = Vector2.zero;
            if (kb.wKey.isPressed || kb.sKey.isPressed) moveInput.y = 1f;
            if (kb.aKey.isPressed || kb.dKey.isPressed) moveInput.x = 1f;
            if (kb.leftShiftKey.isPressed) moveInput *= 2f;
        }

        float speed = moveInput.magnitude;
        if (animator != null) animator.SetFloat("Speed", speed);
    }
}
