using UnityEngine;

public class Yak : EnemyControl
{
    private int attackStep;
    private float attackTimer;
    private float attackDelayTime;
    private float stepOneTime;
    private float stepThreeTime;
    private Vector3 moveStartPos;
    private Vector3 attackTarget;
    private Vector3 Direction;

    protected override void Start()
    {
        base.Start();
        delayMoveTime = 2f;
        attackStep = 0;
        attackTimer = 0f;
        attackDelayTime = 2f;
        stepOneTime = 0.7f;
        stepThreeTime = 0.7f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(isDead) return;
        
        if (Vector3.Distance(player.transform.position, transform.position) <= detectionRange)
        {
            if(!isTracing) rig.linearVelocity = new Vector3(0, 0, 0);
            isTracing = true;
        }
        else
        {
            isTracing = false;
        }
        Move();
    }

    protected override void Move()
    {
        if (attackStep != 0) 
        {
            Attack();
            return;
        }

        if (!isTracing)
        {
            if (!isMoving)
            {
                delayMoveTimer += Time.deltaTime;
                if (delayMoveTimer >= delayMoveTime)
                {
                    delayMoveTimer = 0f;
                    isMoving = true;
                    Direction = dir == 1 ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
                    dir *= -1;
                    rig.linearVelocity = speed * Direction;
                    moveStartPos = transform.position;
                    isMoving = true;
                }
            }
            else
            {
                rig.linearVelocity = speed * Direction;
                if (Vector3.Distance(transform.position, moveStartPos) >= idleRange)
                {
                    isMoving = false;
                    rig.linearVelocity = new Vector3(0, 0, 0);
                }
            }
        }
        else
        {
            if (Vector3.Distance(player.transform.position, transform.position) < detectionRange * 0.7f)
            {
                Attack();
            }
            else
            {
                Vector3 Direction = (player.transform.position - transform.position).normalized;
                Vector3 finalTarget = new Vector3(Direction.x, 0, Direction.z);
                rig.linearVelocity = finalTarget * speed * 1.2f;
                isMoving = true;
            }
        }
    }
    void Attack()
    {
        switch (attackStep)
        {
            case 0:
                rig.linearVelocity = new Vector3(0, 0, 0);
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackDelayTime)
                {
                    attackTimer = 0f;
                    attackStep += 1;
                }
                return;

            case 1: //ready
                attackTimer += Time.deltaTime;
                if (attackTimer >= stepOneTime)
                {
                    repelPlayer = true;
                    attackTimer = 0f;
                    attackStep += 1;
                    moveStartPos = transform.position;
                    attackTarget = (player.transform.position - transform.position).normalized;
                    attackTarget = new Vector3(attackTarget.x, 0, attackTarget.z);
                    rig.linearVelocity = speed * 5f * attackTarget;
                }
                break;

            case 2: //ing
                rig.linearVelocity = speed * 5f * attackTarget;
                if (Vector3.Distance(transform.position, moveStartPos) >= moveRange * 1.8f)
                {
                    isMoving = false;
                    rig.linearVelocity = new Vector3(0, 0, 0);
                    attackStep += 1;
                }
                break;

            case 3: //stop
                attackTimer += Time.deltaTime;
                repelPlayer = false;
                if (attackTimer >= stepThreeTime)
                {
                    attackTimer = 0f;
                    attackStep = 0;
                }
                break;
            default:
                break;
        }
    }
}
