using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Action Setting")]
    public bool direction;
    public float moveSpeed;
    public float currentMoveSpeed; //be used to control mapRotate
    public float moveInput;
    public bool isDash;
    public float jumpForce;
    public PlayerAni playerAni;
    public ParticleSystem footEffect;
    public float dashForce;
    public GameObject dashObj;
    public GameObject dashObj1;
    public GameObject dashObj2;
    public float attackStepDelay = 0.2f;
    public BoxCollider attackCollider;
    public GameObject attackPratical;
     public bool isHurt;

    [Header("Collision Setting")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkRadius = 0.3f;
    public bool isGrounded;

    [Header("Animator Control")]
    public Animator OtherAni;
    public Animator animator;
    public GameObject PlayerFace;

    [Header("Other Setting")]
    public GameObject drawPrefab;
    public float costOfDraw;
    public float gravityValue;
    public int drawKey;
    public PlayerStatus playerStatus;

    private float timer;
    private string action; //dash, jump, run, attack, skill, draw, idle
    private Rigidbody rb;

    //move
    private float accelerateSpeed = 80f;
    private float xScale;

    //jump
    private int jumpCount;
    private float jumpDelay; //use to ani control(jump to idle)
    private float jumpDelayTimer; //use to delay time of next jump

    //dash
    private float dashDelay;
    private float dashUnused;

    //attack
    private int attackStep;
    private bool canAttack;
    private float attackDelay;
    private float attackKeep;
    private bool isAttack;
    private Vector3 oriPos;
    private int previousAct;
    private bool skyAttack;

    //draw
    private bool isDrawing;
    private float drawTimer;

    //hurt
    private float hurtTimer;
    private int hurtType;

    //dead
    private bool isDead;
    private float deadTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        action = "action";
        timer = 0f;
        rb = GetComponent<Rigidbody>();
        currentMoveSpeed = 0f;
        direction = false;
        jumpCount = 0;
        isDrawing = false;
        jumpDelay = 0;
        isDash = false;
        dashDelay = 0f;
        attackStep = 0;
        attackDelay = 0;
        canAttack = true;
        attackKeep = 0;
        isAttack = false;
        drawTimer = 0f;
        hurtTimer = 0f;
        deadTimer = 0f;
        xScale = transform.localScale.x;
        skyAttack = true;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);
        Dead();
        if(isDead) return;

        if (isGrounded && !skyAttack)
        {
            skyAttack = true;
        }

        //cancel jump
        if (isGrounded && rb.linearVelocity.y < 0.1f && jumpCount!=0 && !isHurt)
        {
            jumpCount = 0;
            if((animator.GetInteger(action) == 2 ||animator.GetInteger(action) == 3) && jumpDelay>0.2f){
                playerAni.ResumeAni();
                if(rb.linearVelocity.x < 0.1f){
                    SwitchAni(0);
                    footEffect.Play();
                }
                else{
                    SwitchAni(1);
                    footEffect.Play();
                }
                jumpDelay = 0;
            }
        }

        //draw action
        if (Input.GetMouseButtonDown(drawKey))
        {
            isDrawing = true;
            playerStatus.isUsingEnergy = true;
            currentMoveSpeed = 0.09f;
            Instantiate(drawPrefab);
        }
        if (Input.GetMouseButtonUp(drawKey))
        {
            playerStatus.isUsingEnergy = false;
            isDrawing = false;
            drawTimer = 0f;
        }

        if (isDrawing)
        {
            drawTimer += Time.deltaTime;
            if (drawTimer >= 0.05f)
            {
                playerStatus.energy -= (int)(costOfDraw);
                drawTimer = 0f;
            } 
        }

        Attack();
        if(jumpDelayTimer<0.3f) jumpDelayTimer+=Time.deltaTime;

        if (isHurt)
        {
            if(hurtType == 0)
            {
                if(Vector3.Distance(oriPos, transform.position)>0.5f) rb.linearVelocity = new Vector3(0, 0, 0);
                hurtTimer+= Time.deltaTime;
                if(hurtTimer > 0.5f){
                    isHurt = false;
                    hurtTimer = 0f;
                }
            } 
            else if(hurtType == 1)
            {
                if(hurtTimer < 0.5f)
                {
                    hurtTimer+= Time.deltaTime;
                }
                else
                {
                    if(isGrounded){
                        hurtTimer = 0;
                        isHurt = false;
                    }
                }
            }
        }
    }
    void FixedUpdate()
    {
        //Gravity
        if (!isGrounded && !isDash)
        {
            rb.AddForce(Vector3.down * gravityValue * gravityValue * Time.deltaTime, ForceMode.Acceleration);
            jumpDelay+=Time.deltaTime;
            if(jumpCount == 0 && !isHurt && !isDead)
            {
                SwitchAni(8);
            }
        }
        else if(rb.linearVelocity.y < 0)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = -0.5f;
            rb.linearVelocity = vel;
        }

        if(isDead || isHurt) return;

        Move();
        Dash();
    }

    void OnDrawGizmos()
    {
        if(groundCheckPoint != null)
        {
            Gizmos.color = isGrounded? Color.green : Color.red;
            Gizmos.DrawSphere(groundCheckPoint.position, checkRadius);
        }
    }

    //action
    public void MoveAction(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<float>();
        
        if(isDead || isHurt) return;
        
    }
    void Move()
    {
        if(isDash || isAttack || isHurt) return;

        if(moveInput < 0 && direction){
            direction = false;
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
        else if(moveInput > 0 && !direction){
            direction = true;
            transform.localScale = new Vector3(-1f*xScale, transform.localScale.y, transform.localScale.z);
        }

        if(Math.Abs(moveInput) < 0.01f || isDrawing){
            if(jumpCount==0 && isGrounded) SwitchAni(0);

            if (currentMoveSpeed != 0f)
            {
                currentMoveSpeed -= accelerateSpeed * Time.deltaTime;
                if(currentMoveSpeed < 0f) currentMoveSpeed = 0f;
            }
            rb.linearVelocity = new Vector3(currentMoveSpeed*moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
            return;
        }

        if(currentMoveSpeed < moveSpeed)
        {
            currentMoveSpeed += accelerateSpeed * Time.deltaTime;
            if(currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;
        }
        if(jumpCount == 0 && isGrounded) SwitchAni(1);
        rb.linearVelocity = new Vector3(currentMoveSpeed*moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
        playerStatus.RaiseEnegry(currentMoveSpeed);
    }
    public void DashAction(InputAction.CallbackContext context)
    {
        if(isHurt || isDead) return;

        if(dashDelay!=0f) {
            dashUnused = 0f;
            return;
        }
        if(dashUnused <0.3f) return;

        isDash = true;
        playerAni.ResumeAni();
        OtherAni.SetInteger(action, 1);
        PlayerFace.SetActive(false);
        dashObj.SetActive(true);
        dashObj1.SetActive(true);
        dashObj2.SetActive(true);
        rb.linearVelocity = direction? new Vector3(dashForce, 0):new Vector3(-1*dashForce, 0);
    }
    void Dash()
    {
        if(!isDash){
            dashUnused += Time.deltaTime;
            return;
        }

        dashDelay+=Time.deltaTime;

        if(dashDelay > 0.4f)
        {
            OtherAni.SetInteger(action, 0);
        }
        if(dashDelay > 0.5f){
            isDash = false;
            dashDelay = 0f;
            PlayerFace.SetActive(true);
            dashObj.SetActive(false);
            dashObj1.SetActive(false);
            dashObj2.SetActive(false);
        }
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if(isDrawing || isDash || isHurt || isDead) return;

        if(jumpDelayTimer<0.25f) return;

        if (context.started && jumpCount <2)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            if(jumpCount == 1){
                jumpDelayTimer = 0f;
                footEffect.Play();
                SwitchAni(2);
            }
            else if(jumpCount == 2){
                jumpDelayTimer = 0f;
                footEffect.Play();
                playerAni.ResumeAni();
                SwitchAni(3);
            }
        }

        if (context.canceled)
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.25f);
            }
        }
    }
    void Attack()
    {
        if(isDash || isHurt) return;

        if (Input.GetMouseButtonDown(0) && !isDash && canAttack && attackStep!=3 && skyAttack)
        {
            int aniInt = animator.GetInteger(action);
            if(aniInt != 4 && aniInt != 5 && aniInt!=6) previousAct = aniInt;
            attackStep+=1;
            attackDelay = 0f;
            attackKeep = 0f;
            SwitchAni(3+attackStep);
            canAttack = false;
            isAttack = true;
            oriPos = transform.position;
            rb.linearVelocity = direction ? new Vector3(10f, 0, 0): new Vector3(-10f, 0, 0);
            attackCollider.enabled = true;
            attackPratical.SetActive(true);
        }
        if(!isAttack){
            attackCollider.enabled = false;
            attackPratical.SetActive(false);
            return;
        }
        if(Vector3.Distance(oriPos, transform.position)>0.2f) rb.linearVelocity = new Vector3(0, 0, 0);

        if(!isGrounded) playerAni.ResumeAni(); //跳躍時的攻擊問題

        //delay between steps
        if(attackDelay > attackStepDelay)
        {
            canAttack = true;
            attackDelay = 0f;
            if(attackStep == 3){
                attackStep = 0;
                if(!isGrounded) skyAttack = false;
            }
        }
        else
        {
            attackDelay += Time.deltaTime;
        }

        //cancel attack
        if(attackKeep > 0.5f)
        {
            isAttack = false;
            attackKeep = 0;
            attackStep = 0;
            if(!isGrounded) skyAttack = false;
            SwitchAni(previousAct);
        }
        else{
            attackKeep+=Time.deltaTime;
        }
        
    }
    void Draw()
    {
        
    }
    public void Hurt(int damage, int type, float x)
    {
        if(isHurt || isDash || isDead) return;

        isHurt = true;
        hurtTimer = 0f;
        hurtType = type;
        if(type == 0) SwitchAni(-1);
        if(type == 1) SwitchAni(7);
        oriPos = transform.position;
        if(type == 0){
            if(x != 0) rb.linearVelocity = x > 0 ? new Vector3(-10f, 0, 0): new Vector3(10f, 0, 0);
            else rb.linearVelocity = direction ? new Vector3(-10f, 0, 0): new Vector3(10f, 0, 0);
        }
        
        if(x > 0){
            direction = false;
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
        else{
            direction = true;
            transform.localScale = new Vector3(-1f*xScale, transform.localScale.y, transform.localScale.z);
        }

        playerStatus.blood -= damage;
    }

    public void Dead()
    {
        if(playerStatus.blood == 0)
        {
            isDead = true;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            SwitchAni(-2);
        }
        if(!isDead) return;

        deadTimer += Time.deltaTime;
        if(deadTimer > 2f)
        {
            Reset();
        }
    }

    public void SwitchAni(int act)
    {
        if (animator.GetInteger(action) == act) return;

        animator.SetInteger(action, act);
    }

    void Reset()
    {
        playerStatus.blood = playerStatus.maxBlood;
        timer = 0f;
        currentMoveSpeed = 0f;
        direction = false;
        jumpCount = 0;
        isDrawing = false;
        jumpDelay = 0;
        isDash = false;
        dashDelay = 0f;
        attackStep = 0;
        attackDelay = 0;
        canAttack = true;
        attackKeep = 0;
        isAttack = false;
        drawTimer = 0f;
        hurtTimer = 0f;
        SwitchAni(0);
        isDead = false;
        skyAttack = true;
        deadTimer = 0f;
        transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
    }
}
