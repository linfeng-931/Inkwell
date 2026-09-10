using UnityEngine;

public class Homingbullet : MonoBehaviour
{
    public float speed = 6f;
    public float rotateSpeed = 200f;
    public float lifeTime = 5f;

    private Transform target;
    private Rigidbody rig;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        rig.useGravity = false;
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void FixedUpdate()
    {
        // if lost target
        if (target == null)
        {
            rig.velocity = transform.right * speed;
            return;
        }

        // direction to player
        Vector3 direction = (target.position - transform.position).normalized;
        direction.z = 0f;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);

        rig.velocity = transform.right * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // TODO: 在這裡造成傷害並生成泥巴飛濺特效
            Destroy(gameObject);
        }
    }
}
