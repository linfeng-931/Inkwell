using UnityEngine;

public class HookState : PlayerState
{
    private Vector3 hookDir;
    private bool isHanging;
    private float hangTimer;

    public HookState(PlayerController manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        manager.rig.linearVelocity = Vector3.zero;
        isHanging = false;
        hangTimer = 0f;

        // handle direction and face
        hookDir = (manager.currentHookTarget - manager.transform.position).normalized;
        manager.FaceTowards(manager.currentHookTarget);

        // visual effect
        if (manager.hookLineRenderer != null)
        {
            manager.hookLineRenderer.enabled = true;
        }
        
        // reset attribute
        manager.canAirDash = true;
        manager.currentAirJumps = 1; 
    }

    public override void Update()
    {
        base.Update();

        if (isHanging)
        {
            hangTimer += Time.deltaTime;
            if (hangTimer >= manager.hookHangTime)
            {
                // change state
                if (manager.isGrounded)
                {
                    manager.TransitionToState<IdleState>(); 
                }
                else
                {
                    manager.TransitionToState<FallState>(); 
                }
                return;
            }
            return;
        }

        // update the start and end point of line
        if (manager.hookLineRenderer != null)
        {
            manager.hookLineRenderer.SetPosition(0, manager.transform.position);
            manager.hookLineRenderer.SetPosition(1, manager.currentHookTarget);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // current distance and moving distance in this frame
        float distance = Vector3.Distance(manager.transform.position, manager.currentHookTarget);
        float moveStep = manager.hookSpeed * Time.fixedDeltaTime;

        // check if player arrived
        if (distance <= manager.hookStopDistance || distance <= moveStep)
        {
            manager.rig.linearVelocity = Vector3.zero;
            manager.rig.MovePosition(manager.currentHookTarget);

            if (manager.hookLineRenderer != null)
            {
                manager.hookLineRenderer.enabled = false;
            }

            isHanging = true;
        }
        else
        {
            manager.rig.linearVelocity = hookDir * manager.hookSpeed;
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (manager.hookLineRenderer != null)
        {
            manager.hookLineRenderer.enabled = false;
        }
    }
}