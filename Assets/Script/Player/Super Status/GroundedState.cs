using UnityEngine;

public class GroundedState : PlayerState
{
    public GroundedState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.currentAirJumps = 1;
    }
    
    public override void Update()
    {
        if (!manager.isGrounded)
        {
            manager.TransitionToState<FallState>();
            return;
        }

        //init canAirDash when player stand on the ground
        manager.canAirDash = true;

        //handle jump state and coyote time
        manager.coyoteTimer = manager.coyoteTime;
        
        if (manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Jump))
        {
            manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Jump);
            manager.coyoteTimer = 0f;
            manager.TransitionToState<JumpState>();
            return;
        }
    }
}
