using UnityEngine;

public class AirborneState : PlayerState
{
    // apex 倍率安全範圍,保證淨重力永遠為正
    private const float MIN_APEX_MULTIPLIER = 0.2f;

    public AirborneState(PlayerController manager) : base(manager) { }

    public override void Update()
    {
        // check if player need to change state
        bool isFalling = manager.rig.linearVelocity.y <= 0.5f;

        if (manager.isGrounded && isFalling)
        {
            if (Mathf.Abs(manager.currentMoveX) > 0.01f)
                manager.TransitionToState<RunState>();
            else
                manager.TransitionToState<IdleState>();
            return;
        }

        float currentSpeedY = manager.rig.linearVelocity.y;

        // handle jump state
        manager.coyoteTimer -= Time.deltaTime;

        if (manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Jump))
        {
            // first jump
            if (manager.coyoteTimer > 0f)
            {
                manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Jump);
                manager.coyoteTimer = 0f;
                manager.TransitionToState<JumpState>();
                return;
            }

            // if player is about to land, let ground state handle jump action
            bool isAboutToLand = false;
            if (currentSpeedY < 0f)
            {
                isAboutToLand = Physics.Raycast(
                    manager.col.bounds.center,
                    Vector3.down,
                    manager.col.bounds.extents.y + 0.3f,
                    manager.groundLayer
                );
            }
            if (!isAboutToLand && manager.currentAirJumps > 0)
            {
                manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Jump);
                manager.currentAirJumps--;
                manager.coyoteTimer = 0f;
                manager.TransitionToState<DoubleJumpState>();
                return;
            }
        }

        // handle attack state
        if (manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Attack))
        {
            manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Attack);

            if (manager.canAirAttack)
            {
                manager.currentComboList = manager.airCombo;
                manager.currentComboIndex = 0;
                manager.TransitionToState<AttackState>();
                return;
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (HandleLedgeSnap()) return;

        ApplyGravity();
        ApplyHorizontalMovement();
    }

    /// <summary>
    /// 自訂重力(物理力必須在 FixedUpdate 施加)。
    /// 頂點附近降低重力形成滯空感,倍率有下限,淨重力不會被抵消到零。
    /// </summary>
    private void ApplyGravity()
    {
        float vy = manager.rig.linearVelocity.y;
        bool isAtApex = Mathf.Abs(vy) < manager.apexThreshold;

        float gravityMult = manager.gravityScale;
        if (isAtApex)
        {
            gravityMult *= Mathf.Clamp(manager.apexHangTimeMultiplier, MIN_APEX_MULTIPLIER, 1f);
        }

        // Rigidbody 本身已有 1g,這裡只補差額
        Vector3 extraGravity = Physics.gravity * (gravityMult - 1f);
        manager.rig.AddForce(extraGravity, ForceMode.Acceleration);
    }

    private void ApplyHorizontalMovement()
    {
        float currentSpeedX = manager.rig.linearVelocity.x;

        float accelRate = (Mathf.Abs(manager.currentMoveX) > 0.01f) ? manager.acceleration : manager.deceleration;
        float targetSpeedX = manager.currentMoveX * manager.airMoveSpeed;

        float newSpeedX = Mathf.MoveTowards(currentSpeedX, targetSpeedX, accelRate * Time.fixedDeltaTime);

        manager.rig.linearVelocity = new Vector3(newSpeedX, manager.rig.linearVelocity.y, 0f);
    }

    /// <returns>true 表示已切換狀態,呼叫端應立刻中止後續處理</returns>
    private bool HandleLedgeSnap()
    {
        // must have horizontal input to trigger ledge snap
        if (Mathf.Abs(manager.currentMoveX) < 0.1f) return false;

        Vector3 moveDir = new Vector3(Mathf.Sign(manager.currentMoveX), 0f, 0f);
        Bounds bounds = manager.col.bounds;

        float range = manager.cornerCorrectionRange;
        float dashLookAhead = manager.dashLookAhead;
        LayerMask mask = manager.groundLayer;

        float radius = bounds.extents.x * 0.9f;
        float inset = radius + 0.05f;
        Vector3 p1 = new Vector3(bounds.center.x, bounds.max.y - inset, bounds.center.z);
        Vector3 p2 = new Vector3(bounds.center.x, bounds.min.y + inset, bounds.center.z);

        // detect front
        if (!Physics.CapsuleCast(p1, p2, radius, moveDir, out RaycastHit wallHit, dashLookAhead, mask))
            return false;

        Vector3 playerTop = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);

        // detect top
        if (Physics.Raycast(playerTop, Vector3.up, range, mask))
            return false; // blocked by ceiling

        Vector3 probeOrigin = (playerTop + Vector3.up * range) + (moveDir * (wallHit.distance + 0.05f));
        float probeDistance = range * 2f;

        if (Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit surfaceHit, probeDistance, mask))
        {
            if (surfaceHit.normal.y > 0.7f)
            {
                float heightDiff = surfaceHit.point.y - bounds.min.y;
                if (heightDiff > bounds.extents.y * 0.5f)
                {
                    Vector3 targetPos = manager.rig.position;
                    targetPos.y = surfaceHit.point.y + bounds.extents.y;

                    manager.rig.MovePosition(targetPos);
                    manager.rig.linearVelocity = new Vector3(manager.rig.linearVelocity.x, 0f, 0f);

                    if (Mathf.Abs(manager.currentMoveX) > 0.01f) manager.TransitionToState<RunState>();
                    else manager.TransitionToState<IdleState>();
                    return true;
                }
            }
        }

        return false;
    }
}