using System;
using UnityEngine;

public abstract class EnemyControl : MonoBehaviour
{
    public int damage;
    public float blood;
    public int speed;
    public bool canRepel;
    public bool repelPlayer;
    public float moveRange;
    public float idleRange;
    public float detectionRange;
    public GameObject[] otherCollider;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkRadius = 0.5f;

    protected bool isGrounded;
    protected GameObject player;
    protected Vector3 startPos;
    protected bool isDead;
    protected Rigidbody rig;
    protected bool isMoving;
    protected bool isTracing;
    protected float delayMoveTime;
    protected float delayMoveTimer;
    protected int dir;
    protected Animator animator;
    protected float scale;
    protected bool disDir;

    private PlayerController playerController;
    private bool flag;
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
        isMoving = false;
        isTracing = false;
        delayMoveTimer = 0f;
        animator = GetComponent<Animator>();
        scale = Math.Abs(transform.localScale.x);
        isGrounded = false;
        disDir = false;
    }

    protected virtual void Update()
    {
        if(isDead) return;

        if(groundCheckPoint != null) isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);

        //update direction
        if (!disDir)
        {
            if(rig.linearVelocity.y > 0){
                dir = 1;
            }
            else if(rig.linearVelocity.y < 0){
                dir = -1;
            }
        }
    
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
            if(repelPlayer){
                if(playerController.isHurt) return;
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                playerRb.linearVelocity = Vector3.zero;
                Vector3 forceDir = rig.linearVelocity.x>0 ? new Vector3(1, 1, 0) : new Vector3(-1, 1, 0);
                playerRb.AddForce(forceDir * 4f, ForceMode.Impulse);
                playerController.Hurt(damage, 1, rig.linearVelocity.x);
            }
            else
            {
                playerController.Hurt(damage, 0, rig.linearVelocity.x);
            }
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
        print("Hurt");
        if(blood <= 0){
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Body");
            if(otherCollider != null)
            {
                foreach (var item in otherCollider)
                {
                    item.layer = LayerMask.NameToLayer("Body");
                }
            }
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
