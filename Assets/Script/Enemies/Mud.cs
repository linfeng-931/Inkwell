using UnityEngine;

public class Mud : MonoBehaviour
{
    public float launchHeight = 2.0f;
    private Rigidbody rig;
    private Transform playerTrans;
    private PlayerController playerController;
    private Collider col;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        LaunchTowardsPlayer();
    }

    void LaunchTowardsPlayer()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTrans.position;

        float displacementY = targetPos.y - startPos.y;
        Vector3 displacementXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);

        float gravity = Physics.gravity.y; 
        float finalLaunchHeight = Mathf.Max(launchHeight, displacementY + 0.5f);

        // Vy
        float velocityY = Mathf.Sqrt(-2 * gravity * finalLaunchHeight);

        // t
        float discriminant = velocityY * velocityY + 2 * gravity * (startPos.y - targetPos.y);

        if (discriminant < 0)
        {
            discriminant = 0;
        }

        float time = (-velocityY - Mathf.Sqrt(discriminant)) / gravity;

        if (time > 0.001f)
        {
            Vector3 velocityXZ = displacementXZ / time;
            Vector3 finalVelocity = new Vector3(velocityXZ.x, velocityY, velocityXZ.z);

            if (!float.IsNaN(finalVelocity.x) && !float.IsNaN(finalVelocity.z))
            {
                rig.AddForce(finalVelocity, ForceMode.VelocityChange);
            }
        }
        else
        {
            rig.AddForce(Vector3.up * velocityY, ForceMode.VelocityChange);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            col.enabled = false;
            Destroy(gameObject);
        }
        else
        {
            playerController.Hurt(1, 0, transform.position.x - playerTrans.position.x);
            Destroy(gameObject);
        }
    }
}