using UnityEngine;

public class JumpState : AirborneState
{
    private bool hasAppliedJumpCut;

    public JumpState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("jump");
        hasAppliedJumpCut = false;
        manager.rig.linearVelocity = new Vector3(manager.rig.linearVelocity.x, manager.jumpForce, 0f);
    }

    public override void Update()
    {
        base.Update();
        if (manager.currentState != this) return;

        if (!hasAppliedJumpCut && !manager.inputBufferManager.isJumpHeld)
        {
            ApplyJumpCut();
        }

        if (manager.rig.linearVelocity.y <= 0)
        {
            manager.TransitionToState<FallState>();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        hasAppliedJumpCut = false;
        base.Exit();
    }

    /// <summary>
    /// handle variable jump height
    /// </summary>
    private void ApplyJumpCut()
    {
        if(manager.rig.linearVelocity.y > 0)
        {
            manager.rig.linearVelocity = new Vector3(
                manager.rig.linearVelocity.x,
                manager.rig.linearVelocity.y * manager.jumpCutMultiplier,
                0f
            );
        }

        hasAppliedJumpCut = true;
    }
}
