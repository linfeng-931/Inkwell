using Unity.Cinemachine;
using UnityEngine;

public class HurtState : PlayerState
{
    private float stateStartTime;
    private bool hasRelocated = false;
    private bool hasTriggeredFadeOut = false;

    public HurtState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        stateStartTime = Time.time;
        hasRelocated = false;
        hasTriggeredFadeOut = false;

        manager.animator.Play(PlayerAnimateHash.Hurt, 0, 0f);

        if(!manager.isHazardHurt) {
            float knockbackDir = manager.isFacingRight ? -1f : 1f;
            manager.rig.linearVelocity = new Vector3(knockbackDir * manager.hurtKnockbackForce, 0f, 0f);
        }
        else
        {
            manager.rig.linearVelocity = Vector3.zero;
            hasTriggeredFadeOut = true;
            GameEvent.OnToggleFade(false);
        }
        
        manager.rig.useGravity = false;
        manager.canTurn = false;
        manager.isPlayerInputEnabled = false;
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
            // hazard
            if (manager.isHazardHurt)
            {
                if (!hasRelocated && elapsedTime >= manager.hurtDuration + 1.0f) // fade out ani 1s
                {
                    hasRelocated = true;

                    manager.rig.position = manager.hazardRespawnPosition;
                    manager.transform.position = manager.hazardRespawnPosition;

                    // move camera
                    if (manager.mainCam.GetComponent<CinemachineBrain>().ActiveVirtualCamera is CinemachineVirtualCameraBase vcam)
                    {
                        vcam.PreviousStateIsValid = false;
                    }

                    manager.animator.Play(PlayerAnimateHash.Idle, 0, 0f);
                    GameEvent.OnToggleFade(true);
                }

                if(elapsedTime >= manager.hurtDuration + 1.5f)
                { 
                    manager.TransitionToState<IdleState>();
                }
                return;
            }

            // normal hurt
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
        manager.isPlayerInputEnabled = true;
        manager.isHazardHurt = false;
    }
}
