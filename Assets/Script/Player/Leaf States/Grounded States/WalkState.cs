using UnityEngine;

public class WalkState : GroundedState
{
    public WalkState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        if (manager.footstepClip != null)
        {
            manager.audioManager.PlaySFX(manager.footstepClip, 0);
        }

        manager.animator.Play(PlayerAnimateHash.Walk, 0, 0f);
    }

    public override void Update()
    {
        base.Update();
        if (manager.currentState != this) return;

        if (Mathf.Abs(manager.currentMoveX) <= 0.1f)
        {
            manager.TransitionToState<IdleState>();
            return;
        }

        manager.playerEnergy.canRecoverEnergy = manager.isGrounded;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float targetSpeedX = manager.currentMoveX * manager.walkSpeed;
        float currentSpeedX = manager.rig.linearVelocity.x;

        float newSpeedX = Mathf.MoveTowards(
            currentSpeedX,
            targetSpeedX,
            manager.acceleration * Time.fixedDeltaTime
        );

        manager.rig.linearVelocity = new Vector3(
            newSpeedX,
            manager.rig.linearVelocity.y,
            0f
        );
    }

    public override void Exit()
    {
        if (manager.audioManager != null)
        {
            manager.audioManager.StopSFX();
        }
        base.Exit();
        manager.playerEnergy.canRecoverEnergy = false;
    }
}
