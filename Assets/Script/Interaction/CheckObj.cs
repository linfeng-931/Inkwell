using UnityEngine;

public class CheckObj : MonoBehaviour
{
    public Interaction interaction;
    public GameObject checkedObj;

    private PlayerController playerController;
    private bool isAct;

    void Start()
    {
        checkedObj.SetActive(false);
        isAct = false;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (interaction.canInteract && !isAct)
        {
            isAct = true;
            playerController.isInteract = true;
            checkedObj.SetActive(true);
        }

        if (isAct)
        {
            if(!interaction.canInteract)
            {
                isAct = false;
                playerController.isInteract = false;
                checkedObj.SetActive(false);
            }
        }
    }
}
