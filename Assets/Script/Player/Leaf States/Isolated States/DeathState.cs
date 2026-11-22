using Unity.Cinemachine;
using UnityEngine;

public class DeathState : PlayerState
{
    private int stage = 0;
    private float stateStartTime;
    private bool hasRelocated = false;
    private bool hasTriggeredFadeOut = false;
    private int originalLayer;

    public DeathState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        hasRelocated = false;
        hasTriggeredFadeOut = false;
        stage = 0;

        originalLayer = manager.gameObject.layer;
        manager.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        manager.animator.Play(PlayerAnimateHash.Dead, 0, 0f);
        manager.rig.linearVelocity = Vector3.zero;
        manager.canTurn = false;
        manager.isPlayerInputEnabled = false;
    }

    public override void Update()
    {
        base.Update();

        switch (stage)
        {
            case 0:
                AnimatorStateInfo stateInfo = manager.animator.GetCurrentAnimatorStateInfo(0);
                if(stateInfo.normalizedTime >= 0.8f)
                {
                    manager.deadStartEffect.Play();
                    
                    stateStartTime = Time.unscaledTime;
                    stage++;
                }
                break;
            case 1:
                float elapsedTime1 = Time.unscaledTime - stateStartTime;
                if (elapsedTime1 >= 0.5f)
                {
                    manager.deadEndEffect.Play();
                    manager.sfxAudioSource.PlayOneShot(manager.hurtClip);
                    stateStartTime = Time.unscaledTime;
                    stage++;
                }
                break;
            case 2:
                float elapsedTime = Time.unscaledTime - stateStartTime;
                if (!hasTriggeredFadeOut && elapsedTime >= manager.fadeOutWaitTime)
                {
                    hasTriggeredFadeOut = true;
                    GameEvent.OnToggleFade(false);
                }

                if (!hasRelocated && elapsedTime >= manager.fadeOutWaitTime + 1.5f)
                {
                    hasRelocated = true;
                    Vector3 safeTarget = manager.lastCheckpointPosition + Vector3.up * 0.2f;

                    manager.TeleportTo(safeTarget, true);

                    manager.playerHealth.ResetHealth();

                    if (manager.col != null)
                    {
                        manager.col.enabled = true;
                    }
                    Physics.SyncTransforms();

                    if (manager.mainCam != null && manager.mainCam.GetComponent<CinemachineBrain>().ActiveVirtualCamera is CinemachineVirtualCameraBase vcam)
                    {
                        vcam.PreviousStateIsValid = false;
                    }

                    manager.animator.Play(PlayerAnimateHash.Idle, 0, 0f);
                    GameEvent.OnToggleFade(true);
                }

                if (elapsedTime >= manager.fadeOutWaitTime + 1.5f)
                {
                    manager.TransitionToState<IdleState>();
                }
                break;
        }   
    }

    public override void Exit()
    {
        base.Exit();

        manager.gameObject.layer = originalLayer;
        if (manager.col != null) manager.col.enabled = true;

        manager.isHazardHurt = false;
        manager.isPlayerInputEnabled = true;
        manager.canTurn = true;
    }
}