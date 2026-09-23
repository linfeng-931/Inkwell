using UnityEngine;

public class InteractState : PlayerState
{
    public InteractState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.isPlayerInputEnabled = false;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        manager.isPlayerInputEnabled = true;
    }
}
