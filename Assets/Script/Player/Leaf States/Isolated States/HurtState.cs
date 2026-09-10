using UnityEngine;

public class HurtState : PlayerState
{
    private float stateStartTime;
    public HurtState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        stateStartTime = Time.time;

        manager.animator.Play(PlayerAnimateHash.Hurt, 0, 0f);

        float knockbackDir = manager.isFacingRight ? -1f : 1f;

        manager.rig.linearVelocity = new Vector3(knockbackDir * manager.hurtKnockbackForce, 0f, 0f);
        manager.rig.useGravity = false;

        manager.canTurn = false;
    }

    public override void Update()
    {
        base.Update();

        float elapsedTime = Time.time - stateStartTime;

        // stop knockback
        if(elapsedTime >= manager.hurtKnockbackDuration)
        {
            manager.rig.linearVelocity = new Vector3(0f, manager.rig.linearVelocity.y, 0f);
        }

        // end of hurt state
        if (elapsedTime >= manager.hurtDuration)
        {
            if (manager.isGrounded)
            {
                manager.TransitionToState<IdleState>();
            }
            else
            {
                manager.TransitionToState<FallState>();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        manager.rig.useGravity = true;
        manager.canTurn = true;
    }
}
