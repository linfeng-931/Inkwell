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
    public GameObject[] otherObj;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkRadius = 0.5f;
    public ParticleSystem particleHurt;
    public GameObject particleDead;

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
    protected bool isHurt;

    private PlayerController playerController;
    private bool flag;
    private float repelSpeed = 30f;
    private float hurtTimer;
    private Vector3 oriPos;


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
    }

    protected virtual void Update()
    {
        if(isDead) return;

        if(groundCheckPoint != null){
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);
            if (!isGrounded && rig.linearVelocity != Vector3.zero)
            {
                isMoving = false;
                rig.linearVelocity = new Vector3(0, 0, 0);
                animator.SetInteger("action", 0);
                isTracing = false;
            }
        }

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
            if(oriPos != null)
            {
                if(Vector3.Distance(oriPos, transform.position) > 1f)
                {
                    hurtTimer = 0f;
                    isHurt = false;
                    rig.linearVelocity = new Vector3(0, 0, 0);
                }
            }
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
                float x = transform.position.x - player.transform.position.x;
                Vector3 forceDir = x<0 ? new Vector3(1, 1, 0) : new Vector3(-1, 1, 0);
                playerRb.AddForce(forceDir * 4f, ForceMode.Impulse);
                playerController.Hurt(damage, 1, transform.position.x - player.transform.position.x);
            }
            else
            {
                playerController.Hurt(damage, 0, transform.position.x - player.transform.position.x);
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
        particleHurt.Play();

        if(blood <= 0){
            isDead = true;
            particleDead.SetActive(true);
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.layer = LayerMask.NameToLayer("Body");
            if(otherObj != null)
            {
                foreach (var item in otherObj)
                {
                    item.layer = LayerMask.NameToLayer("Body");
                    if(item.GetComponent<SpriteRenderer>() != null) item.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            GetComponent<Collider>().isTrigger = false;
            rig.useGravity = true;
            rig.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }

        if (canRepel && blood > 0)
        {
            oriPos = transform.position;
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
