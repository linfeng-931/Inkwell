using UnityEngine;
using UnityEngine.Video;

public class CutsceneState : PlayerState
{
    private enum ReadyStep
    {
        WaitingForGround,
        MovingToPosition,
        TimelineControlled
    }

    private ReadyStep currentStep;
    private const float STOP_DISTANCE = 0.01f; // stop range

    public CutsceneState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        manager.isPlayerInputEnabled = false;
        manager.canTurn = false;
        manager.isCutsceneReady = false;

        manager.rig.linearVelocity = new Vector3(0f, manager.rig.linearVelocity.y, 0f);

        if(manager.requireGroundedForCutscene && !manager.isGrounded)
        {
            currentStep = ReadyStep.WaitingForGround;
        }
        else
        {
            StartMoveToPosition();
        }
    }

    public override void Update()
    {
        base.Update();

        switch (currentStep)
        {
            case ReadyStep.WaitingForGround:
                if (manager.isGrounded)
                {
                    StartMoveToPosition();
                }
                break;
            case ReadyStep.MovingToPosition:
                MoveTowardsPosition();
                break;
            case ReadyStep.TimelineControlled:
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
        manager.isPlayerInputEnabled = true;
        manager.canTurn = true;
        manager.rig.useGravity = true;
        manager.cutscenePosition = null;

        bool currentVisualDir = manager.transform.localScale.x >= 0 ? true : false;
        manager.isFacingRight = currentVisualDir;
    }

    /// <summary>
    /// ready to enter moving
    /// </summary>
    private void StartMoveToPosition()
    {
        if(manager.cutscenePosition != null)
        {
            currentStep = ReadyStep.MovingToPosition;
            manager.animator.Play(PlayerAnimateHash.Walk, 0, 0f);
        }
        else
        {
            LockPhysicsAndNotifyReady();
        }
    }

    /// <summary>
    /// move to target position
    /// </summary>
    private void MoveTowardsPosition()
    {
        Vector3 targetPos = manager.cutscenePosition.position;
        Vector3 currentPos = manager.transform.position;
        float horizontalDistance = targetPos.x - currentPos.x;

        if(Mathf.Abs(horizontalDistance) <= STOP_DISTANCE)
        {
            manager.transform.position = new Vector3(targetPos.x, currentPos.y, currentPos.z);
            LockPhysicsAndNotifyReady();
        }
        else
        {
            float dir = Mathf.Sign(horizontalDistance);
            manager.rig.linearVelocity = new Vector3(dir * manager.walkSpeed, manager.rig.linearVelocity.y, 0f);
        }
    }

    /// <summary>
    /// ready to enter timeline
    /// </summary>
    private void LockPhysicsAndNotifyReady()
    {
        currentStep = ReadyStep.TimelineControlled;
        manager.rig.linearVelocity = Vector3.zero;
        manager.animator.Play(PlayerAnimateHash.Idle, 0, 0f);

        manager.isCutsceneReady = true; // start to play timeline (CutsceneManager.cs)
    }
}
