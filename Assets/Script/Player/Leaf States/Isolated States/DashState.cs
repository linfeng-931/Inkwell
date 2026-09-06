using UnityEngine;

public class DashState : PlayerState
{
    private Vector3 dashDirection;
    private float dashStartTime;
    private bool isForcedSliding = false;

    // record player original collider
    private float originalHeight;
    private float originalRadius;
    private Vector3 originalCenter;

    public DashState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        // init parameter about dash distance
        dashStartTime = Time.time;
        dashDirection = new Vector3(
                manager.currentMoveX != 0 ?
                    manager.currentMoveX :
                    (manager.isFacingRight ? 1f : -1f),
                0f, 0f
            ).normalized;

        // handle rigbody
        manager.rig.useGravity = false;
        manager.rig.linearVelocity = Vector3.zero;

        // handle collider
        originalHeight = manager.col.height;
        originalRadius = manager.col.radius;
        originalCenter = manager.col.center;

        manager.col.height = originalRadius * 2f; // modify collider size
        manager.col.center = originalCenter;

        // switch playerMesh and dashParticle
        manager.playerFace.SetActive(false);
        manager.PlayDashParticle();
    }

    public override void Update()
    {
        float timePassed = Time.time - dashStartTime;

        if (timePassed >= manager.dashDuration)
        {
            if (CanRestoreSize())
            {
                if (manager.isGrounded) manager.TransitionToState<IdleState>();
                else manager.TransitionToState<FallState>();
                return;
            }
            else
            {
                isForcedSliding = true;
                return;
            }
        }

        manager.rig.linearVelocity = dashDirection * manager.dashSpeed;
    }

    public override void FixedUpdate()
    {
        if (isForcedSliding)
        {
            manager.rig.linearVelocity = dashDirection * (manager.dashSpeed * 0.8f);
        }
        else
        {
            manager.rig.linearVelocity = dashDirection * manager.dashSpeed;
            HandleCornerCorrection();
        }
    }

    public override void Exit()
    {
        manager.rig.useGravity = true;
        manager.rig.linearVelocity = manager.rig.linearVelocity * manager.dashEndCut;

        manager.col.height = originalHeight;
        manager.col.radius = originalRadius;
        manager.col.center = originalCenter;

        manager.playerFace.SetActive(true);
        manager.StopDashParticle();
    }

    /// <summary>
    /// check current position can restore player size
    /// </summary>
    private bool CanRestoreSize()
    {
        Transform playerTrans = manager.transform;

        // calc scaled dimensions
        float scaleY = playerTrans.lossyScale.y;
        float scaleXZ = Mathf.Max(playerTrans.lossyScale.x, playerTrans.lossyScale.z);
        
        float requiredHeight = originalHeight * scaleY;
        float realRadius = (originalRadius * scaleXZ) * 0.9f;
        float centerDist = requiredHeight - (2f * realRadius);
        float clearance = 0.02f;

        Vector3 upDir = playerTrans.up;
        Vector3 worldCenter = playerTrans.TransformPoint(originalCenter);
        Vector3 smallCenter = playerTrans.TransformPoint(manager.col.center);

        // fast static check
        Vector3 pointOffset = upDir * (centerDist / 2f - clearance);
        if (!Physics.CheckCapsule(worldCenter - pointOffset, worldCenter + pointOffset, realRadius, manager.groundLayer, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        // measure vertical space
        float distUp = requiredHeight;
        if (Physics.SphereCast(smallCenter, realRadius, upDir, out RaycastHit hitUp, requiredHeight, manager.groundLayer, QueryTriggerInteraction.Ignore))
            distUp = hitUp.distance;

        float distDown = requiredHeight;
        if (Physics.SphereCast(smallCenter, realRadius, -upDir, out RaycastHit hitDown, requiredHeight, manager.groundLayer, QueryTriggerInteraction.Ignore))
            distDown = hitDown.distance;

        // check if space is enough
        if (distUp + distDown + (realRadius * 2f) >= requiredHeight + clearance)
        {
            // calc safe Y limits
            float minOffset = -distDown + clearance + (centerDist / 2f);
            float maxOffset = distUp - clearance - (centerDist / 2f);

            // clamp offset into safe zone
            float currentOffset = Vector3.Dot(worldCenter - smallCenter, upDir);
            float targetOffset = Mathf.Clamp(currentOffset, minOffset, maxOffset);

            // push player up/down if needed
            if (Mathf.Abs(targetOffset - currentOffset) > 0.001f)
            {
                manager.rig.position += upDir * (targetOffset - currentOffset);
            }
            return true;
        }

        return false; // space too small
    }

    /// <summary>
    /// if the player touches the corner of platform, allow them to climb onto it
    /// </summary>
    private void HandleCornerCorrection()
    {
        Bounds bounds = manager.col.bounds;

        // parameter from manager
        float range = manager.cornerCorrectionRange;
        float dashLookAhead = manager.dashLookAhead;
        LayerMask mask = manager.groundLayer;

        float radius = bounds.extents.x * 0.9f;
        float inset = radius + 0.05f;
        Vector3 p1 = new Vector3(bounds.center.x, bounds.max.y - inset, bounds.center.z);
        Vector3 p2 = new Vector3(bounds.center.x, bounds.min.y + inset, bounds.center.z);

        // detect front (ray from collider center)
        if (!Physics.CapsuleCast(p1, p2, radius, dashDirection, out RaycastHit wallHit, dashLookAhead, mask))
        {
            return; // no touch anything
        }

        // detect top
        Vector3 playerTop = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        if (Physics.Raycast(playerTop, Vector3.up, range, mask))
        {
            return; // blocked by ceiling
        }

        // handle main logic
        Vector3 probeOrigin = (playerTop + Vector3.up * range) + (dashDirection * (wallHit.distance + 0.05f));
        float probeDistance = range * 2f;

        if (Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit surfaceHit, probeDistance, mask))
        {
            if (surfaceHit.normal.y > 0.7f)
            {
                float halfHeight = bounds.extents.y;
                Vector3 targetPos = manager.rig.position;
                targetPos.y = surfaceHit.point.y + halfHeight;

                manager.rig.MovePosition(targetPos);
            }
        }
    }
}
