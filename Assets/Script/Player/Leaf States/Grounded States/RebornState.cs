using UnityEngine;

public class RebornState : GroundedState
{
    public RebornState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        manager.playerFace2.SetActive(true);
        manager.playerFace.SetActive(false);
        manager.animator2.Play(PlayerAnimateHash.Reborn, 0, 0f);
        manager.isPlayerInputEnabled = false;
        manager.canTurn = false;
    }

    public override void Update()
    {
        base.Update();

        AnimatorStateInfo stateInfo = manager.animator2.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == PlayerAnimateHash.Reborn && stateInfo.normalizedTime >= 1.0f)
        {
            manager.TransitionToState<IdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();

        manager.playerFace2.SetActive(false);
        manager.playerFace.SetActive(true);
        manager.isPlayerInputEnabled = true;
        manager.canTurn = true;
    }
}
