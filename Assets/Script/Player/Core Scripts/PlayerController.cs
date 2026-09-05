using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState { get; private set; }
    private PlayerStateFactory stateFactory;

    //components
    public Animator animator { get; private set; }
    public Rigidbody rig { get; private set; }
    public InputBufferManager inputBufferManager { get; private set; }

    [Header("Player Control Toggle")]
    public bool isPlayerInputEnabled = true;
    public float currentMoveX { get; set; }

    [Header("Environmental Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded { get; private set; }

    #region Action Setting
    [Header("Move Setting")]
    public float runSpeed = 8f;
    public float walkSpeed = 3f;
    public float acceleration = 50f;
    public float deceleration = 50f;

    [Header("Airborne Setting")]
    public float gravityScale = 2f;
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f;
    public float airMoveSpeed = 5f;

    [Header("Apex Modifier Setting")]
    public float apexThreshold = 1.5f;
    public float apexHangTimeMultiplier = 0.5f;

    [Header("Coyote Time Setting")]
    public float coyoteTime = 0.15f;
    public float coyoteTimer { get; set; }

    [Header("Dash Setting")]
    public GameObject dashParticle;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.6f;
    public float dashEndCut = 0.1f;
    public float lastDashTime = -100f;
    public bool canAirDash = true;
    public float dashLookAhead = 0.5f;
    public float cornerCorrectionRange = 0.4f;
    public float correctionStep = 0.1f;

    private ParticleSystem[] dashParticleSystems;

    #endregion

    [Header("Turn Setting")]
    public bool canTurn = true;
    public bool isFacingRight = true; //should check your player image direction

    [Header("Art Setting")]
    public GameObject playerFace;

    private void Awake()
    {
        //init components
        animator = playerFace.GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        inputBufferManager = GetComponent<InputBufferManager>();

        dashParticleSystems = dashParticle.GetComponentsInChildren<ParticleSystem>();
    }

    void Start()
    {
        stateFactory = new PlayerStateFactory(this);

        //original state
        TransitionToState<IdleState>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isPlayerInputEnabled)
        {
            currentMoveX = inputBufferManager.moveInputX;
        }

        if (canTurn)
        {
            HandleTurning();
        }

        CheckGlobalAbilities();

        currentState.Update();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate();
    }

    /// <summary>
    /// handle transition to new state and exit current state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void TransitionToState<T>() where T : PlayerState
    {
        PlayerState newState = stateFactory.GetState<T>();

        //have uncompleted action, requesting to exit current state
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    /// <summary>
    /// turn player face
    /// </summary>
    public void HandleTurning()
    {
        if (currentMoveX > 0.1f && !isFacingRight)
        {
            isFacingRight = true;
            Vector3 playerScale = transform.localScale;
            playerScale.x *= -1;
            transform.localScale = playerScale;
        }
        else if (currentMoveX < -0.1f && isFacingRight)
        {
            isFacingRight = false;
            Vector3 playerScale = transform.localScale;
            playerScale.x *= -1;
            transform.localScale = playerScale;
        }
    }

    /// <summary>
    /// check and enter isolated states
    /// </summary>
    private void CheckGlobalAbilities()
    {
        if (inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Dash))
        {
            bool isCooldownReady = Time.time >= (lastDashTime + dashCooldown);
            bool hasSpaceToDash = isGrounded || canAirDash;

            if (isCooldownReady && hasSpaceToDash)
            {
                lastDashTime = Time.time;
                if (!isGrounded)
                {
                    canAirDash = false;
                }

                inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Dash);
                TransitionToState<DashState>();
            }
        }
    }

    public void PlayDashParticle()
    {
        foreach(ParticleSystem ps in dashParticleSystems)
        {
            ps.Play();
        }
    }

    public void StopDashParticle()
    {
        foreach(ParticleSystem ps in dashParticleSystems)
        {
            ps.Stop();
        }
    }
}