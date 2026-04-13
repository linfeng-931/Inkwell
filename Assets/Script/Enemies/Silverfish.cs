using UnityEngine;

public class Silverfish : EnemyControl
{
    public GameObject attackCollider;

    private int attackStep;
    private float attackTimer;
    private float attackDelayTime;
    private Vector3 moveStartPos;
    private Vector3 Direction;

    protected override void Start()
    {
        base.Start();
        delayMoveTime = 1f;
        attackStep = 0;
        attackTimer = 0f;
        attackDelayTime = 1f;
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

        transform.localScale = new Vector3(dir*scale, scale, scale);
        if (!isTracing)
        {
            transform.localScale = new Vector3(dir*scale, scale, scale);
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
                    animator.SetInteger("action", 1);
                }
            }
            else
            {
                rig.linearVelocity = speed * Direction;
                if (Vector3.Distance(transform.position, moveStartPos) >= idleRange)
                {
                    isMoving = false;
                    rig.linearVelocity = new Vector3(0, 0, 0);
                    animator.SetInteger("action", 0);
                }
            }
        }
        else
        {
            if (Vector3.Distance(player.transform.position, transform.position) < 0.6f)
            {
                Attack();
            }
            else
            {
                Vector3 Direction = (player.transform.position - transform.position).normalized;
                Vector3 finalTarget = new Vector3(Direction.x, 0, Direction.z);
                rig.linearVelocity = finalTarget * speed;
                isMoving = true;
                animator.SetInteger("action", 1);
                transform.localScale = new Vector3(-1*finalTarget.x*scale, scale, scale);
                dir = (-1*finalTarget.x) > 0? 1 : -1;
            }
        }
    }
    void Attack()
    {
        switch (attackStep)
        {
            case 0:
                rig.linearVelocity = new Vector3(0, 0, 0);
                Vector3 Direction = (player.transform.position - transform.position).normalized;
                transform.localScale = Direction.x > 0 ? new Vector3(-1*scale, scale, scale): new Vector3(scale, scale, scale);
                dir = (-1*Direction.x) > 0? 1 : -1;
                animator.SetInteger("action", 0);
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackDelayTime)
                {
                    attackTimer = 0f;
                    attackStep += 1;
                    animator.SetInteger("action", 2);
                }
                return;
            default:
                break;
        }
    }

    public void Ready()
    {
        attackTimer = 0f;
        attackCollider.SetActive(true);
    }
    public void Ready_End()
    {
        animator.SetInteger("action", 0);
    }
    public void Stop()
    {
        attackTimer = 0f;
        attackStep = 0;
    }
    public void DisCollider()
    {
        attackCollider.SetActive(false);    
    }
}

