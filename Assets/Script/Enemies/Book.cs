using UnityEngine;

public class Book : MonoBehaviour
{
    public GameObject attackParticle;

    private PlayerController playerController;
    private Transform playerTrans;
    private Rigidbody playerRig;
    private Animator playerAni;
    private PlayerStatus playerStatus;
    private bool isAttack;
    private bool canAttack;
    private float timer;
    private int attackStep;
    private Animator animator;
    private Vector3 oriScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTrans = player.transform;
        playerController = player.GetComponent<PlayerController>();
        playerRig = player.GetComponent<Rigidbody>();
        playerAni = player.transform.GetChild(0).GetComponent<Animator>();
        playerStatus = player.GetComponent<PlayerStatus>();
        animator = GetComponent<Animator>();
        timer = 0f;
        attackStep = 0;
        attackParticle.SetActive(false);
    }

    // Update is called once per frame
    void Update()
{
    float dist = Vector3.Distance(playerTrans.position, transform.position);

    if(!isAttack)
    {
        if(dist < 2.0f)
        {
            timer += Time.deltaTime;
            if(timer > 3f)
            {
                animator.SetTrigger("attack");
                isAttack = true;
                timer = 0f;
                attackStep = 1;
            }
        }
        else
        {
            timer = 0f;
        }
    }
    else
    {
        timer += Time.deltaTime;

        if(attackStep >= 2) 
        {
            playerTrans.position = new Vector3(transform.position.x, transform.position.y + 1f, playerTrans.position.z);
        }

        if(timer > 0.5f && attackStep == 1)
        {
            canAttack = true;
            attackStep = 2;
        }

        if(timer > 3f && attackStep == 2)
        {
            animator.SetTrigger("disAttack");
            attackStep = 3;
            playerController.isInteract = false;
            playerRig.linearVelocity = Vector3.zero;
            playerRig.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            playerTrans.localScale = oriScale;
            attackParticle.SetActive(false);
        }

        if(timer > 3.4f && attackStep == 3)
        {
            isAttack = false;
            timer = 0f;
            canAttack = false;
            attackStep = 0;
        }
    }
}

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && canAttack)
        {
            playerStatus.blood--;
            playerController.isInteract = true;
            playerTrans.position = new Vector3(transform.position.x, transform.position.y + 1f, playerTrans.position.z);
            canAttack = false;
            playerAni.SetInteger("action", 0);
            oriScale = playerTrans.localScale;
            playerTrans.localScale = playerTrans.localScale * 0.5f;
            attackParticle.SetActive(true);
        }
    }
}
