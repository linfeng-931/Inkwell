using UnityEngine;

public class Muk : EnemyControl
{
    public GameObject mud;
    public float attackDelayTime;

    private int attackStep;
    private float attackTimer;
    private bool canAttack;
    private Quaternion oriQuaternion;
    private Vector3 oriPosition;

    protected override void Start()
    {
        base.Start();
        attackStep = 0;
        canAttack = false;
    }

    protected override void Update()
    {
        base.Update();

        if((player.transform.position.x - transform.position.x) < 0)
        {
            dir = -1;
        }
        else
        {
            dir = 1;   
        }
        transform.localScale = new Vector3(-1*dir*scale, scale, scale);

        if (Vector3.Distance(player.transform.position, transform.position) < detectionRange)
        {
            isTracing = true;
        }
        Attack();

        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if(attackTimer>=attackDelayTime){
                canAttack = true;
                animator.SetInteger("action", 1);
                attackStep = 0;
                attackTimer = 0f;
            }
        }
    }

    protected override void Move()
    { }

    void Attack()
    {
        if (!isTracing || !canAttack) return;

        switch (attackStep)
        {
            case 1:
                Instantiate(mud, transform.position, new Quaternion(0,0,0,0));
                attackTimer = 0f;
                attackStep += 1;
                break;

            case 2:
                canAttack = false; 
                attackTimer = 0f;
                attackStep = 0;
                animator.SetInteger("action", 0);
                break;

            default:
                break;
        }
    }

    public void AddStep()
    {
        attackStep += 1;
    }
}
