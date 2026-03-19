using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Action Setting")]
    public bool direction;
    public float moveSpeed;
    public float jumpForce;
    public Animator animator;
    public PlayerAni playerAni;
    public ParticleSystem footEffect;

    [Header("Collision Setting")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkRadius = 0.3f;
    public bool isGrounded;

    [Header("Other Setting")]
    public GameObject drawPrefab;
    public float gravityValue;

    private float timer;
    private string action; //dash, jump, run, attack, skill, draw, idle
    private Rigidbody rb;

    //move
    private float currentMoveSpeed;
    private float accelerateSpeed = 80f;
    private float moveInput;

    //jump
    private int jumpCount;
    private float jumpDelay;
    private float jumpDelayTimer;

    //draw
    private bool isDrawing;


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
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);
        if (isGrounded && rb.linearVelocity.y < 0.1f)
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

        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            currentMoveSpeed = 0.09f;
            Instantiate(drawPrefab);
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }
    void FixedUpdate()
    {
        //Gravity
        if (!isGrounded)
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
    }
    void Dash()
    {
        
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if(isDrawing) return;

        if (context.started && jumpCount <2)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            if(jumpCount == 1){
                footEffect.Play();
                SwitchAni(2);
            }
            else if(jumpCount == 2){
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
    public void Jump()
    {
        
    }
    void Attack()
    {
        
    }
    void Draw()
    {
        
    }

    void SwitchAni(int act)
    {
        animator.SetInteger("action", act);
    }
}
