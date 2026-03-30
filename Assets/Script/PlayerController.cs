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
    public Animator animator;
    public PlayerAni playerAni;
    public ParticleSystem footEffect;
    public float dashForce;
    public GameObject dashObj;
    public GameObject PlayerFace;
    public float attackStepDelay = 0.2f;

    [Header("Collision Setting")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkRadius = 0.3f;
    public bool isGrounded;

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

    //draw
    private bool isDrawing;
    private float drawTimer;

    //hurt
    private bool isHurt;
    private float hurtTimer;


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
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);

        //cancel jump
        if (isGrounded && rb.linearVelocity.y < 0.1f && jumpCount!=0)
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
            if(Vector3.Distance(oriPos, transform.position)>0.5f) rb.linearVelocity = new Vector3(0, 0, 0);
            hurtTimer+= Time.deltaTime;
            if(hurtTimer > 0.5f){
                isHurt = false;
                hurtTimer = 0f;
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
        }
        else if(rb.linearVelocity.y < 0)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = -0.5f;
            rb.linearVelocity = vel;
        }

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
        if(moveInput < 0 && direction){
            direction = false;
            transform.localScale = new Vector3(-1f*transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if(moveInput > 0 && !direction){
            direction = true;
            transform.localScale = new Vector3(-1f*transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    void Move()
    {
        if(isDash || isAttack || isHurt) return;
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
        if(jumpCount == 0) SwitchAni(1);
        rb.linearVelocity = new Vector3(currentMoveSpeed*moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
        playerStatus.RaiseEnegry(currentMoveSpeed);
    }
    public void DashAction(InputAction.CallbackContext context)
    {
        if(isHurt) return;

        if(dashDelay!=0f) {
            dashUnused = 0f;
            return;
        }
        if(dashUnused <0.3f) return;

        isDash = true;
        playerAni.ResumeAni();
        dashObj.SetActive(true);
        PlayerFace.SetActive(false);
        rb.linearVelocity = direction? new Vector3(dashForce, 0):new Vector3(-1*dashForce, 0);
    }
    void Dash()
    {
        if(!isDash){
            dashUnused += Time.deltaTime;
            return;
        }

        dashDelay+=Time.deltaTime;
        if(dashDelay > 0.5f){
            isDash = false;
            dashDelay = 0f;
            dashObj.SetActive(false);
            PlayerFace.SetActive(true);
        }
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if(isDrawing || isDash || isHurt) return;

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

        if (Input.GetMouseButtonDown(0) && !isDash && canAttack && attackStep!=3)
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
        }
        if(!isAttack) return;
        if(Vector3.Distance(oriPos, transform.position)>0.2f) rb.linearVelocity = new Vector3(0, 0, 0);

        if(!isGrounded) playerAni.ResumeAni(); //跳躍時的攻擊問題

        //delay between steps
        if(attackDelay > attackStepDelay)
        {
            canAttack = true;
            attackDelay = 0f;
            if(attackStep == 3) attackStep = 0;
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
            SwitchAni(previousAct);
        }
        else{
            attackKeep+=Time.deltaTime;
        }
        
    }
    void Draw()
    {
        
    }
    public void Hurt(int damage)
    {
        if(isHurt) return;
        isHurt = true;
        oriPos = transform.position;
        rb.linearVelocity = direction ? new Vector3(-10f, 0, 0): new Vector3(10f, 0, 0);
        playerStatus.blood -= damage;
    }

    void SwitchAni(int act)
    {
        animator.SetInteger("action", act);
    }
}
