using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Action Setting")]
    public PlayerAni playerAni;

    //move
    public bool direction;
    public float moveSpeed;
    public float currentMoveSpeed; //be used to control mapRotate
    public float moveInput;

    //jump
    public float jumpForce;
    public ParticleSystem jumpEffect0;
    public ParticleSystem jumpEffect1;
    public ParticleSystem dropEffect;

    //dash
    public bool isDash;
    public float dashForce;
    public float dashTime;
    public GameObject dashObj;

    //attack
    public float attackStepDelay = 0.1f;
    public BoxCollider attackCollider;
    public GameObject attackPratical;
    public AttackEffect attackEffect;
    public bool isHurt;

    [Header("Collision Setting")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public LayerMask otherGroundLayer;
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
    public GameObject swirlParticle;
    public bool isStory;
    public bool isStory2;
    public bool isInteract;
    public bool isGoTarget;

    private float timer;
    private string action; //dash, jump, run, attack, skill, draw, idle
    private Rigidbody rb;
    private int combinedLayerMask;

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
    private Transform mapParent;

    //hurt
    private float hurtTimer;
    private int hurtType;

    //dead
    private bool isDead;
    private float deadTimer;

    //swirl
    private bool isSwirl;

    //interaction
    private Vector3 interactTarget;
    private int toTargetType;

    // Puzzle
    public static bool isPuzzleActive = false;

    // Camera Shake
    public CameraShake camShake;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        action = "action";
        timer = 0f;
        rb = GetComponent<Rigidbody>();
        currentMoveSpeed = 0f;
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
        direction = true;
        isGoTarget = false;
        transform.localScale = new Vector3(-1f * xScale, transform.localScale.y, transform.localScale.z);
        if (GameObject.FindGameObjectWithTag("Map") != null) mapParent = GameObject.FindGameObjectWithTag("Map").transform;
        else mapParent = null;
        combinedLayerMask = groundLayer | otherGroundLayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPuzzleActive) return;
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, combinedLayerMask);
        Dead();
        GoTarget();
        if (isDead || isStory || isStory2 || isInteract) return;

        if (isSwirl)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                swirlParticle.SetActive(false);
                isSwirl = false;
                PlayerFace.SetActive(true);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                SwitchAni(2);
            }
            else return;
        }

        if (isGrounded && !skyAttack)
        {
            skyAttack = true;
        }

        //cancel jump
        if (isGrounded && rb.linearVelocity.y < 0.1f && jumpCount != 0 && !isHurt)
        {
            jumpCount = 0;
            if ((animator.GetInteger(action) == 2 || animator.GetInteger(action) == 3) && jumpDelay > 0.2f)
            {
                playerAni.ResumeAni();
                if (rb.linearVelocity.x < 0.1f)
                {
                    SwitchAni(0);
                    dropEffect.Play();
                }
                else
                {
                    SwitchAni(1);
                    dropEffect.Play();
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
            if (mapParent != null)
            {
                GameObject newObj = Instantiate(drawPrefab);
                newObj.transform.SetParent(mapParent, true);
                newObj.transform.localScale = new Vector3(
                    1f / mapParent.lossyScale.x,
                    1f / mapParent.lossyScale.y,
                    1f / mapParent.lossyScale.z
                );
            }
            else Instantiate(drawPrefab);
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
        if (jumpDelayTimer < 0.3f) jumpDelayTimer += Time.deltaTime;

        if (isHurt)
        {
            if (hurtType == 0)
            {
                if (Vector3.Distance(oriPos, transform.position) > 0.5f) rb.linearVelocity = new Vector3(0, 0, 0);
                hurtTimer += Time.deltaTime;
                if (hurtTimer > 0.5f)
                {
                    isHurt = false;
                    hurtTimer = 0f;
                }
            }
            else if (hurtType == 1)
            {
                if (hurtTimer < 0.5f)
                {
                    hurtTimer += Time.deltaTime;
                }
                else
                {
                    if (isGrounded)
                    {
                        hurtTimer = 0;
                        isHurt = false;
                    }
                }
            }
        }
    }
    void FixedUpdate()
    {
        if (isPuzzleActive) return;
        if (isSwirl)
        {
            return;
        }

        //Gravity
        if (!isGrounded && !isDash)
        {
            rb.AddForce(Vector3.down * gravityValue * gravityValue * Time.deltaTime, ForceMode.Acceleration);
            jumpDelay += Time.deltaTime;
            if (jumpCount == 0 && !isHurt && !isDead)
            {
                if (!isGoTarget) SwitchAni(8);
            }
        }
        else if (rb.linearVelocity.y < 0)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = -0.5f;
            rb.linearVelocity = vel;
        }

        if (isDead || isHurt || isInteract) return;

        Move();
        Walk();
        Dash();
    }

    void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawSphere(groundCheckPoint.position, checkRadius);
        }
    }

    //Collider/ Trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Swirl"))
        {
            jumpCount = 0;
            rb.linearVelocity = new Vector3(0, 0, 0);
            isSwirl = true;
            Vector3 pos = other.gameObject.transform.position;
            pos.z = transform.position.z;
            transform.position = pos;
            swirlParticle.SetActive(true);
            PlayerFace.SetActive(false);
        }
    }

    //action
    public void MoveAction(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<float>();

        if (isDead || isHurt) return;

    }
    void Move()
    {
        if (isDash || isAttack || isHurt || isStory || isInteract) return;

        if (moveInput < 0 && direction)
        {
            direction = false;
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput > 0 && !direction)
        {
            direction = true;
            transform.localScale = new Vector3(-1f * xScale, transform.localScale.y, transform.localScale.z);
        }

        if (Math.Abs(moveInput) < 0.01f || isDrawing)
        {
            if (jumpCount == 0 && isGrounded) SwitchAni(0);

            if (currentMoveSpeed != 0f)
            {
                currentMoveSpeed -= accelerateSpeed * Time.deltaTime;
                if (currentMoveSpeed < 0f) currentMoveSpeed = 0f;
            }
            rb.linearVelocity = new Vector3(currentMoveSpeed * moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
            return;
        }

        if (currentMoveSpeed < moveSpeed)
        {
            currentMoveSpeed += accelerateSpeed * Time.deltaTime;
            if (currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;
        }
        if (jumpCount == 0 && isGrounded) SwitchAni(1);
        rb.linearVelocity = new Vector3(currentMoveSpeed * moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
        playerStatus.RaiseEnegry(currentMoveSpeed);
    }
    void Walk()
    {
        if (!isStory) return;
        if (moveInput < 0 && direction)
        {
            direction = false;
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput > 0 && !direction)
        {
            direction = true;
            transform.localScale = new Vector3(-1f * xScale, transform.localScale.y, transform.localScale.z);
        }

        if (Math.Abs(moveInput) < 0.01f || isDrawing)
        {
            if (jumpCount == 0 && isGrounded) SwitchAni(0);

            if (currentMoveSpeed != 0f)
            {
                currentMoveSpeed -= accelerateSpeed * Time.deltaTime;
                if (currentMoveSpeed < 0f) currentMoveSpeed = 0f;
            }
            rb.linearVelocity = new Vector3(currentMoveSpeed * moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
            return;
        }

        if (currentMoveSpeed < moveSpeed * 0.4f)
        {
            currentMoveSpeed += accelerateSpeed * Time.deltaTime;
            if (currentMoveSpeed > moveSpeed * 0.4f) currentMoveSpeed = moveSpeed * 0.4f;
        }
        if (jumpCount == 0 && isGrounded) SwitchAni(11);
        rb.linearVelocity = new Vector3(currentMoveSpeed * moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
        playerStatus.RaiseEnegry(currentMoveSpeed);
    }
    public void DashAction(InputAction.CallbackContext context)
    {
        if (isHurt || isDead || isStory || isStory2 || isInteract) return;

        if (dashDelay != 0f)
        {
            dashUnused = 0f;
            return;
        }
        if (dashUnused < 0.3f) return;


        isDash = true;
        playerAni.ResumeAni();
        OtherAni.SetInteger(action, 1);
        PlayerFace.SetActive(false);
        dashObj.SetActive(true);
        rb.linearVelocity = direction ? new Vector3(dashForce, 0) : new Vector3(-1 * dashForce, 0);
    }
    void Dash()
    {
        if (!isDash)
        {
            if (dashUnused < 0.3f)
            {
                dashUnused += Time.deltaTime;
            }
            else
            {
                dashObj.SetActive(false);
            }
            return;
        }

        dashDelay += Time.deltaTime;

        if (dashDelay > dashTime - 0.1f)
        {
            OtherAni.SetInteger(action, 0);
        }
        if (dashDelay > dashTime)
        {
            isDash = false;
            dashDelay = 0f;
            PlayerFace.SetActive(true);
        }
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if (isDrawing || isDash || isHurt || isDead || isStory || isStory2 || isInteract) return;

        if (jumpDelayTimer < 0.25f) return;

        if (context.started && jumpCount < 2)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            if (jumpCount == 1)
            {
                jumpDelayTimer = 0f;
                jumpEffect0.Play();
                jumpEffect1.Play();
                SwitchAni(2);
            }
            else if (jumpCount == 2)
            {
                jumpDelayTimer = 0f;
                jumpEffect0.Play();
                jumpEffect1.Play();
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

    /*
    public void Jump()
    {
        if (waitJump)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > jumpWaitTime)
            {
                waitTimer = 0f;
                waitJump = false;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
            }
        }

        if (jumpCount == 1)
        {
            jumpDelayTimer = 0f;
            jumpEffect0.Play();
            jumpEffect1.Play();
            SwitchAni(2);
        }
        else if (jumpCount == 2)
        {
            jumpDelayTimer = 0f;
            jumpEffect0.Play();
            jumpEffect1.Play();
            playerAni.ResumeAni();
            SwitchAni(3);
        }
    }
    */

    //Saving(在save point interaction中使用)
    public void Saving()
    {
        playerAni.ResumeAni();
        OtherAni.SetInteger(action, 2);
        PlayerFace.SetActive(false);
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }
    public void DeSaving()
    {
        isInteract = false;
        PlayerFace.SetActive(true);
    }

    void Attack()
    {
        if (isDash || isHurt || isStory || isStory2 || isInteract) return;

        if (Input.GetMouseButtonDown(0) && !isDash && canAttack && attackStep != 3 && skyAttack)
        {
            int aniInt = animator.GetInteger(action);
            if (aniInt != 4 && aniInt != 5 && aniInt != 6) previousAct = aniInt;
            attackStep += 1;
            attackEffect.attackAmount += 1;
            attackDelay = 0f;
            attackKeep = 0f;
            SwitchAni(3 + attackStep);
            canAttack = false;
            isAttack = true;
            oriPos = transform.position;
            rb.linearVelocity = direction ? new Vector3(10f, 0, 0) : new Vector3(-10f, 0, 0);
            attackCollider.enabled = true;
            attackPratical.SetActive(true);
        }
        if (!isAttack)
        {
            attackCollider.enabled = false;
            attackPratical.SetActive(false);
            return;
        }
        if (Vector3.Distance(oriPos, transform.position) > 0.5f) rb.linearVelocity = new Vector3(0, 0, 0);

        if (!isGrounded) playerAni.ResumeAni(); //跳躍時的攻擊問題

        //delay between steps
        if (attackStep == 3 && attackDelay > attackStepDelay + 0.1f)
        {
            canAttack = true;
            attackDelay = 0f;
            attackStep = 0;
            if (!isGrounded) skyAttack = false;
        }
        else if (attackDelay > attackStepDelay && attackStep != 3)
        {
            canAttack = true;
            attackDelay = 0f;
        }
        else
        {
            attackDelay += Time.deltaTime;
        }

        //cancel attack
        if (attackKeep > 0.45f)
        {
            isAttack = false;
            attackKeep = 0;
            attackStep = 0;
            if (!isGrounded) skyAttack = false;
            SwitchAni(previousAct);
        }
        else
        {
            attackKeep += Time.deltaTime;
        }

    }
    void Draw()
    {

    }
    public void Hurt(int damage, int type, float x)
    {
        if (isHurt || isDash || isDead || isStory || isStory2 || isInteract) return;

        
        if (camShake != null)
        {
            camShake.Shake(0.5f, 0.2f);
        }
        else {
            Debug.Log("鏡頭晃動");
        }

        

        isHurt = true;
        hurtTimer = 0f;
        hurtType = type;
        if (type == 0) SwitchAni(-1);
        if (type == 1) SwitchAni(7);
        oriPos = transform.position;

        if (type == 0)
        {
            if (x != 0) rb.linearVelocity = x > 0 ? new Vector3(-10f, 0, 0) : new Vector3(10f, 0, 0);
            else rb.linearVelocity = direction ? new Vector3(-10f, 0, 0) : new Vector3(10f, 0, 0);
        }

        if (x > 0)
        {
            transform.localScale = new Vector3(-1f * xScale, transform.localScale.y, transform.localScale.z);
            direction = true;
        }
        else if (x < 0)
        {
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
            direction = false;
        }

        playerStatus.blood -= damage;
    }

    public void Dead()
    {
        if (playerStatus.blood == 0)
        {
            isDead = true;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            SwitchAni(-2);
        }
        if (!isDead) return;

        deadTimer += Time.deltaTime;
        if (deadTimer > 2f)
        {
            Reset();
        }
    }

    public void SetUpForInteraction(Vector3 target, int type)
    {
        interactTarget = target;
        isInteract = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        isGoTarget = true;
        toTargetType = type;
        if (type == 0) SwitchAni(11);
        else if (type == 1) SwitchAni(1);
    }
    void GoTarget()
    {
        if (!isGoTarget) return;

        if (toTargetType == 0)
        {
            Vector3 dirTarget = (interactTarget - transform.position).normalized;
            if (dirTarget.x > 0)
            {
                direction = true;
                transform.localScale = new Vector3(-1f * xScale, transform.localScale.y, transform.localScale.z);
            }
            else if (dirTarget.x < 0)
            {
                direction = false;
                transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
            }

            rb.linearVelocity = new Vector3(moveSpeed * 0.4f * dirTarget.x, rb.linearVelocity.y, rb.linearVelocity.z);
            if (Math.Abs(interactTarget.x - transform.position.x) < 0.1f)
            {
                isGoTarget = false;
                SwitchAni(0);
                rb.linearVelocity = Vector3.zero;
            }
        }
        else if (toTargetType == 1)
        {
            Vector3 dirTarget = (interactTarget - transform.position).normalized;
            if (dirTarget.x > 0)
            {
                direction = true;
                transform.localScale = new Vector3(-1f * xScale, transform.localScale.y, transform.localScale.z);
            }
            else if (dirTarget.x < 0)
            {
                direction = false;
                transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
            }

            rb.linearVelocity = new Vector3(moveSpeed * dirTarget.x, rb.linearVelocity.y, rb.linearVelocity.z);
            if (Math.Abs(interactTarget.x - transform.position.x) < 0.1f)
            {
                isGoTarget = false;
                SwitchAni(0);
                rb.linearVelocity = Vector3.zero;
            }
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
        isGoTarget = false;
        transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
    }
}
