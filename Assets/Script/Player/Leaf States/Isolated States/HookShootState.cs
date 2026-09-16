using UnityEngine;
using System.Collections;

public class HookShootState : PlayerState
{
    private Vector3 shootDir;

    // state
    private bool isRetracting;
    private bool isPaused;
    private float pauseTimer;
    private Vector3 pauseHitPoint;

    // retract timing
    private float retractElapsedTime;
    private float retractTotalTime;

    public HookShootState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        manager.canTurn = false;

        shootDir = manager.GetMouseDirection();
        manager.currentHookTipPos = manager.transform.position;

        isRetracting = false;
        isPaused = false;
        pauseTimer = 0f;

        retractElapsedTime = 0f;
        retractTotalTime = 0f;

        // stop the player while shooting the hook
        manager.rig.linearVelocity = Vector3.zero;

        // face the shooting direction
        manager.FaceTowards(manager.transform.position + shootDir * 10f);

        if (manager.hookClip != null) {
            manager.audioManager.PlaySFX(manager.hookClip);
        }

        // show rope
        manager.hookLineRenderer.enabled = true;
        manager.hookLineRenderer.positionCount = Mathf.Max(2, manager.ropeResolution);
    }

    public override void Update()
    {
        base.Update();
        DrawRope();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // keep player suspended during this state
        manager.rig.linearVelocity = Vector3.zero;

        if (isPaused)
        {
            UpdateBounce();
        }
        else if (isRetracting)
        {
            RetractHook();
        }
        else
        {
            ShootHook();
        }
    }

    // shoot
    private void ShootHook()
    {
        float step = manager.hookShootSpeed * Time.fixedDeltaTime;
        int layerMask = manager.hookLayer | manager.groundLayer;

        if (Physics.SphereCast(manager.currentHookTipPos, manager.hookTipRadius, shootDir, out RaycastHit hit, step, layerMask))
        {
            // hook target
            if (IsInLayer(hit.collider.gameObject.layer, manager.hookLayer))
            {
                HookTarget targetInfo = hit.collider.GetComponent<HookTarget>();
                HookTargetType type = targetInfo != null ? targetInfo.targetType : HookTargetType.HookPoint;

                if (targetInfo == null || !targetInfo.canBeHooked) { 
                    return;
                }

                // Play Hook Animation
                if (targetInfo != null) {
                    targetInfo.TriggerHitAnimation();
                }

                manager.currentHookTarget = manager.CalculateHookDestination(hit.collider.transform.position, type);
                manager.TransitionToState<HookState>();
                return;
            }

            // ground / wall
            if (IsInLayer(hit.collider.gameObject.layer, manager.groundLayer))
            {
                manager.currentHookTipPos = hit.point - shootDir * (manager.hookTipRadius * 0.5f);
                StartPause();
                return;
            }
        }

        // Move hook forward.
        manager.currentHookTipPos += shootDir * step;

        // Check for overlaps after moving.
        if (CheckHit()) return;

        // Maximum range reached.
        float distance = Vector3.Distance(manager.transform.position, manager.currentHookTipPos);
        if (distance >= manager.hookRange)
        {
            manager.currentHookTipPos = manager.transform.position + shootDir * manager.hookRange;
            StartPause();
        }
    }

    // pause and bounce
    private void StartPause()
    {
        isPaused = true;
        isRetracting = false;
        pauseTimer = 0f;
        pauseHitPoint = manager.currentHookTipPos;
    }

    private void UpdateBounce()
    {
        pauseTimer += Time.fixedDeltaTime;

        float normalized = Mathf.Clamp01(pauseTimer / Mathf.Max(manager.hookMissPauseTime, 0.0001f));
        float decay = 1f - normalized;
        float bounce = Mathf.Abs(Mathf.Sin(pauseTimer * manager.bounceFrequency)) * manager.bounceAmplitude * decay;

        manager.currentHookTipPos = pauseHitPoint - shootDir * bounce;

        if (pauseTimer >= manager.hookMissPauseTime)
        {
            StartRetract();
        }
    }

    // retract
    private void StartRetract()
    {
        isPaused = false;
        isRetracting = true;
        retractElapsedTime = 0f;

        float startDistance = Vector3.Distance(manager.currentHookTipPos, manager.transform.position);
        retractTotalTime = startDistance / Mathf.Max(manager.hookRetractSpeed, 0.0001f);
    }

    private void RetractHook()
    {
        float step = manager.hookRetractSpeed * Time.fixedDeltaTime;

        manager.currentHookTipPos = Vector3.MoveTowards(manager.currentHookTipPos, manager.transform.position, step);
        retractElapsedTime += Time.fixedDeltaTime;

        if (Vector3.Distance(manager.currentHookTipPos, manager.transform.position) <= 0.2f)
        {
            manager.currentHookTipPos = manager.transform.position;

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

    private void DrawRope()
    {
        Vector3 start = manager.transform.position;
        Vector3 end = manager.currentHookTipPos;

        // prevent math errors if too close
        if ((end - start).sqrMagnitude <= 0.000001f) return;

        // set line nodes count
        int resolution = Mathf.Max(2, manager.ropeResolution);
        manager.hookLineRenderer.positionCount = resolution;

        if (!isRetracting)
        {
            DrawStraightRope(start, end, resolution);
            return;
        }

        // ir is retracting, add wave effect
        float retractProgress = retractTotalTime > 0f ? Mathf.Clamp01(retractElapsedTime / retractTotalTime) : 1f;

        // dynamic wave size
        float growth = manager.ropeSGrowth.Evaluate(retractProgress);
        float sAmplitude = manager.ropeSAmount * growth;
        Vector3 ropeDir = (end - start).normalized;

        // get perpendicular direction for wave
        Vector3 side = new Vector3(-ropeDir.y, ropeDir.x, 0f);

        for (int i = 0; i < resolution; i++)
        {
            // t = %
            float t = i / (float)(resolution - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            // create S-shape wave
            float s = Mathf.Sin(t * Mathf.PI * 2f * manager.ropeSFrequency);

            // pin both ends so they don't move
            float envelope = Mathf.Sin(Mathf.PI * t);

            // apply wave offset
            point += side * s * envelope * sAmplitude;

            // keep the visual rope above the ground.
            point = ClampRopePointToGround(point);

            manager.hookLineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// start shoot is stright rope
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="resolution"></param>
    private void DrawStraightRope(Vector3 start, Vector3 end, int resolution)
    {
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            manager.hookLineRenderer.SetPosition(i, Vector3.Lerp(start, end, t));
        }
    }

    /// <summary>
    /// prevent rope touch ground
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private Vector3 ClampRopePointToGround(Vector3 point)
    {
        if (Physics.Raycast(point + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f, manager.groundLayer))
        {
            point.y = Mathf.Max(point.y, hit.point.y + 0.05f);
        }
        return point;
    }


    // hit detection
    private bool CheckHit()
    {
        int layerMask = manager.hookLayer | manager.groundLayer;
        Collider[] hits = Physics.OverlapSphere(manager.currentHookTipPos, manager.hookTipRadius, layerMask);

        foreach (Collider hit in hits)
        {
            // hook target
            if (IsInLayer(hit.gameObject.layer, manager.hookLayer))
            {
                HookTarget targetInfo = hit.GetComponent<HookTarget>();
                HookTargetType type = targetInfo != null ? targetInfo.targetType : HookTargetType.HookPoint;

                manager.currentHookTarget = manager.CalculateHookDestination(hit.transform.position, type);
                manager.TransitionToState<HookState>();
                return true;
            }

            // ground
            if (IsInLayer(hit.gameObject.layer, manager.groundLayer))
            {
                manager.currentHookTipPos = hit.ClosestPoint(manager.currentHookTipPos);
                StartPause();
                return true;
            }
        }

        return false;
    }

    private bool IsInLayer(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    public override void Exit()
    {
        base.Exit();
        manager.canTurn = true;
        manager.hookLineRenderer.enabled = false;
        manager.hookLineRenderer.positionCount = 2;
    }
}