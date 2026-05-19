using UnityEngine;

public class AnimationTriggerResetExample : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayJump()
    {
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Jump");
    }

    public void PlayAttack()
    {
        animator.ResetTrigger("Jump");
        animator.SetTrigger("Attack");
    }
}
