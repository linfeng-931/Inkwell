using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;

[RequireComponent(typeof(Rigidbody))]
public class Mud : MonoBehaviour
{
    public enum States { Idle, Attack, Hurt, Death }
    private StateMachine<States> fsm;

    [Header("Detect Setting")]
    public float sightRange = 10f;
    public Transform player;

    [Header("Attack Setting")]
    public float attackCooldown = 2.5f;
    public GameObject mudBulletPrefab;
    public Transform attackStartPoint;

    [Header("Attribute")]
    public int maxHealth = 4;
    private int currentHealth;

    private Rigidbody rig;
    private bool facingRight = true;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        fsm = StateMachine<States>.Initialize(this, States.Idle);
    }

    void Update()
    {
        if (fsm.State == States.Death) return;
    }

    private void FlipTowards(float targetX)
    {
        if ((targetX > transform.position.x && !facingRight) || (targetX < transform.position.x && facingRight))
        {
            facingRight = !facingRight;
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
        if (currentHealth <= 0) fsm.ChangeState(States.Death);
        else fsm.ChangeState(States.Hit);
    }

    // Idle
    void Idle_Enter()
    {
        
    }

    void Idle_Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= sightRange)
        {
            fsm.ChangeState(States.Attack);
        }
    }

    // Attack
    IEnumerator Attack_Enter()
    {
        // shot mud continuously
        while (true)
        {
            // before attack
            // animator.Play("Mud_Spit_Windup");
            yield return new WaitForSeconds(0.5f);

            // create new mud bullet
            GameObject bullet = Instantiate(mudBulletPrefab, attackStartPoint.position, Quaternion.identity);

            // set target at homing bullet script
            if (bullet.TryGetComponent<Homingbullet>(out Homingbullet homingScript))
            {
                homingScript.SetTarget(player);
            }

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    void Attack_Update()
    {
        if (player == null) return;

        // face player
        FlipTowards(player.position.x);


        // check player distance
        if (Vector3.Distance(transform.position, player.position) > sightRange)
        {
            fsm.ChangeState(States.Idle);
        }
    }

    // Hurt
    IEnumerator Hurt_Enter()
    {
        yield return new WaitForSeconds(0.4f);
        fsm.ChangeState(States.Attack);
    }

    // Death
    void Death_Enter()
    {
        rig.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        // animator.Play("Mud_Melt");
        Destroy(gameObject, 1.5f);
    }
}
