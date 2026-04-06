using UnityEngine;

public class Muk : EnemyControl
{
    public Transform axis;
    public Transform head;
    public GameObject mud;

    private int attackStep;
    private float attackTimer;
    private bool canAttack;
    private Quaternion oriQuaternion;
    private Vector3 oriPosition;

    protected override void Start()
    {
        base.Start();
        attackStep = 0;
        oriQuaternion = head.transform.localRotation;
        oriPosition = head.transform.localPosition;
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

        if (Vector3.Distance(player.transform.position, transform.position) < detectionRange)
        {
            isTracing = true;
        }
        Attack();

        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if(attackTimer>=0.5f){
                canAttack = true;
                attackTimer = 0f;
            }
        }
    }

    protected override void Move()
    { }

    void Attack()
    {
        if (!isTracing || !canAttack) return;

        Vector3 currentAngles = transform.localEulerAngles;
        currentAngles.y = dir == 1? 180f:0f; 
        transform.localEulerAngles = currentAngles;

        Vector3 relativeBack = transform.TransformDirection(Vector3.back);
        Vector3 relativeForward = transform.TransformDirection(Vector3.forward);

        switch (attackStep)
        {
            case 0:
                head.RotateAround(axis.position, relativeBack, speed * Time.deltaTime);
                if (head.localEulerAngles.z <= 290f && head.localEulerAngles.z > 180f)
                {
                    head.localRotation = Quaternion.Euler(0, 0, 290f);
                    attackStep += 1;
                }
                break;

            case 1:
                attackTimer += Time.deltaTime;
                if(attackTimer >= 0.2f)
                {
                    Instantiate(mud, transform.position, new Quaternion(0,0,0,0));
                    attackTimer = 0f;
                    attackStep += 1;
                }
                break;

            case 2:
                head.RotateAround(axis.position, relativeForward, speed * Time.deltaTime);
                if (head.localEulerAngles.z > 355f || head.localEulerAngles.z <= 3f)
                {
                    head.localRotation = oriQuaternion;
                    head.localPosition = oriPosition;
                    canAttack = false; 
                    attackTimer = 0f;
                    attackStep = 0;
                }
                break;

            default:
                break;
        }
    }
}
