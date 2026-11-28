
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
using MonsterLove.StateMachine;

[RequireComponent(typeof(Rigidbody))]
public class Yak : MonoBehaviour, IEnemy
{
    public enum States { Idle, Patrol, Chase, Ready, Attack, Cooldown, Hurt, Death }
    private StateMachine<States> fsm;

    [Header("Attribute")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Move Setting")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;

    [Header("Attack Setting")]
    public float attackSpeed = 12f;
    public float readyTime = 1f;
    public float attackDuration = 1f;
    public float cooldownTime = 1.5f;
    public float faceDeadZone = 0.8f;

    private float attackStartTime = -100;
    private float attackDirectionX = 1f;

    [Header("Detect Setting")]
    public float sightRange = 8f;
    public float attackRange = 5f;
    public float hurtPlayerRange = 1f;
    public Transform hurtPlayerPoint;

    private Transform player;
    private PlayerHealth playerHealth;

    [Header("Prevent Setting")]
    public Transform edgeCheckPoint;
    public float rayDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Hurt Setting")]
    public float hurtStunTime = 0.2f;
    public float hurtFlashTime = 0.1f; 

    [Header("Hurt and Death Effect")]
    public ParticleSystem hurtParticle;
    public ParticleSystem deadParticle;
    public Material hurtMaterial;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Coroutine flashCoroutine;

    [Header("Animate")]
    public Animator animator;
    public string idleAni = "Yak_Idle";
    public string walkAni = "Yak_Walk";
    public string attackStartAni = "Yak_Attack_Start";
    public string attackAni = "Yak_Attack";
    public string attackEndAni = "Yak_Attack_End";
    public string damagedAni = "Yak_Damaged";

    [Header("SFX")]
    public AudioManager audioManager;
    public AudioClip atkSfx;

    [Header("Event")]
    public UnityEvent afterDead;
    public event Action<Yak> OnDied;

    private Rigidbody rig;
    private bool facingRight = false;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        fsm = StateMachine<States>.Initialize(this);
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.sharedMaterial;
    }

    void Start()
    {
        player = MapManager.Instance.player.transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        audioManager = MapManager.Instance.audioManager;
        fsm.ChangeState(States.Idle);
    }

    private bool IsPathBlockedOrEdge()
    {
        // Ground enemy only moves on X axis
        Vector3 forwardDirection = facingRight ? Vector3.right : Vector3.left;

        bool isWallAhead = Physics.Raycast(
            transform.position,
            forwardDirection,
            rayDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        bool isEdgeAhead = false;

        isEdgeAhead = !Physics.Raycast(
            edgeCheckPoint.position,
            Vector3.down,
            rayDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        // draw ray
        Debug.DrawRay(transform.position, forwardDirection * rayDistance, Color.red);
        Debug.DrawRay(edgeCheckPoint.position, Vector3.down * rayDistance, Color.green);

        return isWallAhead || isEdgeAhead;
    }

    private bool IsPlayerAlive()
    {
        return playerHealth != null && playerHealth.currentHealth > 0;
    }

    private bool IsPlayerInSight()
    {
        if (!IsPlayerAlive()) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float distToPlayer = dirToPlayer.sqrMagnitude;

        bool checkY = player.position.y >= (transform.position.y - 0.5f);

        return (distToPlayer <= sightRange * sightRange) && checkY;
    }

    private bool IsPlayerInAttackRange()
    {
        float sqrDistance = (player.position - transform.position).sqrMagnitude;
        return sqrDistance <= attackRange * attackRange;
    }

    private bool IsPlayerInHurtRange()
    {
        float sqrDistance = (player.position - hurtPlayerPoint.position).sqrMagnitude;
        return sqrDistance <= hurtPlayerRange * hurtPlayerRange;
    }

    private void Turn()
    {
        facingRight = !facingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

    private void FacePlayer(float deadZone = 0.05f)
    {
        float dirToPlayer = player.position.x - transform.position.x;

        if (Mathf.Abs(dirToPlayer) < deadZone) return;

        if ((dirToPlayer > 0 && !facingRight) || (dirToPlayer < 0 && facingRight))
        {
            Turn();
        }
    }

    private void StopMovement()
    {
        rig.linearVelocity = Vector3.zero;
    }

    private void StopHorizontalMovement()
    {
        rig.linearVelocity = new Vector3(0f, rig.linearVelocity.y, 0f);
    }

    public void TakeDamage(int damage)
    {
        if (fsm.State == States.Death) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            fsm.ChangeState(States.Death, StateTransition.Overwrite);
            return;
        }

        // don't stop attack if yak is ready state
        if(fsm.State == States.Ready || fsm.State == States.Attack)
        {
            PlayHurtFeedback();
            return;
        }

        fsm.ChangeState(States.Hurt, StateTransition.Overwrite);
    }

    private void PlayHurtFeedback()
    {
        hurtParticle.Play();

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetHurtMaterial(true);
        yield return new WaitForSeconds(hurtFlashTime);
        SetHurtMaterial(false);
        flashCoroutine = null;
    }

    private void SetHurtMaterial(bool isHurt)
    {
        spriteRenderer.sharedMaterial = isHurt ? hurtMaterial : originalMaterial;
    }

    #region State
    // Idle
    IEnumerator Idle_Enter()
    {
        animator.Play(idleAni, 0);
        StopHorizontalMovement();

        yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 3f));

        fsm.ChangeState(States.Patrol);
    }

