using Unity.Cinemachine;
using UnityEngine;

public class DeathState : PlayerState
{
    private float stateStartTime;
    private bool hasRelocated = false;
    private bool hasTriggeredFadeOut = false;

    public DeathState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        stateStartTime = Time.time;
        hasRelocated = false;
        hasTriggeredFadeOut = false;

        manager.animator.Play(PlayerAnimateHash.Dead, 0, 0f);

        manager.rig.linearVelocity = new Vector3(0f, manager.rig.linearVelocity.y, 0f);

        manager.canTurn = false;
        manager.isPlayerInputEnabled = false;
    }

    public override void Update()
    {
        base.Update();

        float elapsedTime = Time.time - stateStartTime;

        // fade out
        if (!hasTriggeredFadeOut && elapsedTime >= manager.fadeOutWaitTime)
        {
            hasTriggeredFadeOut = true;
            GameEvent.OnToggleFade(false);
        }

        // Reset Player Data
        if (!hasRelocated && elapsedTime >= manager.fadeOutWaitTime + 1.0f) // fade out ani 1s
        {
            hasRelocated = true;

            manager.rig.linearVelocity = Vector3.zero;
            manager.rig.position = manager.lastCheckpointPosition;
            manager.transform.position = manager.lastCheckpointPosition;

            // move camera
            if (manager.mainCam.GetComponent<CinemachineBrain>().ActiveVirtualCamera is CinemachineVirtualCameraBase vcam)
            {
                vcam.PreviousStateIsValid = false;
            }

            manager.animator.Play(PlayerAnimateHash.Idle, 0, 0f);
            GameEvent.OnToggleFade(true);
        }

        // end
        if (elapsedTime >= manager.fadeOutWaitTime+ 3f)
        {
            manager.TransitionToState<IdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
        manager.isPlayerInputEnabled = true;
        manager.canTurn = true;
    }
}
