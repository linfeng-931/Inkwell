using UnityEngine;

public class Needle : MonoBehaviour
{
    public bool isAct;
    public float needleSpeed;
    
    private float timer;
    private Rigidbody needleRig;
    private Collider needleCol;
    private Transform playerTrans;
    private PlayerHealth playerHealth;
    private Transform startPos;

    void Awake()
    {
        timer = 0f;
        needleRig = GetComponent<Rigidbody>();
        needleCol = GetComponent<Collider>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTrans = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if(!isAct) return;
        timer += Time.deltaTime;
        if(timer >= 4f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Weapon"))
        {
            playerHealth.TakeDamage(1, startPos.position);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            needleRig.linearVelocity = Vector3.zero;
            needleCol.enabled = false;
            isAct = false;
            Destroy(gameObject);
        }
    }

    public void Shoot(Transform startPos)
    {
        this.startPos = startPos;
        transform.SetParent(null);
        needleRig.isKinematic = false;
        isAct = true;
        Vector3 target = (playerTrans.position - transform.position).normalized;
        target.z = 0f;
        needleRig.linearVelocity = target*needleSpeed;
    }
}
