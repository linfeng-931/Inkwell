using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;

[RequireComponent(typeof(Rigidbody))]
public class Silverfish : MonoBehaviour, IEnemy
{
    public enum States { Idle, Patrol, Chase, Attack, Hurt, Death }
    private StateMachine<States> fsm;

    [Header("Attribute Setting")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Detect Setting")]
    public float sightRange = 6f;
    public float attackRange = 1.5f;

    private Transform player;

    [Header("Prevent Setting")]
    public Transform edgeCheckPoint;
    public float rayDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Separation Setting")]
    public float separationRadius = 0.65f; // detect enemy
    public LayerMask enemyLayer;

    [Header("Attack Setting")]
    public float attackCastTime = 0.3f;
    public float attackInterval = 1f;

    [Header("Hurt Setting")]
    public float hurtForceH = 0.1f;
    public float hurtForceV = 0.1f;
    public float hurtStunTime = 0.3f;

    [Header("Animate")]
    public Animator animator;
    public string idleAni = "Silverfish_Idle";
    public string walkAni = "Silverfish_Walk";
    public string attackAni = "Silverfish_Attack";
    public string attackEndAni = "Silverfish_Attack_End";
    public string damagedAni = "Silverfish_Damaged";

    [Header("SFX")]
    public AudioManager audioManager;
    public AudioClip atkSfx;

    private Rigidbody rig;
    private bool facingRight = false;
    private PlayerHealth playerHealth;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();

        currentHealth = maxHealth;

        // Prevent unwanted rotation
        rig.constraints = RigidbodyConstraints.FreezeRotation;

        fsm = StateMachine<States>.Initialize(this, States.Idle);
    }

    void Start()
    {
        player = MapManager.Instance.player.transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        audioManager = MapManager.Instance.audioManager;
        fsm.ChangeState(States.Idle);
    }

    void Update()
    {
        if (fsm.State == States.Death) return;
    }

    /// <summary>
    /// check if enemy will touch wall or edge
    /// </summary>
    /// <returns></returns>
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

    private void Turn()
    {
        facingRight = !facingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

    private void FlipToward(float targetX)
    {
        float distanceX =
            targetX - transform.position.x;

        // too close, don't flip
        if (Mathf.Abs(distanceX) < 0.05f)
            return;

        bool targetFacingRight =
            distanceX > 0f;

        // already facing target direction
        if (targetFacingRight == facingRight) return;

        Turn();
    }

    private bool IsPlayerInSight()
    {
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

    public void TakeDamage(int damage)
    {
        if (fsm.State == States.Death) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            fsm.ChangeState(States.Death, StateTransition.Overwrite);
        }
        else
        {
            fsm.ChangeState(States.Hurt, StateTransition.Overwrite);
        }
    }

    /// <summary>
    /// detect other enemy
    /// </summary>
    /// <returns></returns>
    private bool IsAllyAhead()
    {
        Vector3 forwardDir = facingRight ? Vector3.right : Vector3.left;
        return Physics.Raycast(transform.position, forwardDir, separationRadius, enemyLayer);
    }

    #region State

    // Idle (stop)
    IEnumerator Idle_Enter()
    {
        animator.Play(idleAni, 0);
        StopMovement();

        yield return new WaitForSeconds(
            Random.Range(2f, 3f)
        );

        if (fsm.State == States.Idle)
        {
            fsm.ChangeState(States.Patrol);
        }
    }

    void Idle_Update()
    {
        if (IsPlayerInSight())
        {
            fsm.ChangeState(States.Chase);
        }
    }

    // Idle (Patrol)
    void Patrol_Enter()
    {
        animator.Play(walkAni, 0);
        if (Random.value > 0.5f)
        {
            Turn();
        }
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
            StopHorizontalMovement();
            fsm.ChangeState(States.Idle);
            return;
        }

        rig.linearVelocity = new Vector3(
            (facingRight ? 1f : -1f) * patrolSpeed,
            rig.linearVelocity.y,
            0f
        );
    }

    // Chase
    void Chase_Enter()
    {
        animator.Play(walkAni, 0);
        StopHorizontalMovement();
    }

    void Chase_Update()
    {
        if (!IsPlayerInSight())
        {
            fsm.ChangeState(States.Idle);
            return;
        }

        if (IsPlayerInAttackRange())
        {
            StopHorizontalMovement();

            fsm.ChangeState(States.Attack);
            return;
        }

        // check player dir compared by this pos
        float dirToPlayer = player.position.x - transform.position.x;

        // if too close with player, don't flip
        float deadZone = 0.8f;

        if (Mathf.Abs(dirToPlayer) > deadZone)
        {
            FlipToward(player.position.x);
        }

        // prevent enemy from walking into wall or edge or other enemy
        if (IsPathBlockedOrEdge() || IsAllyAhead())
        {
            // stop, prevent to touch wall or drop from platform
            StopHorizontalMovement();
        }
        else
        {
            rig.linearVelocity = new Vector3(
                (facingRight ? 1f : -1f) * chaseSpeed,
                rig.linearVelocity.y,
                0f
            );
        }
    }

    void Chase_Exit()
    {
        StopHorizontalMovement();
    }

    // Attack
    IEnumerator Attack_Enter()
    {
        StopMovement();

        FlipToward(player.position.x);

        animator.Play(idleAni, 0);
        yield return new WaitForSeconds(attackCastTime); // ready

        animator.Play(attackAni, 0);
        audioManager.PlaySFX(atkSfx);
        yield return new WaitForSeconds(0.03f); // go

        if (fsm.State == States.Attack && IsPlayerInAttackRange())
        {
            playerHealth.TakeDamage(1, transform.position);
        }

        yield return new WaitForSeconds(0.25f);
        animator.Play(attackEndAni, 0);
        yield return new WaitForSeconds(attackInterval);
        if (fsm.State == States.Attack)
        {
            fsm.ChangeState(States.Chase);
        }
    }

    // Hurt
    IEnumerator Hurt_Enter()
    {
        StopMovement();
        animator.Play(damagedAni, 0);
        FlipToward(player.position.x);
        Vector3 knockbackDirection = new Vector3(facingRight ? -1f : 1f, 0f, 0f);

        rig.AddForce(
            knockbackDirection * hurtForceH + Vector3.up * hurtForceV,
            ForceMode.Impulse
        );

        yield return new WaitForSeconds(hurtStunTime); // hit stun

        if (IsPlayerInSight())
        {
            fsm.ChangeState(States.Chase);
        }
        else
        {
            fsm.ChangeState(States.Idle);
        }
    }

    // Death
    void Death_Enter()
    {
        StopAllCoroutines();

        StopMovement();
        rig.isKinematic = true;

        // disable all colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 2f);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // draw edge check point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(edgeCheckPoint.position, edgeCheckPoint.position + Vector3.down * rayDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }

    #endregion

    private void StopMovement()
    {
        rig.linearVelocity = Vector3.zero;
    }

    private void StopHorizontalMovement()
    {
        // Keep Y velocity for gravity
        rig.linearVelocity = new Vector3(0f, rig.linearVelocity.y, 0f);
    }
}