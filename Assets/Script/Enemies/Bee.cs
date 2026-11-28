using UnityEngine;
using System.Collections;
using System;
using MonsterLove.StateMachine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Bee : MonoBehaviour, IEnemy
{
    public enum States { Idle, Chase, Attack, Hurt, Death }
    private StateMachine<States> fsm;

    [Header("Move Setting")]
    public float dartSpeed = 10f;
    public float chaseSpeed = 3.5f;
    public float idleRadius = 4f; // random move range during idle
    public LayerMask obstacleLayer;
    public float obstacleCheckDist = 1f;

    [Header("Detect Setting")]
    public float sightRange = 8f;
    public float attackRange = 5f;
    
    private Transform player;

    [Header("Attack Setting")]
    public GameObject needlePrefab;
    public Transform attackStartPoint;

    [Header("Attribute")]
    public int maxHealth = 3;
    private int currentHealth;

    private Rigidbody rig;
    private bool facingRight = false;
    private Vector3 bornPos;

    [Header("Animate")]
    public Animator bodyAnimator;
    public Animator wingAnimator;
    public string idleAni = "Idle";
    public string attackAni = "Attack";
    public string damagedAni = "Damaged";

    [Header("Separation Setting")]
    public float separationRadius = 1.5f; // detect enemy
    public float separationWeight = 2f;
    public LayerMask enemyLayer;

    [Header("Hurt Setting")]
    public float hurtForceH = 5f;
    public float hurtForceV = 2f;
    public float hurtStunTime = 0.6f;
    public float hurtFlashTime = 0.1f;

    [Header("Hurt and Death Effect")]
    public ParticleSystem hurtParticle;
    public ParticleSystem deadParticle;
    public Material hurtMaterial;
    public SpriteRenderer[] spriteRenderers;

    private Material[] originalMaterials;

    [Header("SFX")]
    public AudioManager audioManager;
    public AudioClip atkSfx;

    [Header("Event")]
    public UnityEvent afterDead;
    public event Action<Bee> OnDied;

    private Coroutine idleCoroutine;
    private Coroutine attackCoroutine;

    // prevent repeated damage from OnTriggerEnter
    private float nextDamageTime = 0f;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();

        currentHealth = maxHealth;
        bornPos = transform.position;

        rig.useGravity = false;
        rig.linearDamping = 2f; // flying drag

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        originalMaterials = new Material[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalMaterials[i] = spriteRenderers[i].sharedMaterial;

        fsm = StateMachine<States>.Initialize(this);
    }

    void Start()
    {
        player = MapManager.Instance.player.transform;
        audioManager = MapManager.Instance.audioManager;
        fsm.ChangeState(States.Idle);
    }

    void Update()
    {
        if (fsm.State == States.Death)
            return;
    }


    private void FlipToward(float targetX)
    {
        if ((targetX > transform.position.x && !facingRight) || (targetX < transform.position.x && facingRight))
        {
            facingRight = !facingRight;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1; 
            transform.localScale = currentScale;
        }
    }

    /// <summary>
    /// when idle state, enemy go to new pos radomly
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomClearPoint()
    {
        for (int i = 0; i < 5; i++) // find five times
        {
            // find random point according to x, y and radius
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * idleRadius;

            Vector3 randomPoint =
                bornPos + new Vector3(randomCircle.x, randomCircle.y, 0f);

            Vector3 offset = randomPoint - transform.position;
            float dist = offset.magnitude;

            // random point is too close
            if (dist <= 0.05f)
                continue;

            Vector3 dir = offset / dist;

            // check the point isn't in wall or ground
            if (!Physics.SphereCast(
                    transform.position,
                    0.5f,
                    dir,
                    out RaycastHit hit,
                    dist,
                    obstacleLayer,
                    QueryTriggerInteraction.Ignore))
            {
                // make sure target point isn't inside wall or ground
                if (!Physics.CheckSphere(
                        randomPoint,
                        0.5f,
                        obstacleLayer,
                        QueryTriggerInteraction.Ignore))
                {
                    return randomPoint;
                }
            }
        }

        return transform.position; // stay in place
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
    private bool IsAllyAhead(Vector3 flightDirection)
    {
        return Physics.SphereCast(
            transform.position,
            0.5f, // size of bee
            flightDirection,
            out RaycastHit hit,
            separationRadius,
            enemyLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private bool IsPlayerInSight()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.z = 0f;
        return toPlayer.sqrMagnitude <= sightRange * sightRange;
    }

    private void SetHurtMaterial(bool isHurt)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sharedMaterial = isHurt ? hurtMaterial : originalMaterials[i];
        }
    }


    #region state

    // Idle
    void Idle_Enter()
    {
        bodyAnimator.Play(idleAni, 0);
        StopMovement();
        idleCoroutine = StartCoroutine(IdleMovementRoutine());
    }


    void Idle_Exit()
    {
        StopCoroutine(idleCoroutine);
        idleCoroutine = null;

        StopMovement();
    }


    IEnumerator IdleMovementRoutine()
    {
        // monsterLove can use while(true)
        while (fsm.State == States.Idle)
        {
            // stop
            StopMovement();

            yield return new WaitForSeconds(
                UnityEngine.Random.Range(1f, 2.5f)
            );

            if (fsm.State != States.Idle)
                yield break;

            // search new pos
            Vector3 targetPoint = GetRandomClearPoint();

            // no valid target point
            if (Vector3.Distance(transform.position, targetPoint) <= 0.05f)
                continue;

            FlipToward(targetPoint.x);

            // dart between two points
            while (fsm.State == States.Idle)
            {
                // recalculate direction every loop
                // prevent Bee from flying past target
                Vector3 offset = targetPoint - transform.position;
                offset.z = 0f;

                float distance = offset.magnitude;

                if (distance <= 0.1f)
                    break;

                Vector3 dir = offset.normalized;

                // obstacle check
                if (Physics.SphereCast(
                        transform.position,
                        0.5f,
                        dir,
                        out RaycastHit hit,
                        obstacleCheckDist,
                        obstacleLayer,
                        QueryTriggerInteraction.Ignore))
                {
                    // if enemy will touch wall and ground, stop
                    StopMovement();
                    break;
                }

                rig.linearVelocity = dir * dartSpeed;

                yield return new WaitForFixedUpdate();
            }

            StopMovement();
        }
    }


    void Idle_Update()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.z = 0f;

        if (toPlayer.sqrMagnitude <= sightRange * sightRange)
        {
            fsm.ChangeState(States.Chase);
        }
    }


    // Chase
    void Chase_Enter()
    {
        bodyAnimator.Play(idleAni, 0);
        StopMovement();
    }


    void Chase_Update()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.z = 0f;

        float distToPlayer = toPlayer.magnitude;

        if (distToPlayer > sightRange)
        {
            fsm.ChangeState(States.Idle);
            return;
        }

        if (distToPlayer <= attackRange)
        {
            fsm.ChangeState(States.Attack);
            return;
        }

        Vector3 dirToPlayer = toPlayer.normalized;

        // if too close with player, don't flip
        float distanceX = player.position.x - transform.position.x;
        float deadZone = 0.8f;

        if (Mathf.Abs(distanceX) > deadZone)
        {
            FlipToward(player.position.x);
        }

        // if enemy will touch wall and ground
        bool isObstacleAhead = Physics.SphereCast(
            transform.position,
            0.5f,
            dirToPlayer,
            out RaycastHit hit,
            obstacleCheckDist,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );

        // if enemy will touch other enemy
        bool isAllyAhead = IsAllyAhead(dirToPlayer);
        
        if (isObstacleAhead || isAllyAhead)
        {
            rig.linearVelocity = Vector3.Lerp(
                rig.linearVelocity,
                Vector3.zero,
                Time.deltaTime * 5f
            );
        }
        else
        {
            rig.linearVelocity = dirToPlayer * chaseSpeed;
        }
    }


    void Chase_Exit()
    {
        StopMovement();
    }


    // Attack
    void Attack_Enter()
    {
        StopMovement();
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    void Attack_Exit()
    {
        StopCoroutine(attackCoroutine);
        attackCoroutine = null;
        
        StopMovement();
    }

    IEnumerator AttackRoutine()
    {
        FlipToward(player.position.x);

        yield return new WaitForSeconds(0.5f);

        bodyAnimator.Play(attackAni, 0);

        GameObject needle = Instantiate(needlePrefab, attackStartPoint.position, Quaternion.identity);
        if (needle.TryGetComponent<Needle>(out Needle needleScript))
        {
            needleScript.Shoot(transform);
        }

        yield return new WaitForSeconds(1.5f);

        if (fsm.State == States.Attack)
        {
            fsm.ChangeState(States.Chase);
        }
    }


    // Hurt
    IEnumerator Hurt_Enter()
    {
        StopMovement();

        bodyAnimator.Play(damagedAni, 0);
        SetHurtMaterial(true);
        if (hurtParticle != null) hurtParticle.Play();

        FlipToward(player.position.x);

        float knockbackDirX = transform.position.x >= player.position.x ? 1f : -1f;

        rig.AddForce(
            new Vector3(knockbackDirX * hurtForceH, hurtForceV, 0f),
            ForceMode.Impulse
        );

        yield return new WaitForSeconds(hurtFlashTime);
        SetHurtMaterial(false);

        yield return new WaitForSeconds(hurtStunTime);

        if (IsPlayerInSight())
            fsm.ChangeState(States.Chase);
        else
            fsm.ChangeState(States.Idle);
    }

    void Hurt_Exit()
    {
        SetHurtMaterial(false);
        StopMovement();
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


    #region col and trigger

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // prevent repeated damage
        if (Time.time < nextDamageTime)
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        nextDamageTime = Time.time + 0.5f;

        playerHealth.TakeDamage(1, transform.position);
    }

    #endregion


    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            separationRadius
        );

        Gizmos.color = Color.green;

        Vector3 idleCenter =
            Application.isPlaying
                ? bornPos
                : transform.position;

        Gizmos.DrawWireSphere(
            idleCenter,
            idleRadius
        );
    }

    #endregion

    private void StopMovement()
    {
        rig.linearVelocity = Vector3.zero;
    }
}