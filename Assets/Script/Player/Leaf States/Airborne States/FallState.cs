public class FallState : AirborneState
{
    public FallState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.animator.Play(PlayerAnimateHash.Fall, 0, 0f);
    }

    public override void Update()
    {
        base.Update();
        if (manager.currentState != this) return;
    }

    public override void Exit()
    {
        base.Exit();
    }
}
