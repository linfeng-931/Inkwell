using UnityEngine;

public class SaveState : GroundedState
{
    private bool hasReachedTarget = false;
    private float stopDistance = 0.1f;
    
    public SaveState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        manager.isPlayerInputEnabled = false;
        manager.canTurn = false;

        hasReachedTarget = false;
        if (manager.cutscenePosition == null)
        {
            StartSavingAnimation();
        }
    }

    public override void Update()
    {
        base.Update();

        if (!hasReachedTarget && manager.cutscenePosition != null)
        {
            MoveToTarget();
        }
        else
        {
            AnimatorStateInfo stateInfo = manager.animator2.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == PlayerAnimateHash.Saving && stateInfo.normalizedTime >= 1.0f)
            {
                manager.TransitionToState<IdleState>();
            }
        }
    }

    private void MoveToTarget()
    {
        Vector3 targetPos = manager.cutscenePosition.position;
        float distanceX = Mathf.Abs(targetPos.x - manager.transform.position.x);

        if (distanceX <= stopDistance)
        {
            manager.rig.linearVelocity = new Vector3(0, manager.rig.linearVelocity.y, 0);
            manager.FaceTowards(targetPos);
            
            StartSavingAnimation();
        }
        else
        {
            float direction = targetPos.x > manager.transform.position.x ? 1f : -1f;
            manager.rig.linearVelocity = new Vector3(direction * manager.walkSpeed, manager.rig.linearVelocity.y, 0);
            manager.HandleTurning(direction > 0);
            manager.animator.Play(PlayerAnimateHash.Walk);
        }
    }

    public override void Exit()
    {
        base.Exit();

        SaveManager.Instance.SaveGame();
        manager.playerFace2.SetActive(false);
        manager.playerFace.SetActive(true);
        manager.isPlayerInputEnabled = true;
        manager.canTurn = true;
    }

    private void StartSavingAnimation()
    {
        hasReachedTarget = true;
        manager.playerFace2.SetActive(true);
        manager.playerFace.SetActive(false);
        manager.animator2.Play(PlayerAnimateHash.Saving, 0, 0f);
    }
}
