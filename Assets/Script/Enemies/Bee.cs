using UnityEngine;

public class Bee : EnemyControl
{
    public float attackDelay;
    public GameObject needle;

    private float delayMoveTime;
    private float delayMoveTimer;
    private Vector3 moveStartPos;
    
    private GameObject currentNeedle;   
    private bool canAttack;
    private float attackTimer;
    private bool createNeedle;

    protected override void Start()
    {
        base.Start();
        delayMoveTime = 1.5f;
        delayMoveTimer = 0f;
        isMoving = false;
        isTracing = false;
        canAttack = false;
        attackTimer = 0f;
        createNeedle = true;
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
        Attack();
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
            if (Vector3.Distance(player.transform.position, transform.position) > detectionRange * 0.7f)
            {
                Vector3 toTarget = ((player.transform.position+new Vector3(0, 1f, 0)) - transform.position).normalized;
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
    void Attack()
    {
        if(!isTracing || !canAttack)
        {
            if(attackTimer <= attackDelay && !canAttack)
            {
                attackTimer += Time.deltaTime;
                if(attackTimer >= attackDelay * 0.5f && createNeedle)
                {
                    currentNeedle = Instantiate(needle, transform, false);
                    currentNeedle.transform.localPosition = new Vector3(0, -0.63f, 0);
                    createNeedle = false;
                }
            }
            else if(!canAttack)
            {
                canAttack = true;  
                attackTimer = 0f;
            }
            
            return;
        }

        currentNeedle.GetComponent<Needle>().Shoot();
        createNeedle = true;
        canAttack = false;
    }
}
