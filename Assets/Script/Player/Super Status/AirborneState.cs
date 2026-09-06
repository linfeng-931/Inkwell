using UnityEngine;

public class AirborneState : PlayerState
{
    public AirborneState(PlayerController manager) : base(manager) { }

    public override void Update()
    {
        //check if player need to change state
        bool isFalling = manager.rig.linearVelocity.y <= 0.5f;

        if (manager.isGrounded && isFalling)
        {
            if (Mathf.Abs(manager.currentMoveX) > 0.01f)
            {
                manager.TransitionToState<RunState>();
            }
            else
            {
                manager.TransitionToState<IdleState>();
            }
            return;
        }

        
        float currentSpeedY = manager.rig.linearVelocity.y;

        //handle gravity
        bool isAtApex = Mathf.Abs(currentSpeedY) < manager.apexThreshold; //threshold check (if player close the top point)
        float currentGravityMult = manager.gravityScale;

        if (isAtApex)
        {
            currentGravityMult *= manager.apexHangTimeMultiplier;
        }

        Vector3 customGravity = Physics.gravity * (currentGravityMult - 1f);
        manager.rig.AddForce(customGravity, ForceMode.Acceleration);

        //handle jump state
        manager.coyoteTimer -= Time.deltaTime;

        if(manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Jump)){
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
            if(currentSpeedY < 0f)
            {
                isAboutToLand = Physics.Raycast(
                    manager.col.bounds.center,
                    Vector3.down,
                    manager.col.bounds.extents.y + 0.3f,
                    manager.groundLayer
                );
            }
            if(!isAboutToLand && manager.currentAirJumps > 0)
            {
                manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Jump);
                manager.currentAirJumps--;
                manager.coyoteTimer = 0f;
                manager.TransitionToState<DoubleJumpState>();
                return;
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleLedgeSnap();

        float currentSpeedX = manager.rig.linearVelocity.x;

        //moving logic
        float accelRate = (Mathf.Abs(manager.currentMoveX) > 0.01f) ? manager.acceleration : manager.deceleration;
        float targetSpeedX = manager.currentMoveX * manager.airMoveSpeed;

        float newSpeedX = Mathf.MoveTowards(
            currentSpeedX,
            targetSpeedX,
            accelRate * Time.deltaTime
        );

        manager.rig.linearVelocity = new Vector3(
            newSpeedX,
            manager.rig.linearVelocity.y,
            0f
        );
    }

    private void HandleLedgeSnap()
    {
        // must have horizontal input to trigger ledge snap
        if(Mathf.Abs(manager.currentMoveX) < 0.1f) return;

        Vector3 moveDir = new Vector3(Mathf.Sign(manager.currentMoveX), 0f, 0f);
        Bounds bounds = manager.col.bounds;

        // parameter from manager
        float range = manager.cornerCorrectionRange;
        float dashLookAhead = manager.dashLookAhead;
        LayerMask mask = manager.groundLayer;

        float radius = bounds.extents.x * 0.9f;
        float inset = radius + 0.05f;
        Vector3 p1 = new Vector3(bounds.center.x, bounds.max.y - inset, bounds.center.z);
        Vector3 p2 = new Vector3(bounds.center.x, bounds.min.y + inset, bounds.center.z);

        // detect front
        if (!Physics.CapsuleCast(p1, p2, radius, moveDir, out RaycastHit wallHit, dashLookAhead, mask))
        {
            return; // no touch anything
        }

        Vector3 playerTop = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);

        // detect top
        if (Physics.Raycast(playerTop, Vector3.up, range, mask))
        {
            return; // blocked by ceiling
        }

        // handle main logic
        Vector3 probeOrigin = (playerTop + Vector3.up * range) + (moveDir * (wallHit.distance + 0.05f));
        float probeDistance = range * 2f;

        if (Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit surfaceHit, probeDistance, mask))
        {
            if (surfaceHit.normal.y > 0.7f)
            {
                float heightDiff = surfaceHit.point.y - bounds.min.y;
                if (heightDiff > bounds.extents.y * 0.5f) 
                {
                    float halfHeight = bounds.extents.y;
                    Vector3 targetPos = manager.rig.position;
                    targetPos.y = surfaceHit.point.y + halfHeight;

                    manager.rig.MovePosition(targetPos);
                    manager.rig.linearVelocity = new Vector3(manager.rig.linearVelocity.x, 0f, 0f);

                    if (Mathf.Abs(manager.currentMoveX) > 0.01f) manager.TransitionToState<RunState>();
                    else manager.TransitionToState<IdleState>();
                }
            }
        }
    }
}
