using UnityEngine;

public class CheckObj : MonoBehaviour
{
    public Interaction interaction;
    public GameObject checkedObj;

    private PlayerController playerController;
    private Rigidbody playerRig;
    private bool isAct;

    void Start()
    {
        checkedObj.SetActive(false);
        isAct = false;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRig = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (interaction.canInteract && !isAct)
        {
            isAct = true;
            //playerController.isInteract = true;
            playerRig.linearVelocity = new Vector3(0, playerRig.linearVelocity.y, 0);
            checkedObj.SetActive(true);
        }

        if (isAct)
        {
            if(!interaction.canInteract)
            {
                isAct = false;
                //playerController.isInteract = false;
                checkedObj.SetActive(false);
            }
        }
    }
}
