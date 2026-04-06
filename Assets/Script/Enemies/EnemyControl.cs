using UnityEngine;

public abstract class EnemyControl : MonoBehaviour
{
    public int damage;
    public float blood;
    public int speed;
    public bool canRepel;
    public float moveRange;
    public float idleRange;
    public float detectionRange;

    protected GameObject player;
    protected Vector3 startPos;
    protected bool isDead;
    protected Rigidbody rig;
    protected bool isMoving;
    protected bool isTracing;

    private PlayerController playerController;
    private bool flag;
    private Animator animator;
    private int dir;
    private float repelSpeed = 30f;
    private bool isHurt;
    private float hurtTimer;


    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        isDead = false;
        flag = true;
        rig = GetComponent<Rigidbody>();
        dir = 1;
        hurtTimer = 0f;
        isHurt = false;
        startPos = transform.position;
        //animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if(isDead) return;
        print(true);
        //update direction
        if(rig.linearVelocity.y > 0) dir = 1;
        else if(rig.linearVelocity.y < 0) dir = -1;

        //hurtDelay
        if (isHurt)
        {
            hurtTimer+=Time.deltaTime;
            if(hurtTimer > 0.2f)
            {
                hurtTimer = 0f;
                isHurt = false;
                rig.linearVelocity = new Vector3(0, 0, 0);
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerController.Hurt(damage);
        }
        else if (collision.gameObject.CompareTag("Weapon"))
        {
            if(!isHurt) Hurt(player.GetComponent<PlayerStatus>().damage);
        }
        else
        {
            StopEnemy();
        }
    }

    protected void Hurt(float damage)
    {
        isHurt = true;
        blood -= damage;
        if(blood <= 0){
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Body");
            GetComponent<Collider>().isTrigger = false;
            rig.useGravity = true;
            rig.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }

        if (canRepel)
        {
            rig.linearVelocity = new Vector3(repelSpeed*dir, 0, 0);
        }
    }

    public void DeadAni() //在動畫最後一偵加入此function
    {
        flag = false;
    }

    protected void StopEnemy()
    {
        isMoving = false; 
        rig.linearVelocity = Vector3.zero;
    }

    //需複寫項
    abstract protected void Move();
}
