public class FallState : AirborneState
{
    public FallState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
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
