using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    public Transform checkPoint;
    public Animator fadeAni;
    public bool reStart;

    private bool isAct;
    private float timer;
    private Transform playerTrans;
    private Rigidbody playerRb;
    private PlayerController playerController;

    void Start()
    {
        reStart = false;
        isAct = false;
        timer = 0f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTrans = player.transform;
        playerRb = player.GetComponent<Rigidbody>();
        playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (isAct)
        {
            timer += Time.deltaTime;
            if(timer > 1.5f){
                reStart = true;
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector3.zero; 
                    playerRb.angularVelocity = Vector3.zero;
                }
                playerTrans.position = checkPoint.position;
            }
            if(timer > 2f)
            {
                timer = 0f;
                isAct = false;
                reStart = false;
                fadeAni.SetTrigger("returnStart");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isAct && other.CompareTag("Player") && !playerController.isDash)
        {
            fadeAni.SetTrigger("changeScene");
            playerController.Hurt(1, 0, transform.position.x - playerTrans.position.x);
            isAct = true;
            timer = 0f;
        }
    }
}
