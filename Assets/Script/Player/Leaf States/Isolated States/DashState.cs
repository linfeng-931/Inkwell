using UnityEngine;

public class DashState : PlayerState
{
    private Vector3 dashDirection;
    private float dashStartTime;

    public DashState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        dashStartTime = Time.time;
        dashDirection = new Vector3(
                manager.currentMoveX != 0 ?
                    manager.currentMoveX :
                    (manager.isFacingRight ? 1f : -1f),
                0f, 0f
            ).normalized;
        manager.rig.useGravity = false;
        manager.rig.linearVelocity = Vector3.zero;

        //需扣除空中次數、開啟無敵幀
    }

    public override void Update()
    {
        float timePassed = Time.time - dashStartTime;

        if (timePassed >= manager.dashDuration)
        {
            if (manager.isGrounded) manager.TransitionToState<IdleState>();
            else manager.TransitionToState<FallState>();
            return;
        }

        manager.rig.linearVelocity = dashDirection * manager.dashSpeed;
    }

    public override void FixedUpdate()
    {
        manager.rig.linearVelocity = dashDirection * manager.dashSpeed;
        HandleCornerCorrection();
    }

    public override void Exit()
    {
        manager.rig.useGravity = true;
        manager.rig.linearVelocity = manager.rig.linearVelocity * manager.dashEndCut;
    }

    /// <summary>
    /// if the player touches the corner of platform, allow them to climb onto it
    /// </summary>
    private void HandleCornerCorrection()
    {
        Vector3 pos = manager.transform.position;
        Vector3 dir = dashDirection;
        float lookAhead = manager.dashLookAhead;
        float range = manager.cornerCorrectionRange;

        // detect front of player
        bool isForwardBlocked = Physics.Raycast(pos, dir, lookAhead, manager.groundLayer);

        if (isForwardBlocked)
        {
            // detect up
            Vector3 upperPos = pos + Vector3.up * range;
            bool isUpperClear = !Physics.Raycast(upperPos, dir, lookAhead, manager.groundLayer);

            if (isUpperClear)
            {
                manager.rig.MovePosition(pos + Vector3.up * manager.correctionStep);
                return;
            }

            // the last choice, detect lower pos
            Vector3 lowerPos = pos + Vector3.down * range;
            bool isLowerClear = !Physics.Raycast(lowerPos, dir, lookAhead, manager.groundLayer);

            if (isLowerClear)
            {
                manager.rig.MovePosition(pos + Vector3.down * manager.correctionStep);
            }
        }
    }
}
