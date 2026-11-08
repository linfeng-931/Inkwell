using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public PlayerEnergy playerEnergy;
    public Camera mainCamera;

    [Header("Lock-On UI - Hook Point")]
    public RectTransform lockOnUIHookPoint;
    public Animator lockOnAnimatorHookPoint;

    [Header("Lock-On UI - Enemy")]
    public RectTransform lockOnUIEnemy;
    public Animator lockOnAnimatorEnemy;

    [Header("Settings")]
    public float detectionRadius = 10f; 
    public float snapAngleThreshold = 0.9f; 
    public float controllerDeadzone = 0.2f;
    
    [Header("Physics")]
    public LayerMask obstacleLayer;

    // for player hook
    public HookTarget CurrentTarget { get; private set; }
    
    private HookTarget previousTarget;
    private float hookCost;
    private PlayerController playerController;
    private bool isUIShowingHookPoint = false;
    private bool isUIShowingEnemy = false;

    private bool isUsingController = false;

    private void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        detectionRadius = playerController.hookRange;
        hookCost = playerController.hookCost;

        UpdateCursorVisibility();
    }

    private void Update()
    {
        CheckForGrappleTargets();
        UpdateLockOnUI();
    }

    private void UpdateCursorVisibility()
    {
        Cursor.visible = !isUsingController;
        Cursor.lockState = isUsingController ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void CheckForGrappleTargets()
    {
        HookTarget bestTarget = null;
        float bestScore = -1f;

        Vector2 aimDirection = GetAimDirection();

        if (aimDirection.sqrMagnitude < 0.01f)
        {
            CurrentTarget = null;
            return;
        }
        
        HookTarget[] targets = FindObjectsOfType<HookTarget>();

        foreach (var target in targets)
        {
            if (!target.canBeHooked) continue;

            float distanceToPlayer = Vector3.Distance(player.position, target.transform.position);

            if (distanceToPlayer <= detectionRadius)
            {
                if (!HasLineOfSight(target.transform.position)) continue;

                Vector2 playerToTargetDir = ((Vector2)target.transform.position - (Vector2)player.position).normalized;

                float dotProduct = Vector2.Dot(aimDirection, playerToTargetDir);

                if (dotProduct >= snapAngleThreshold)
                {
                    float score = (dotProduct * 10f) - distanceToPlayer + (target.priority * 5f);

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
    
    /// <summary>
    /// getting aim direction in world space according to input equipment
    /// </summary>
    /// <returns></returns>
    private Vector2 GetAimDirection()
    {
        if (GameInputManager.Instance == null) return playerController.isFacingRight ? Vector2.right : Vector2.left;

        Vector2 moveInput = GameInputManager.Instance.MoveInput;
        // check input from
        bool isGamepad = GameInputManager.Instance.IsMoveInputFromGamepad;

        // controller
        if (isGamepad && moveInput.sqrMagnitude > controllerDeadzone * controllerDeadzone)
        {
            if (!isUsingController)
            {
                isUsingController = true;
                UpdateCursorVisibility();
            }
            return moveInput.normalized;
        }

        // mouse and keyboard
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            if (mouseDelta.sqrMagnitude > 1.0f || (!isGamepad && moveInput.sqrMagnitude > 0f))
            {
                if (isUsingController)
                {
                    isUsingController = false;
                    UpdateCursorVisibility();
                }
            }

            if (!isUsingController && mainCamera != null && player != null)
            {
                Vector2 playerScreenPos = mainCamera.WorldToScreenPoint(player.position);
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                return (mouseScreenPos - playerScreenPos).normalized;
            }
        }

        // default => player direction
        return playerController.isFacingRight ? Vector2.right : Vector2.left;
    }

    private void UpdateLockOnUI()
    {
        bool isGrappling = playerController.currentState is HookShootState || playerController.currentState is HookState;
        bool shouldShow = CurrentTarget != null && playerEnergy.currentEnergy >= hookCost && !isGrappling;

        bool showHookPoint = false;
        bool showEnemy = false;

        if (shouldShow)
        {
            Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(CurrentTarget.transform.position);

            if (CurrentTarget.targetType == HookTargetType.HookPoint)
            {
                showHookPoint = true;
                lockOnUIHookPoint.position = targetScreenPos;
            }
            else if (CurrentTarget.targetType == HookTargetType.AirEnemy || CurrentTarget.targetType == HookTargetType.GroundEnemy)
            {
                showEnemy = true;
                lockOnUIEnemy.position = targetScreenPos;
            }

            if (CurrentTarget != previousTarget && previousTarget != null)
            {
                if (previousTarget.targetType == HookTargetType.HookPoint && isUIShowingHookPoint)
                {
                    CloseUI(lockOnAnimatorHookPoint, ref isUIShowingHookPoint);
                }
                else if ((previousTarget.targetType == HookTargetType.AirEnemy || previousTarget.targetType == HookTargetType.GroundEnemy) && isUIShowingEnemy)
                {
                    CloseUI(lockOnAnimatorEnemy, ref isUIShowingEnemy);
                }
            }
        }

        if (showHookPoint)
        {
            if (!isUIShowingHookPoint || CurrentTarget != previousTarget)
                OpenUI(lockOnAnimatorHookPoint, ref isUIShowingHookPoint);
        }
        else if (isUIShowingHookPoint)
        {
            CloseUI(lockOnAnimatorHookPoint, ref isUIShowingHookPoint);
        }

        if (showEnemy)
        {
            if (!isUIShowingEnemy || CurrentTarget != previousTarget)
                OpenUI(lockOnAnimatorEnemy, ref isUIShowingEnemy);
        }
        else if (isUIShowingEnemy)
        {
            CloseUI(lockOnAnimatorEnemy, ref isUIShowingEnemy);
        }

        previousTarget = CurrentTarget;
    }

    private void OpenUI(Animator animator, ref bool isShowingFlag)
    {
        if (animator != null)
        {
            animator.ResetTrigger("close");
            animator.SetTrigger("open");
        }
        isShowingFlag = true;
    }

    private void CloseUI(Animator animator, ref bool isShowingFlag)
    {
        if (animator != null)
        {
            animator.ResetTrigger("open");
            animator.SetTrigger("close");
        }
        isShowingFlag = false;
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