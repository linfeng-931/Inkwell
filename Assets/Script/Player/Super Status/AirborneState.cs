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

        //action logic
        float currentSpeedX = manager.rig.linearVelocity.x;
        float currentSpeedY = manager.rig.linearVelocity.y;

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
        //first jump
        manager.coyoteTimer -= Time.deltaTime;

        if (manager.coyoteTimer > 0f && manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Jump))
        {
            manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Jump);
            manager.coyoteTimer = 0f;
            manager.TransitionToState<JumpState>();
        }

        //double jump
    }
}
