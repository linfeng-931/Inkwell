using UnityEngine;
using UnityEngine.UI;

// [System.Serializable]
// public class CursorStyle
// {
//     [Header("Visuals")]
//     public Sprite sprite;
//     public Color color = Color.white;
//
//     [Header("Animator")]
//     public RuntimeAnimatorController animator;
// }

public class CursorManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public PlayerEnergy playerEnergy;
    public Camera mainCamera;
    
    // [Header("Cursor UI")]
    // public RectTransform cursorUI;
    // public Image cursorImage;
    // public Animator cursorAnimator;
    // public CursorStyle normalStyle;

    [Header("Lock-On UI")]
    public RectTransform lockOnUI;
    public Animator lockOnAnimator;

    [Header("Settings")]
    public float detectionRadius = 10f; 
    public float snapAngleThreshold = 0.9f; 
    
    [Header("Physics")]
    public LayerMask obstacleLayer;

    // for player hook
    public HookTarget CurrentTarget { get; private set; }
    
    private HookTarget previousTarget;
    private float hookCost;

    private void Start()
    {
        // 改用 Unity 內建滑鼠，確保滑鼠顯示並解除鎖定
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // hidden cursor (已註解)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Confined;

        detectionRadius = player.GetComponent<PlayerController>().hookRange;
        hookCost = player.GetComponent<PlayerController>().hookCost;
        
        // ApplyCursorStyle(normalStyle); (已註解)
    }

    // private void ApplyCursorStyle(CursorStyle style)
    // {
    //     if (style == null) return;
    //
    //     // activite animator
    //     if (style.animator != null && cursorAnimator != null)
    //     {
    //         cursorAnimator.enabled = true;
    //         cursorAnimator.runtimeAnimatorController = style.animator;
    //         cursorAnimator.Play(0, -1, 0f); 
    //     }
    //     else
    //     {
    //         // close animator
    //         if (cursorAnimator != null)
    //         {
    //             cursorAnimator.enabled = false;
    //             cursorAnimator.runtimeAnimatorController = null;
    //         }
    //
    //         if (style.sprite != null) cursorImage.sprite = style.sprite;
    //         cursorImage.color = style.color;
    //     }
    // }

    private void Update()
    {
        // cursorUI.position = Input.mousePosition; (已註解)

        CheckForGrappleTargets();

        UpdateLockOnUI();
    }

    private void CheckForGrappleTargets()
    {
        HookTarget bestTarget = null;
        float bestScore = -1f;

        Vector2 playerScreenPos = mainCamera.WorldToScreenPoint(player.position);
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector2 playerToMouseDir = (mouseScreenPos - playerScreenPos).normalized;

        HookTarget[] targets = FindObjectsOfType<HookTarget>();

        foreach (var target in targets)
        {
            if (!target.canBeHooked) continue;

            float distanceToPlayer = Vector3.Distance(player.position, target.transform.position);

            if (distanceToPlayer <= detectionRadius)
            {
                // detect hook obj
                if (!HasLineOfSight(target.transform.position)) continue;

                Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(target.transform.position);
                Vector2 playerToTargetDir = (targetScreenPos - playerScreenPos).normalized;

                float dotProduct = Vector2.Dot(playerToMouseDir, playerToTargetDir);

                if (dotProduct >= snapAngleThreshold)
                {
                    float score = dotProduct * 10f - distanceToPlayer + (target.priority * 5f);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = target;
                    }
                }
            }
        }

        CurrentTarget = bestTarget;
    }

    private void UpdateLockOnUI()
    {
        if (CurrentTarget != null && (playerEnergy.currentEnergy >= hookCost) )
        {
            Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(CurrentTarget.transform.position);
            lockOnUI.position = targetScreenPos;

            // find new obj -> Trigger open
            if (CurrentTarget != previousTarget)
            {
                if (lockOnAnimator != null)
                {
                    lockOnAnimator.ResetTrigger("close");
                    lockOnAnimator.SetTrigger("open");
                }
            }
        }
        else
        {
            // lost obj -> Trigger close
            if (previousTarget != null)
            {
                if (lockOnAnimator != null)
                {
                    lockOnAnimator.ResetTrigger("open");
                    lockOnAnimator.SetTrigger("close");
                }
            }
        }

        previousTarget = CurrentTarget;
    }

    private bool HasLineOfSight(Vector3 targetWorldPos)
    {
        Vector3 dirToTarget = targetWorldPos - player.position;
        float distance = dirToTarget.magnitude;

        if (Physics.Raycast(player.position, dirToTarget.normalized, distance, obstacleLayer))
        {
            return false; 
        }
        return true; 
    }
}