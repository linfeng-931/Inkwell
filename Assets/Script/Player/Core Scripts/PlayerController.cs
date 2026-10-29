using System;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState { get; private set; }
    private PlayerStateFactory stateFactory;

    [Header("Component References")]
    public Animator animator { get; private set; }
    public Animator animator2 { get; private set; }
    public Animator animator3 { get; private set; } //wait for live2d
    public Rigidbody rig { get; private set; }
    public InputBufferManager inputBufferManager { get; private set; }
    public CapsuleCollider col { get; private set; }
    public PlayerEnergy playerEnergy { get; set; }
    public PlayerHealth playerHealth { get; set; }

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
    public float walkSpeed = 5f;
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
    //public float correctionStep = 0.1f;

    public float lastDashTime { get; set; } = -100f;

    private ParticleSystem[] dashParticleSystems;

    [Header("Roll Setting (LedgeSnap)")]
    public float cornerCorrectionRange = 0.4f; // ledgeSnap range
    public float rollDuration = 0.1f;
    public float rollSpeed = 1f;
    public float rollEndDuration = 0.2f;
    public ParticleSystem rollParticle;

    [Header("Attack Setting")]
    public AttackData[] groundCombo;
    public AttackData[] airCombo;
    public bool canAirAttack = true;
    public HitBox hitBox;
    public int damage = 1;

    public int currentComboIndex { get; set; } = 0;
    public AttackData[] currentComboList { get; set; }
    public bool canAttack { get; set; } = true; // for story

    [Header("Hurt and Die Setting")]
    public float hurtKnockbackForce = 5f;
    public float hurtKnockbackDuration = 0.1f;
    public float hurtDuration = 0.8f;
    public float fadeOutWaitTime = 0.5f;
    public bool isHazardHurt = false;
    public Vector3 hazardRespawnPosition;
    public Vector3 lastCheckpointPosition;

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

    public CursorManager cursorManager; 
    public Transform hookLineStartPoint;

    //hook Visuals
    public GameObject hookPenObject;
    public Transform hookPenRoot;
    public Transform hookPenTip;
    public Transform hookStartPoint;

    public float maxArmAngleUp = 60f;
    public float maxArmAngleDown = -45f;

    [Header("Hook Rope")]
    public int ropeResolution = 10; // line effect
    public LineRenderer hookLineRenderer;

    public float bounceAmplitude = 1.2f;
    public float bounceFrequency = 45f;
     public Vector3 currentRopeVisualPos;

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

    public Camera mainCam { get; set; }

    [Header("Cutscene Setting")]
    public Transform cutscenePosition;
    public bool requireGroundedForCutscene;
    public bool isCutsceneReady;

    #endregion

    [Header("Turn Setting")]
    [SerializeField] private bool sceneScaleFacesRight = false;
    public bool canTurn = true;
    public bool isFacingRight = true; //should check your player image direction

    [Header("Art Setting")]
    public GameObject playerFace;
    public GameObject playerFace2; // ani not in live2d
    public GameObject playerFace3; // wait live2d

    [Header("SFX Setting")]
    public AudioManager audioManager;
    public AudioSource sfxAudioSource;
    public AudioClip footstepClip;
    public AudioClip jumpClip;
    public AudioClip hookClip;
    public AudioClip atkClip;
    public AudioClip dashClip;
    public AudioClip outOffEnergy;
    public AudioClip fullOffEnergy;

    [Header("Time line Control")]
    private PlayerLoadout loadout;
    private static bool IsCombatState(System.Type t) =>
    t == typeof(AttackState) || t == typeof(DashState) ||
    t == typeof(HookShootState) || t == typeof(ShootState);

    [Header("Player Status")]
    public bool CanMove => !PlayerLock.IsLocked(PlayerLockType.Movement);
    public bool CanFight => (loadout == null || loadout.CombatEnabled)
                            && !PlayerLock.IsLocked(PlayerLockType.Combat);
    public bool CanRun => loadout == null || loadout.RunEnabled;
    public bool canJump = false;
    private bool wasCanMove = true;
    [SerializeField] private float jumpGraceAfterUnlock = 0.25f;

    private void Awake()
    {
        //init components
        animator = playerFace.GetComponent<Animator>();
        animator2 = playerFace2.GetComponent<Animator>();
        animator3 = playerFace3.GetComponent<Animator>();
        playerFace2.SetActive(false);
        playerFace3.SetActive(false);
        hookPenObject.SetActive(false);

        rig = GetComponent<Rigidbody>();
        inputBufferManager = GetComponent<InputBufferManager>();
        col = GetComponent<CapsuleCollider>();
        playerEnergy = GetComponent<PlayerEnergy>();
        playerHealth = GetComponent<PlayerHealth>();
        mainCam = Camera.main;

        dashParticleSystems = dashParticle.GetComponentsInChildren<ParticleSystem>();
        loadout = GetComponent<PlayerLoadout>();

        if (isFacingRight != sceneScaleFacesRight)
        {
            Flip();
        }
    }

    void Start()
    {
        stateFactory = new PlayerStateFactory(this);
        lastCheckpointPosition = transform.position;

        //original state
        TransitionToState<IdleState>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        bool canMoveNow = CanMove;
        if (canMoveNow && !wasCanMove)
        {
            inputBufferManager.BlockInput(InputBufferManager.InputActionType.Jump, jumpGraceAfterUnlock);
        }
        wasCanMove = canMoveNow;

        if (isPlayerInputEnabled)
        {
            currentMoveX = CanMove ? inputBufferManager.moveInputX : 0f;
            CheckGlobalAbilities();
        }


        if (isPlayerInputEnabled)
        {
            currentMoveX = CanMove ? inputBufferManager.moveInputX : 0f;
            CheckGlobalAbilities();
        }

        if (canTurn)
        {
            HandleTurning();
        }

        if (!CanRun && currentState is RunState) TransitionToState<WalkState>();

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
        // Lock Player Run
        if (typeof(T) == typeof(RunState) && !CanRun)
        {
            if (currentState is WalkState) return;
            TransitionToState<WalkState>();
            return;
        }

        // Lock Player Attack or Jump
        if (IsJumpState(typeof(T)) && (!canJump || !CanMove)) { return; }
        if (!CanFight && IsCombatState(typeof(T))) { return; }

        PlayerState newState = stateFactory.GetState<T>();

        //have uncompleted action, requesting to exit current state
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    private static bool IsJumpState(System.Type t) =>
    t == typeof(JumpState) || t == typeof(DoubleJumpState);

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
    /// turn player face
    /// </summary>
    public void HandleTurning(bool faceRight)
    {
        if (faceRight && !isFacingRight)
        {
            isFacingRight = true;
            Flip();
        }
        else if (!faceRight && isFacingRight)
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

        Vector3 faceSize = playerFace.transform.localScale;
        playerFace.transform.localScale = new Vector3(faceSize.x * -1f, faceSize.y, faceSize.z);

        transform.localScale = playerScale;
    }

    /// <summary>
    /// check and enter isolated states
    /// </summary>
    private void CheckGlobalAbilities()
    {
        // Disabled player skill
        if (!CanFight) { return; }

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
            else
            {
                audioManager.PlaySFX(outOffEnergy);
                inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Dash);
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
            else
            {
                audioManager.PlaySFX(outOffEnergy);
                inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Hook);
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
            else
            {
                audioManager.PlaySFX(outOffEnergy);
                inputBufferManager.ConsumeInput(InputBufferManager.InputActionType.Shoot);
            }
        }
    }

    /// <summary>
    /// control particle or other effect
    /// </summary>
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

    public void UpdateCheckpoint(Vector3 newPoint)
    {
        lastCheckpointPosition = newPoint;
    }

    /// <summary>
    /// setup target pos and enter cutscene state
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="requireGrounded"></param>
    public void EnterCutscene(Transform startPosition = null, bool requireGrounded = false)
    {
        cutscenePosition = startPosition;
        requireGroundedForCutscene = requireGrounded;
        TransitionToState<CutsceneState>();
    }

    // Control every teleportation, include
    // 1. Spawn in specific location when scene change
    // 2. Spawn in specific location when hurt in sky or etc
    // 3. Spawn in specific location when player totally died
    public void TeleportTo(Vector3 targetPosition, bool resetVelocity = true)
    {
        if (rig != null)
        {
            if (resetVelocity)
            {
                rig.linearVelocity = Vector3.zero;
                rig.angularVelocity = Vector3.zero;
            }
            rig.position = targetPosition;
        }
        transform.position = targetPosition;
        Physics.SyncTransforms();
    }

    #region GameEvent
    private void OnEnable()
    {
        GameEvent.OnInteractStateChanged += HandleInteractState;
        GameEvent.OnPlayerReborn += HandleReborn;
        GameEvent.OnPlayerSave += HandleSave;
    }

    private void OnDisable()
    {
        GameEvent.OnInteractStateChanged -= HandleInteractState;
        GameEvent.OnPlayerReborn -= HandleReborn;
        GameEvent.OnPlayerSave -= HandleSave;
    }

    private void HandleInteractState(bool isInteract) // use game event to enter interact
    {
        if (isInteract)
        {
            TransitionToState<InteractState>();
        }
        else
        {
            inputBufferManager.BlockInput(InputBufferManager.InputActionType.Jump, jumpGraceAfterUnlock);
            if (isGrounded) TransitionToState<IdleState>();
            else TransitionToState<FallState>();
        }
    }

    public void StopMovement()
    {
        if (rig != null)
        {
            rig.linearVelocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
        }

        if (!(currentState is IdleState))
        {
            TransitionToState<IdleState>();
        }
    }

    private void HandleReborn()
    {
        TransitionToState<RebornState>();
    }

    private void HandleSave(Transform startPosition)
    {
        EnterSave(startPosition);
    }
    
    /// <summary>
    /// setup target pos and enter cutscene state
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="requireGrounded"></param>
    public void EnterSave(Transform startPosition = null, bool requireGrounded = false)
    {
        cutscenePosition = startPosition;
        requireGroundedForCutscene = requireGrounded;
        TransitionToState<SaveState>();
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