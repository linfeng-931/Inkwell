using UnityEngine;

public class JumpState : AirborneState
{
    private bool hasAppliedJumpCut;
    private float stateEnterTime;

    private const float MIN_JUMP_HOLD_DURATION = 0.08f;

    private const float MAX_JUMP_DURATION = 1.2f;

    public JumpState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        stateEnterTime = Time.time;
        hasAppliedJumpCut = false;

        if (manager.jumpClip != null)
        {
            manager.audioManager.PlaySFX(manager.jumpClip);
        }

        Vector3 v = manager.rig.linearVelocity;
        manager.rig.linearVelocity = new Vector3(v.x, manager.jumpForce, 0f);
        manager.animator.Play(PlayerAnimateHash.JumpStart, 0, 0f);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (manager.currentState != this) return;

        CheckAndApplyJumpCut();
        CheckTransitionToFall();
    }

    public override void Exit()
    {
        hasAppliedJumpCut = false;
        base.Exit();
    }

    private void CheckTransitionToFall()
    {
        bool isFalling = manager.rig.linearVelocity.y <= 0f;
        bool exceededMaxDuration = Time.time - stateEnterTime >= MAX_JUMP_DURATION;

        if (isFalling || exceededMaxDuration)
        {
            manager.TransitionToState<FallState>();
        }
    }


    private void CheckAndApplyJumpCut()
    {
        if (hasAppliedJumpCut) return;
        if (Time.time - stateEnterTime < MIN_JUMP_HOLD_DURATION) return;
        if (manager.inputBufferManager.isJumpHeld) return;

        Vector3 v = manager.rig.linearVelocity;
        if (v.y > 0f)
        {
            manager.rig.linearVelocity = new Vector3(v.x, v.y * manager.jumpCutMultiplier, 0f);
        }
        hasAppliedJumpCut = true;
    }
}