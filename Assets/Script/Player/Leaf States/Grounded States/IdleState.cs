using UnityEngine;

public class IdleState : GroundedState
{
    public IdleState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        //prevent state from being overwritten
        if (manager.currentState != this) return;

        //check if player need to change state
        if (Mathf.Abs(manager.currentMoveX) > 0.1f)
        {
            manager.TransitionToState<RunState>();
        }

        //action logic
        if(manager.rig.linearVelocity.x!=0) {
            float currentSpeedX = manager.rig.linearVelocity.x;
            float newSpeedX = Mathf.MoveTowards(
                currentSpeedX, 
                0f, 
                manager.deceleration * Time.deltaTime
            );

            manager.rig.linearVelocity = new Vector3(
                newSpeedX,
                manager.rig.linearVelocity.y,
                0f
            );
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
