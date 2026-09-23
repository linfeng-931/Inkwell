using UnityEngine;
using UnityEngine.Audio;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState { get; private set; }
    private PlayerStateFactory stateFactory;

    [Header("Component References")]
    public Animator animator { get; private set; }
    public Rigidbody rig { get; private set; }
    public InputBufferManager inputBufferManager { get; private set; }
    public CapsuleCollider col { get; private set; }
    public PlayerEnergy playerEnergy { get; set; }

    [Header("Player Control Toggle")]
    public bool isPlayerInputEnabled = true;
    public float currentMoveX { get; set; }

    [Header("Environmental Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded { get; private set; }

    [Header("Energy Setting")]
    public float dashCost = 20f;
    public float hookCost = 35f;
    public float shootCost = 35f;

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
    public int currentAirJumps = 1;
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
    public bool canAirDash = true;
    public float dashLookAhead = 0.5f;
    public float cornerCorrectionRange = 0.4f;
    public float correctionStep = 0.1f;

    public float lastDashTime { get; set; } = -100f;

    private ParticleSystem[] dashParticleSystems;

    [Header("Attack Setting")]
    public AttackData[] groundCombo;
    public AttackData[] airCombo;
    public bool canAirAttack = true;
    public int damage = 1;

    public int currentComboIndex { get; set; } = 0;
    public AttackData[] currentComboList { get; set; }

    [Header("Hurt and Die Setting")]
    public float hurtKnockbackForce = 5f;
    public float hurtKnockbackDuration = 0.1f;
    public float hurtDuration = 0.8f;

    [Header("Hook Setting")]
    public float hookRange = 10f;
    public float hookSpeed = 25f; // move speed
    public float hookStopDistance = 1f;
    public float hookCooldown = 0.5f;
    public int hookDamage = 1;

    public LayerMask hookLayer;

    public float hookBodyOffsetX = 0.5f;
    public float hookBodyOffsetY = 1.5f;

    public float hookShootSpeed = 40f; // line speed
    public float hookRetractSpeed = 60f;

    public float hookTipRadius = 0.3f; // detect point range

    public float hookMissPauseTime = 0.15f;
    public float hookHangTime = 0.2f; // hang on the air

    public Vector3 currentHookTipPos { get; set; }
    public Vector3 currentHookTarget { get; set; }
    public float lastHookTime { get; set; } = -100f;

    [Header("Hook Rope")]
    public int ropeResolution = 10; // line effect
    public LineRenderer hookLineRenderer;

    public float bounceAmplitude = 1.2f;
    public float bounceFrequency = 45f;

    [Header("Rope Retract Shape")]
    public float ropeSAmount = 2f; // width
    public float ropeSFrequency = 1f; // amount
    public float ropeSOffset = 0.35f; // sense of fluidity
    public AnimationCurve ropeSGrowth = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // dynamic contraction curve

    [Header("Shoot Setting")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float shootBulletSpeed = 25f;
    public float shootDuration = 0.2f;
    public float shootCooldown = 0.2f;

    public float lastShootTime { get; set; } = -100f;

    private Camera mainCam;
    #endregion

    [Header("Turn Setting")]
    public bool canTurn = true;
    public bool isFacingRight = true; //should check your player image direction

    [Header("Art Setting")]
    public GameObject playerFace;

    [Header("SFX Setting")]
    public AudioManager audioManager;
    public AudioSource sfxAudioSource;
    public AudioClip footstepClip;
    public AudioClip hookClip;
    public AudioClip atkClip;

    private void Awake()
    {
        //init components
        animator = playerFace.GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        inputBufferManager = GetComponent<InputBufferManager>();
        col = GetComponent<CapsuleCollider>();
        playerEnergy = GetComponent<PlayerEnergy>();

        dashParticleSystems = dashParticle.GetComponentsInChildren<ParticleSystem>();

        mainCam = Camera.main;
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
            CheckGlobalAbilities();
        }

        if (canTurn)
        {
            HandleTurning();
        }

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

    public bool CheckGrounded()
    {
        Bounds bounds = col.bounds;

        float castRadius = bounds.extents.x * 0.9f; // avoid to detect wall
        Vector3 startPos = bounds.center - new Vector3(0, bounds.extents.y - 0.1f, 0);
        return Physics.SphereCast(startPos, castRadius, Vector3.down, out RaycastHit hit, 0.2f, groundLayer);
    }

    /// <summary>
    /// turn player face
    /// </summary>
    public void HandleTurning()
    {
        if (currentMoveX > 0.1f && !isFacingRight)
        {
            isFacingRight = true;
            Flip();
        }
        else if (currentMoveX < -0.1f && isFacingRight)
        {
            isFacingRight = false;
            Flip();
        }
    }

    /// <summary>
    /// force to face target position
    /// </summary>
    /// <param name="targetPos"></param>
    public void FaceTowards(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x && !isFacingRight)
        {
            isFacingRight = true;
            Flip();
        }
        else if (targetPos.x < transform.position.x && isFacingRight)
        {
            isFacingRight = false;
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 playerScale = transform.localScale;
        playerScale.x *= -1;
        transform.localScale = playerScale;
    }

    /// <summary>
    /// check and enter isolated states
    /// </summary>
    private void CheckGlobalAbilities()
    {
        // dash
        if (inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Dash))
        {
            bool hasEnergy = playerEnergy.currentEnergy >= dashCost;
            if (hasEnergy)
            {
                bool isCooldownReady = Time.time >= (lastDashTime + dashCooldown);
                bool hasSpaceToDash = isGrounded || canAirDash;

                if (isCooldownReady && hasSpaceToDash)
                {
                    playerEnergy.ConsumeEnergy(dashCost);
                    lastDashTime = Time.time;

                    if (!isGrounded)
                    {
                        canAirDash = false;
                    }

                    inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Dash);
                    TransitionToState<DashState>();

                    return;
                }
            }
        }

        // hook
        if (inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Hook))
        {
            bool hasEnergy = playerEnergy.currentEnergy >= hookCost;
            if (hasEnergy)
            {
                bool isCooldownReady = Time.time >= (lastHookTime + hookCooldown);

                if (isCooldownReady)
                {
                    playerEnergy.ConsumeEnergy(hookCost);
                    lastHookTime = Time.time;

                    inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Hook);
                    TransitionToState<HookShootState>();

                    return;
                }
            }
        }

        // shoot
        if (inputBufferManager.HasBufferedInput(InputBufferManager.InputActionType.Shoot))
        {
            bool hasEnergy = playerEnergy.currentEnergy >= shootCost;
            if (hasEnergy)
            {
                bool isCooldownReady = Time.time >= (lastShootTime + shootCooldown);

                if (isCooldownReady)
                {
                    playerEnergy.ConsumeEnergy(shootCost);
                    lastShootTime = Time.time;
                    
                    inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Shoot);
                    TransitionToState<ShootState>();

                    return;
                }
            }
        }
    }

    // control particle or other effect
    public void PlayDashParticle()
    {
        foreach (ParticleSystem ps in dashParticleSystems)
        {
            ps.Play();
        }
    }

    public void StopDashParticle()
    {
        foreach (ParticleSystem ps in dashParticleSystems)
        {
            ps.Stop();
        }
    }

    /// <summary>
    /// get mouse direction according to player center
    /// </summary>
    public Vector3 GetMouseDirection()
    {
        Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

        // get x, y plane
        Plane playerPlane = new Plane(Vector3.forward, transform.position);
        Ray ray = mainCam.ScreenPointToRay(mouseScreenPos);

        if (playerPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePos = ray.GetPoint(distance);
            return (worldMousePos - transform.position).normalized;
        }

        return isFacingRight ? Vector3.right : Vector3.left;
    }

    /// <summary>
    /// calculate the final pos according to hook target type
    /// </summary>
    public Vector3 CalculateHookDestination(Vector3 targetPos, HookTargetType type)
    {
        Vector3 destination = targetPos;
        float directionToPlayerX = transform.position.x < targetPos.x ? -1f : 1f;

        switch (type)
        {
            case HookTargetType.AirEnemy:
            case HookTargetType.GroundEnemy:
                destination.x += directionToPlayerX * hookBodyOffsetX;
                destination.y = targetPos.y;
                break;
            case HookTargetType.HookPoint:
                destination.x = targetPos.x;
                destination.y = targetPos.y + hookBodyOffsetY;
                break;
        }
        destination.z = targetPos.z;
        return destination;
    }

    #region GameEvent
    private void OnEnable()
    {
        GameEvent.OnInteractStateChanged += HandleInteractState;
    }

    private void OnDisable()
    {
        GameEvent.OnInteractStateChanged -= HandleInteractState;
    }

    private void HandleInteractState(bool isInteract) // use game event to enter interact
    {
        if (isInteract)
        {
            TransitionToState<InteractState>();
        }
        else
        {
            if(isGrounded) TransitionToState<IdleState>();
            else TransitionToState<FallState>();
        }
    }
    #endregion

    # region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hookRange);
    }
    #endregion
}