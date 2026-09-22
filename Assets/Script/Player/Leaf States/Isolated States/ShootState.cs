using UnityEngine;

public class ShootState : PlayerState
{
    private float stateTimer;

    public ShootState(PlayerController manager) : base(manager){}

    public override void Enter()
    {
        base.Enter();
        stateTimer = 0f;

        manager.rig.linearVelocity = new Vector3(0f, manager.rig.linearVelocity.y, 0f);
        
        manager.animator.Play(PlayerAnimateHash.Idle, 0, 0f);
        manager.canTurn = false;
        CombatFeedbackManager.Instance.TriggerHitFeedback(0.06f, 1.5f);

        Vector3 shootDirection = manager.GetMouseDirection();
        manager.FaceTowards(manager.transform.position + shootDirection);
        Bullet bullet = Object.Instantiate(
                manager.bulletPrefab, 
                manager.bulletSpawnPoint.position, 
                Quaternion.identity
            ).GetComponent<Bullet>();

        bullet.Initialize(shootDirection, 0, 2);
    }

    public override void Update()
    {
        base.Update();
        stateTimer += Time.deltaTime;

        // end shoot state
        if(stateTimer >= manager.shootDuration)
        {
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

    public override void Exit()
    {
        base.Exit();
        manager.canTurn = true;
    }
}
