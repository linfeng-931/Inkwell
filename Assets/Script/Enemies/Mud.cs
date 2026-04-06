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

        //Vy
        float gravity = Physics.gravity.y;
        float velocityY = Mathf.Sqrt(-2 * gravity * launchHeight);

        //t
        float time = (-velocityY - Mathf.Sqrt(velocityY * velocityY - 2 * gravity * displacementY)) / gravity;

        //Vx
        Vector3 velocityXZ = displacementXZ / time;

        Vector3 finalVelocity = new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
        rig.AddForce(finalVelocity, ForceMode.VelocityChange);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            col.enabled = false;
            Destroy(gameObject); //或做效果
        }
        else
        {
            playerController.Hurt(1, 0, rig.linearVelocity.x);
            Destroy(gameObject);
        }
    }
}