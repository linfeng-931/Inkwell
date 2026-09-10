using UnityEngine;

public class DoubleJumpState : JumpState
{
    public DoubleJumpState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.animator.Play(PlayerAnimateHash.DoubleJumpStart, 0, 0f);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
