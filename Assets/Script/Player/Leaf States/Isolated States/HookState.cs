using UnityEngine;

public class HookState : PlayerState
{
    private Vector3 hookDir;
    private bool isHanging;
    private float hangTimer;

    private Quaternion penOriginalAngle;

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
            manager.hookLineRenderer.positionCount = 2;
            manager.hookLineRenderer.SetPosition(0, manager.hookLineStartPoint.position);

            manager.hookLineRenderer.SetPosition(1, manager.currentRopeVisualPos);
        }

        // reset attribute
        manager.canAirDash = true;
        manager.currentAirJumps = 1; 

        // handle pen
        penOriginalAngle = manager.hookPenRoot.localRotation;
        manager.hookPenObject.SetActive(true);
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

        // handle pen
        Vector3 dirToTarget = (manager.currentRopeVisualPos - manager.transform.position).normalized;
        float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        
        bool isFacingRight = manager.transform.localScale.x > 0; 
        float baseAngle = isFacingRight ? 0f : 180f;
        float relativeAngle = Mathf.DeltaAngle(baseAngle, targetAngle);
        float clampedRelativeAngle = Mathf.Clamp(relativeAngle, manager.maxArmAngleDown, manager.maxArmAngleUp);
        
        float finalArmAngle = baseAngle + clampedRelativeAngle;
        manager.hookPenRoot.rotation = Quaternion.Euler(0, 0, finalArmAngle);

        // pen tip stop on target pos
        manager.hookPenTip.position = manager.currentRopeVisualPos;
        float tipAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        manager.hookPenTip.rotation = Quaternion.Euler(0, 0, tipAngle);

        // update the start and end point of line
        if (manager.hookLineRenderer != null)
        {
            manager.hookLineRenderer.SetPosition(0, manager.transform.position);
            manager.hookLineRenderer.SetPosition(1, manager.currentRopeVisualPos);
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
        manager.hookPenObject.SetActive(false);

        if (manager.hookLineRenderer != null)
        {
            manager.hookLineRenderer.enabled = false;
        }

        manager.hookPenTip.position = manager.hookStartPoint.position;
        manager.hookPenTip.localRotation = penOriginalAngle;
        manager.hookPenRoot.localRotation = penOriginalAngle;
    }
}