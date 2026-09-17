using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Setting")]
    public float speed = 25f;

    [Header("Visual")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject impactVisual; // hit collider

    private bool isDead; // bullet status
    private float lifeTimer;
    private float deadTimer;
    private Vector3 moveDir;
    private int type = 0; // 0: Player, 1: Enemy
    private int damage = 2;

    /// <summary>
    /// for shooter
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="bulletType"></param>
    public void Initialize(Vector3 direction, int bulletType, int bulletDamage)
    {
        moveDir = direction.normalized;
        moveDir.z = 0;
        
        type = bulletType;
        isDead = false;
        damage = bulletDamage;
        
        normalVisual.SetActive(true);
        impactVisual.SetActive(false);
    }

    void Update()
    {
        if (isDead)
        {
            deadTimer += Time.deltaTime;
            if (deadTimer > 0.5f) Destroy(gameObject);
            return;
        }

        // fly
        transform.position += moveDir * speed * Time.deltaTime;
        
        lifeTimer += Time.deltaTime;
        if (lifeTimer > 3f)
        {
            TriggerImpact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // ignore object according to type
        if (type == 0 && (other.CompareTag("Player") || other.CompareTag("Body"))) return;
        if (type == 1 && other.CompareTag("Enemy")) return;

        // hit any object
        TriggerImpact();

        // hurt
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<IEnemy>().TakeDamage(damage);
        }
    }

    /// <summary>
    /// if trigger, handle the impact
    /// </summary>
    private void TriggerImpact()
    {
        isDead = true;
        normalVisual.SetActive(false);
        impactVisual.SetActive(true);
    }
}