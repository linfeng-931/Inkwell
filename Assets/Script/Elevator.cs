using UnityEngine;

public class Elevator : MonoBehaviour
{
    public Interaction interaction;
    public CameraController cameraController;
    public Transform point;
    public bool isAct;
    public bool startAct;
    public bool endAct;
    public bool dir; //true-up or false-down
    public bool disScript;

    private Animator animator;
    private GameObject player;
    private PlayerController playerController;
    private Animator playerAnimator;
    private bool changeStatus;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("dir", dir);
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
        playerController = player.GetComponent<PlayerController>();
        playerAnimator = player.transform.GetChild(0).GetComponent<Animator>();
        changeStatus = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!disScript)
        {
            if(isAct && !changeStatus){
                cameraController.enabled = false;
                playerController.enabled = false;
                playerAnimator.SetInteger("action", 0);

                if(playerController.transform.parent != this.transform)
                {
                    playerController.transform.SetParent(this.transform);
                }
                    changeStatus = true;
            }
            if (isAct)
            {
                player.transform.position = point.position;
            }
        }
        

        EndAct();
    }

    public void StartAct()
    {
        if(!startAct) return;

        playerController.transform.SetParent(null);
        cameraController.enabled = true;
        playerController.enabled = true;
        playerAnimator.speed = 0;
        isAct = false;
        changeStatus = false;
    }
    public void RePlayerAni()
    {
        playerAnimator.speed = 1f;
    }

    public void EndAct()
    {
        if(!endAct) return;

        if (interaction.canInteract)
        {
            isAct = true;
            changeStatus = false;
            animator.SetTrigger("isAct");
            dir = !dir;
            animator.SetBool("dir", dir);
            endAct = false;
        } 
    }
}
