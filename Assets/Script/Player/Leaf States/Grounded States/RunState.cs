using UnityEngine;

public class RunState : GroundedState
{
    public RunState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.animator.Play(PlayerAnimateHash.Run, 0, 0f);
    }

    public override void Update()
    {
        base.Update();
        if (manager.currentState != this) return;

        //check if player need to change state
        if (Mathf.Abs(manager.currentMoveX) <= 0.1f)
        {
            manager.TransitionToState<IdleState>();
            return;
        }

        // recover energy
        manager.playerEnergy.canRecoverEnergy = manager.isGrounded;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float targetSpeedX = manager.currentMoveX * manager.runSpeed;
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
        base.Exit();
        manager.playerEnergy.canRecoverEnergy = false;
    }
}