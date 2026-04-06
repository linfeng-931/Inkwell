using UnityEngine;

public class Bee : EnemyControl
{
    private float delayMoveTime;
    private float delayMoveTimer;
    private Vector3 moveStartPos;

    protected override void Start()
    {
        base.Start();
        delayMoveTime = 1.5f;
        delayMoveTimer = 0f;
        isMoving = false;
        isTracing = false;
    }

    protected override void Update()
    {
        base.Update();

        if(isDead){
            isMoving = false; 
            rig.linearVelocity = new Vector3(0, rig.linearVelocity.y, 0);
            return;
        }

        Move();
        if (Vector3.Distance(player.transform.position, transform.position) <= detectionRange)
        {
            isTracing = true;
        }
        else
        {
            isTracing = false;
        }
    }

    protected override void Move()
    {
        //防撞
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rig.linearVelocity.normalized, out hit, 0.5f))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                StopEnemy();
            }
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
                    Vector3 Direction;

                    if (Vector3.Distance(transform.position, startPos) >= idleRange)
                    {
                        Direction = (startPos - transform.position).normalized;
                    }
                    else
                    {
                        Vector2 random2D = Random.insideUnitCircle.normalized;
                        float finalY = random2D.y;
                        //需防止撞地板

                        Direction = new Vector3(random2D.x, finalY, 0);
                    }


                    rig.linearVelocity = speed * Direction;
                    moveStartPos = transform.position;
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, moveStartPos) >= moveRange)
                {
                    isMoving = false;
                    rig.linearVelocity = new Vector3(0, 0, 0);
                }
            }
        }
        else
        {
            if (Vector3.Distance(player.transform.position, transform.position) > detectionRange * 0.5f)
            {
                Vector3 toTarget = (player.transform.position - transform.position).normalized;
                rig.linearVelocity = speed * 0.2f * toTarget;
            }
            else
            {
                if (!isMoving)
                {
                    delayMoveTimer += Time.deltaTime;
                    if (delayMoveTimer >= delayMoveTime)
                    {
                        delayMoveTimer = 0f;
                        isMoving = true;
                        Vector3 Direction;

                        if (Vector3.Distance(transform.position, startPos) >= idleRange)
                        {
                            Direction = (startPos - transform.position).normalized;
                        }
                        else
                        {
                            Vector2 random2D = Random.insideUnitCircle.normalized;
                            float finalY = random2D.y;
                            //需防止撞地板

                            Direction = new Vector3(random2D.x, finalY, 0);
                        }


                        rig.linearVelocity = speed * Direction;
                        moveStartPos = transform.position;
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, moveStartPos) >= moveRange)
                    {
                        isMoving = false;
                        rig.linearVelocity = new Vector3(0, 0, 0);
                    }
                }
            }

        }

    }
}
