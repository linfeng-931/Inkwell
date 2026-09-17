using UnityEngine;
using System.Collections;

public enum HookTargetType
{
    AirEnemy,
    GroundEnemy,
    HookPoint
}

public class HookTarget : MonoBehaviour
{
    public HookTargetType targetType;

    public bool canBeHooked = true;

    [SerializeField] private Animator animator;
    [SerializeField] private Collider targetCollider;

    // Hook Target Animation
    public void TriggerHitAnimation()
    {
        if (!canBeHooked) return;
        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        canBeHooked = false;
        if (targetCollider != null) targetCollider.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Hit");

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("pen_object_open"));
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("pen_object_closed"));
        }
        canBeHooked = true;
        if (targetCollider != null) targetCollider.enabled = true;
    }
}
