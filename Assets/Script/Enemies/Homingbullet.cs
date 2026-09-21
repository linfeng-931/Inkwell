using UnityEngine;
using System.Collections;

public class Homingbullet : MonoBehaviour, IEnemy
{
    [Header("Attribute Setting")]
    public int damage = 1;
    public float explodeWaitTime = 0.5f;
    public float rotateSpeed = 200f;
    public float speed = 6f;
    public float lifeTime = 5f;

    [Header("Animate Setting")]
    public string startAni;
    public string normalAni;
    public string endAni;

    [Header("Detect Setting")]
    public float explodeZone = 1f;

    [Header("Particle Setting")]
    public ParticleSystem readyExplodeParticle;
    public ParticleSystem explodeParticle;

    private Transform target;
    private Rigidbody rig;
    private Animator animator;
    private bool readyExplode = false;
    private bool startAniEnd = false;
    private Vector3 direction;
    private float startTime;

    void Awake()
    {
        startTime = Time.time;
        animator = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        rig.useGravity = false;
        if (startAni != "") animator.Play(startAni, 0);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update() {
        // check and change animate step
        if(startAni != "" && !startAniEnd)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(startAni) && stateInfo.normalizedTime >= 1.0f)
            {
                animator.Play(normalAni);
                startAniEnd = true;
            }
        }

        if((startTime+lifeTime) <= Time.time)
        {
            TriggerExplosion();
        }
    }

    void FixedUpdate()
    {
        if(readyExplode) return;

        // if lost target
        if (target == null)
        {
            rig.linearVelocity = transform.right * speed;
            return;
        }

        // direction to target
        direction = (target.position - transform.position).normalized;
        direction.z = 0f;

        rig.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(readyExplode) return;

        if (other.CompareTag("Player") && target.CompareTag("Player"))
        {
            other.gameObject.transform.GetComponent<PlayerHealth>().TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            TriggerExplosion();
        }
    }

    /// <summary>
    /// ready to explode
    /// </summary>
    private void TriggerExplosion()
    {
        if (!readyExplode)
        {
            readyExplode = true;
            rig.linearVelocity = Vector3.zero;
            StartCoroutine(ExplodeSequence());
        }
    }

    IEnumerator ExplodeSequence()
    {
        if(readyExplodeParticle != null) readyExplodeParticle.Play();

        yield return new WaitForSeconds(explodeWaitTime);

        if (endAni != "") animator.Play(endAni);
        if (readyExplodeParticle != null) 
        {
            readyExplodeParticle.Stop();
        }
        if (explodeParticle != null)
        {
            // isolate particle obj
            explodeParticle.transform.SetParent(null); 
            explodeParticle.Play();
            
            Destroy(explodeParticle.gameObject, 2f); 
        }

        // handle explosion
        if (Vector3.Distance(transform.position, target.position) <= explodeZone)
        {
            if (target.CompareTag("Player"))
            {
                target.GetComponent<PlayerHealth>().TakeDamage(damage, transform.position);
            }
        }

        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        readyExplode = true;
        rig.linearVelocity = Vector3.zero;
        animator.Play(endAni);
        Destroy(gameObject, 0.5f);
    }

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeZone);
    }

    #endregion
}