    void Idle_Update()
    {
        if (IsPlayerInSight())
        {
            fsm.ChangeState(States.Chase);
        }
    }

    // Patrol
    void Patrol_Enter()
    {
        animator.Play(walkAni, 0);
        Turn();
    }

    void Patrol_Update()
    {
        if (IsPlayerInSight())
        {
            fsm.ChangeState(States.Chase);
            return;
        }

        if (IsPathBlockedOrEdge())
        {
            fsm.ChangeState(States.Idle);
            return;
        }
        rig.linearVelocity = new Vector3((facingRight ? 1 : -1) * patrolSpeed, rig.linearVelocity.y, 0f);
    }

    void Patrol_Exit()
    {
        StopHorizontalMovement();
    }

    // Chase
    void Chase_Enter()
    {
        animator.Play(walkAni, 0);
    }

    void Chase_Update()
    {
        // enter idle
        if (!IsPlayerInSight())
        {
            fsm.ChangeState(States.Idle);
            return;
        }

        // enter attack
        if (IsPlayerInAttackRange())
        {
            fsm.ChangeState(States.Ready);
            return;
        }

        // chase logic
        FacePlayer(faceDeadZone);

        if (IsPathBlockedOrEdge())
        {
            StopHorizontalMovement();
        }
        else
        {
            rig.linearVelocity = new Vector3((facingRight ? 1 : -1) * chaseSpeed, rig.linearVelocity.y, 0f);
        }
    }

    void Chase_Exit()
    {
        StopHorizontalMovement();
    }

    // Ready
    IEnumerator Ready_Enter()
    {
        StopHorizontalMovement();

        // check face
        FacePlayer();
        attackDirectionX = facingRight ? 1f : -1f;

        animator.Play(attackStartAni, 0);

        yield return new WaitForSeconds(readyTime);
        fsm.ChangeState(States.Attack);
    }

    // Attack
    void Attack_Enter()
    {
        attackStartTime = Time.time;
        animator.Play(attackAni, 0);

        if (audioManager != null && atkSfx != null) audioManager.PlaySFX(atkSfx);
    }

    void Attack_Update()
    {
        if (attackStartTime + attackDuration <= Time.time || IsPathBlockedOrEdge())
        {
            fsm.ChangeState(States.Cooldown);
            return;
        }

        if (IsPlayerAlive() && IsPlayerInHurtRange())
        {
            playerHealth.TakeDamage(1, transform.position);
            fsm.ChangeState(States.Cooldown);
            return;
        }

        rig.linearVelocity = new Vector3(attackDirectionX * attackSpeed, rig.linearVelocity.y, 0f);
    }

    void Attack_Exit()
    {
        StopHorizontalMovement();
    }

    // Cooldown
    IEnumerator Cooldown_Enter()
    {
        // stop ani
        animator.Play(attackEndAni, 0);
        StopHorizontalMovement();
        yield return new WaitForSeconds(0.2f);

        // wait ani
        animator.Play(idleAni, 0);
        yield return new WaitForSeconds(cooldownTime);

        fsm.ChangeState(States.Idle);
    }

    // Hurt
    IEnumerator Hurt_Enter()
    {
        StopHorizontalMovement();
        FacePlayer();

        animator.Play(damagedAni, 0);
        PlayHurtFeedback();

        yield return new WaitForSeconds(hurtStunTime);

        if (IsPlayerInSight()) fsm.ChangeState(States.Chase);
        else fsm.ChangeState(States.Idle);
    }

    void Hurt_Exit()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        SetHurtMaterial(false);
    }

    // Death
    void Death_Enter()
    {
        OnDied?.Invoke(this);

        StopAllCoroutines();
        StopMovement();
        rig.isKinematic = true;

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        
        deadParticle.transform.SetParent(null, true);
        deadParticle.Play(true);

        var main = deadParticle.main;
        float lifeTime = main.duration + main.startLifetime.constantMax;
        Destroy(deadParticle.gameObject, lifeTime);

        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = false;
        }

        afterDead?.Invoke();
        Destroy(gameObject);
    }
    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hurtPlayerPoint.position, hurtPlayerRange);

        // draw edge check point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(edgeCheckPoint.position, edgeCheckPoint.position + Vector3.down * rayDistance);
    }

    #endregion
}
