using UnityEngine;

public class RollState : PlayerState
{
    private float startTime;
    private int direction;
    private bool isEnding;
    private float rollEndTimer;
    private Transform visualTransform;

    public RollState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();

        manager.animator.Play(PlayerAnimateHash.Roll, 0, 0f);
        startTime = Time.time;
        direction = manager.isFacingRight ? 1 : -1;
        isEnding = false;
        rollEndTimer = 0f;

        visualTransform = manager.playerFace.transform;
        var main = manager.rollParticle.main;
        main.startSpeed = manager.isFacingRight ? new ParticleSystem.MinMaxCurve(-7f, -10f) : new ParticleSystem.MinMaxCurve(7f, 10f);
        manager.rollParticle.Play();
    }

    public override void Update()
    {
        base.Update();

        if (!isEnding)
        {
            float elapsed = Time.time - startTime;
            float rotateSpeed = (360f / manager.rollDuration) * Time.deltaTime * direction * -1f;

            visualTransform.Rotate(0f, 0f, rotateSpeed);
            if(elapsed >= manager.rollDuration)
            {
                isEnding = true;
                visualTransform.localRotation = Quaternion.identity;
                manager.animator.Play(PlayerAnimateHash.RollEnd, 0, 0f);
            }
        }
        else
        {
            rollEndTimer += Time.deltaTime;
            if(rollEndTimer >= manager.rollEndDuration)
            {
                if(Mathf.Abs(manager.currentMoveX) > 0.01f)
                {
                    manager.TransitionToState<RunState>();
                }
                else
                {
                    manager.TransitionToState<IdleState>();
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!isEnding)
        {
            manager.rig.linearVelocity = new Vector3(direction * manager.rollSpeed, manager.rig.linearVelocity.y, 0f);
        }
        else
        {
            float t = rollEndTimer / manager.rollEndDuration;
            float smoothSpeedX = Mathf.Lerp(direction * manager.rollSpeed, 0f, t);
            
            manager.rig.linearVelocity = new Vector3(smoothSpeedX, manager.rig.linearVelocity.y, 0f);
        }
    }
        

    public override void Exit()
    {
        base.Exit();
        visualTransform.localRotation = Quaternion.identity;
        manager.rollParticle.Stop();
    }
}
