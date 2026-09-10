using UnityEngine;

public class WalkState : GroundedState
{
    public WalkState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.animator.Play(PlayerAnimateHash.Walk, 0, 0f);
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
