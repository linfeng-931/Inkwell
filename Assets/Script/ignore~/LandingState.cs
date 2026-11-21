using UnityEngine;

public class LandingState : GroundedState
{
    private float landingDuration = 0.4f;
    private float timer = 0f;

    public LandingState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        timer = 0f;

        if (manager.jumpClip != null)
        {
            manager.audioManager.PlaySFX(manager.jumpClip);
        }

        manager.animator.Play(PlayerAnimateHash.Landing, 0, 0f);

        manager.rig.linearVelocity = new Vector3(0f, manager.rig.linearVelocity.y, 0f);
    }

    public override void Update()
    {
        base.Update();
        if (manager.currentState != this) return;

        timer += Time.deltaTime;

        bool hasMovementInput = Mathf.Abs(manager.currentMoveX) > 0.01f;

        if (timer >= landingDuration || hasMovementInput)
        {
            if (hasMovementInput)
                manager.TransitionToState<RunState>();
            else
                manager.TransitionToState<IdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
