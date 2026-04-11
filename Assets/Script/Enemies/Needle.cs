using UnityEngine;

public class Needle : MonoBehaviour
{
    public bool isAct;
    public float needleSpeed;
    
    private float timer;
    private Rigidbody needleRig;
    private Collider needleCol;
    private Transform playerTrans;
    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;
        needleRig = GetComponent<Rigidbody>();
        needleCol = GetComponent<Collider>();
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isAct) return;
        timer += Time.deltaTime;
        if(timer >= 10f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DrawMesh"))
        {
            //destory drawMesh
            other.transform.root.gameObject.GetComponent<DrawMesh>().isComplete = true; 

            Destroy(gameObject);
            return;
        }
        if (!other.CompareTag("Player") && !other.CompareTag("Weapon"))
        {
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            needleCol.enabled = false;
        }
        else
        {
            playerController.Hurt(1, 0, needleRig.linearVelocity.x);
        }
    }

    public void Shoot()
    {
        transform.SetParent(null);
        needleRig.isKinematic = false;
        isAct = true;
        Vector3 target = (playerTrans.position - transform.position).normalized;
        needleRig.linearVelocity = target*needleSpeed;
    }
}
