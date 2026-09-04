using UnityEngine;

public class GroundedState : PlayerState
{
    public GroundedState(PlayerController manager) : base(manager) { }

    public override void Update()
    {
        base.Update();

        if (!manager.isGrounded)
        {
            manager.TransitionToState<FallState>();
            return;
        }

        //init canAirDash when player stand on the ground
        manager.canAirDash = true;

        //handle jump state
        manager.coyoteTimer = manager.coyoteTime;
        
        if (manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Jump))
        {
            manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Jump);
            manager.coyoteTimer = 0f;
            manager.TransitionToState<JumpState>();
        }
    }
}
