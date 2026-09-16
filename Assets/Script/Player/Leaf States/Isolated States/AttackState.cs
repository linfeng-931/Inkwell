using UnityEngine;

public class AttackState : PlayerState
{
    private AttackData currentAttack;
    private float stateStartTime;
    private bool inputRegistered;
    private bool hasEnabledHitBox;
    private HitBox hitBox;

    public AttackState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        if (manager.atkClip != null)
        {
            manager.audioManager.PlaySFX(manager.atkClip);
        }

        // get and set attack data
        currentAttack = manager.currentComboList[manager.currentComboIndex];
        stateStartTime = Time.time;
        inputRegistered = false;

        // animate
        manager.animator.Play(currentAttack.animationName, 0, 0f);

        // close gravity
        if (!manager.isGrounded)
        {
            manager.rig.linearVelocity = new Vector3(manager.rig.linearVelocity.x, 0f, 0f);
            manager.rig.useGravity = false;
        }

        // hit box setting
        hasEnabledHitBox = false;
        hitBox = manager.GetComponentInChildren<HitBox>();
        hitBox.DisableHitBox();
    }

    public override void Update()
    {
        base.Update();

        float elapsedTime = Time.time - stateStartTime;

        // hit box and damage setting
        if(elapsedTime >= currentAttack.activeHitBoxStartTime && elapsedTime <= currentAttack.activeHitBoxEndTime)
        {
            if (!hasEnabledHitBox)
            {
                hitBox.EnableHitBox(manager.damage, currentAttack.hitboxSize, currentAttack.hitboxOffset);
                hasEnabledHitBox = true;
            }
        }
        else if (elapsedTime > currentAttack.activeHitBoxEndTime && hasEnabledHitBox)
        {
            hitBox.DisableHitBox();
            hasEnabledHitBox = false;
        }

        // move and combo setting
        float currentY = manager.isGrounded ? manager.rig.linearVelocity.y : 0f;

        // move when attack        
        if(elapsedTime <= currentAttack.thrustDuration)
        {
            float dir = manager.isFacingRight ? 1f: -1f;
            manager.rig.linearVelocity = new Vector3(dir * currentAttack.forwardThrust, currentY, 0f);
        }
        else
        {
            manager.rig.linearVelocity = new Vector3(0f, currentY, 0f);
        }

        // check time
        if(elapsedTime >= currentAttack.comboStartTime && elapsedTime <= currentAttack.comboEndTime)
        {
            if (manager.inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Attack))
            {
                // enter next attack step
                if(manager.currentComboIndex < manager.currentComboList.Length - 1)
                {
                    manager.inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Attack);
                    inputRegistered = true;
                }
            }
        }

        // end attack
        if(elapsedTime >= currentAttack.duration)
        {
            if (inputRegistered)
            {
                manager.currentComboIndex++;
                manager.TransitionToState<AttackState>();
            }
            else
            {
                ReturnToNormalState();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        manager.rig.useGravity = true;
    }

    private void ReturnToNormalState()
    {
        manager.currentComboIndex = 0;
        if (manager.isGrounded)
            manager.TransitionToState<IdleState>();
        else
        {
            manager.canAirAttack = false;
            manager.TransitionToState<FallState>();
        }
    }
}
