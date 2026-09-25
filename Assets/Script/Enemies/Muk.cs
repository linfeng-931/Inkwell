using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;

[RequireComponent(typeof(Rigidbody))]
public class Muk : MonoBehaviour, IEnemy
{
    public enum States { Idle, Attack, Hurt, Death }
    private StateMachine<States> fsm;

    [Header("Detect Setting")]
    public float sightRange = 10f;
    
    private Transform player;

    [Header("Attack Setting")]
    public float attackCooldown = 2.5f;
    public GameObject mudBulletPrefab;
    public Transform attackStartPoint;

    private float nextAttackTime = 0f;

    [Header("Attribute")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Animate")]
    public Animator animator;
    public string idleAni = "Muk_Idle";
    public string attackAni = "Muk_Attack";
    public string damagedAni = "Muk_Damaged";

    [Header("Hurt and Death Setting")]
    public ParticleSystem hurtParticle;
    public ParticleSystem deadParticle;
    public Material hurtMaterial;

    private Material originalMaterial;

    [Header("SFX")]
    public AudioManager audioManager;
    public AudioClip atkSfx;

    private Rigidbody rig;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        fsm = StateMachine<States>.Initialize(this, States.Idle);
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
    }

    void Start()
    {
        player = MapManager.Instance.player.transform;
        audioManager = MapManager.Instance.audioManager;
        fsm.ChangeState(States.Idle);
    }

    private void FlipTowards(float targetX)
    {
        if ((targetX > transform.position.x && !facingRight) || (targetX < transform.position.x && facingRight))
        {
            facingRight = !facingRight;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
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

    // Idle
    void Idle_Enter()
    {
        animator.Play(idleAni, 0);
    }

    void Idle_Update()
    {
        if (Time.time >= nextAttackTime && Vector3.Distance(transform.position, player.position) <= sightRange)
        {
            fsm.ChangeState(States.Attack);
        }
    }

    // Attack
    IEnumerator Attack_Enter()
    {
        // before attack
        animator.Play(attackAni, 0);
        yield return new WaitForSeconds(0.6f);

        // create new mud bullet
        GameObject bullet = Instantiate(mudBulletPrefab, attackStartPoint.position, Quaternion.identity);

        // set target at homing bullet script
        if (bullet.TryGetComponent<Homingbullet>(out Homingbullet homingScript))
        {
            homingScript.SetTarget(player);
        }

        yield return new WaitForSeconds(0.4f);

        nextAttackTime = Time.time + attackCooldown;
        fsm.ChangeState(States.Idle);
    }

    // Hurt
    IEnumerator Hurt_Enter()
    {
        animator.Play(damagedAni, 0);
        spriteRenderer.material = hurtMaterial;
        hurtParticle.Play();
        yield return new WaitForSeconds(0.1f);

        spriteRenderer.material = originalMaterial;
        yield return new WaitForSeconds(0.3f);

        fsm.ChangeState(States.Idle);
    }

    // Death
    void Death_Enter()
    {
        rig.linearVelocity = Vector3.zero;

        GetComponent<Collider>().enabled = false;

        deadParticle.transform.SetParent(null); 
        deadParticle.Play();
        Destroy(deadParticle.gameObject, 2f); 

        Destroy(gameObject, 0.5f);
    }
}
