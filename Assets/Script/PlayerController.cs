using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Action Setting")]
    public bool direction;
    public float moveSpeed;
    public float jumpForce;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        action = "Idle";
        timer = 0f;
        rb = GetComponent<Rigidbody>();
        currentMoveSpeed = 0f;
        direction = true;
        jumpCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);

        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(drawPrefab);
        }
    }
    void FixedUpdate()
    {
        //Gravity
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityValue * gravityValue * Time.deltaTime, ForceMode.Acceleration);
        }
        else if(rb.linearVelocity.y < 0)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = -0.5f;
            rb.linearVelocity = vel;
        }

        if (isGrounded)
        {
            jumpCount = 0;
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
        if(moveInput < 0 && direction) direction = false;
        else if(moveInput > 0 && !direction) direction = true;
    }
    void Move()
    {
        if(Math.Abs(moveInput) < 0.01f){
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

        rb.linearVelocity = new Vector3(currentMoveSpeed*moveInput, rb.linearVelocity.y, rb.linearVelocity.z);
    }
    void Dash()
    {
        
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if (context.started && jumpCount <1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
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
}
