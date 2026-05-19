using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationTriggerController : MonoBehaviour
{
    private Animator animator;

    private bool jumpRequested;

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
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
            jumpRequested = true;

        if (jumpRequested)
        {
            if (animator != null) animator.SetTrigger("Jump");
            jumpRequested = false;
        }
    }
}
