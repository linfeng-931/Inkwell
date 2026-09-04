using UnityEngine;

public class JumpState : AirborneState
{
    public JumpState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        manager.rig.linearVelocity = new Vector3(manager.rig.linearVelocity.x, manager.jumpForce, 0f);
    }

    public override void Update()
    {
        base.Update();
        if (manager.currentState != this) return;

        if (manager.inputBufferManager.isJumpReleased)
        {
            if(manager.rig.linearVelocity.y > 0)
            {
                manager.rig.linearVelocity = new Vector3(
                    manager.rig.linearVelocity.x,
                    manager.rig.linearVelocity.y * manager.jumpCutMultiplier,
                    0f
                );
            }
        }
        if (manager.rig.linearVelocity.y <= 0)
        {
            manager.TransitionToState<FallState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
