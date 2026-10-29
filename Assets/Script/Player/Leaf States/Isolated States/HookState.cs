using UnityEngine;

public class HookState : PlayerState
{
    private Vector3 hookDir;
    private bool isHanging;
    private float hangTimer;

    private Quaternion penOriginalAngle;

    private Vector3 lastPos;
    private float stuckTimer;

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

        // prevent stuck
        lastPos = manager.transform.position;
        stuckTimer = 0f;    
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
        Vector3 targetDir = (manager.currentRopeVisualPos - manager.hookPenRoot.position).normalized;
        Vector3 localDir = manager.hookPenRoot.parent.InverseTransformDirection(targetDir);
        localDir.x = Mathf.Abs(localDir.x);

        float localAngle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
        float clampedAngle = Mathf.Clamp(localAngle, manager.maxArmAngleDown, manager.maxArmAngleUp);
        manager.hookPenRoot.localRotation = Quaternion.Euler(0, 0, -clampedAngle);

        // pen tip stop on target pos
        manager.hookPenTip.position = manager.currentRopeVisualPos;

        // pen tip angle
        if (manager.hookLineRenderer != null && manager.hookLineRenderer.positionCount >= 2)
        {
            Vector3 startPos = manager.hookLineStartPoint.position;
            Vector3 endPos = manager.currentRopeVisualPos;
            Vector3 tipDir = (endPos - startPos).normalized;
            
            if (tipDir != Vector3.zero)
            {
                Vector3 localTipDir = manager.hookPenTip.parent.InverseTransformDirection(tipDir);
                float localTipAngle = Mathf.Atan2(localTipDir.y, localTipDir.x) * Mathf.Rad2Deg;
                
                float tipOffset = 140f; 
                manager.hookPenTip.localRotation = Quaternion.Euler(0, 0, localTipAngle + tipOffset);
            }
            
            manager.hookLineRenderer.SetPosition(0, manager.hookLineStartPoint.position);
            manager.hookLineRenderer.SetPosition(1, manager.currentRopeVisualPos);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isHanging) return;
        
        // current distance and moving distance in this frame
        float distance = Vector3.Distance(manager.transform.position, manager.currentHookTarget);
        float moveStep = manager.hookSpeed * Time.fixedDeltaTime;

        // handle stuck
        float actualMoveDist = Vector3.Distance(manager.transform.position, lastPos);

        if (actualMoveDist < (moveStep * 0.1f))
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= 0.1f) // time buff
            {
                manager.rig.linearVelocity = Vector3.zero;
                manager.TransitionToState<FallState>();
                return;
            }
        }
        else
        {
            stuckTimer = 0f; // if normal move, reset
        }
        
        lastPos = manager.transform.position;

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