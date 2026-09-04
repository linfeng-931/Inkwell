using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public Interaction interaction;
    public Transform point;
    public Animator inkAni;
    
    private PlayerController playerController;
    private bool isSaved;
    private bool saving;
    private bool ready;
    private float timer;
    
    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        isSaved = false;
        ready = false;
        saving = false;
        timer = 0f;
    }

    void Update()
    {
        // if (interaction.canInteract)
        // {
        //     if (!saving)
        //     {
        //         if(!ready){
        //             playerController.SetUpForInteraction(point.position, 0);
        //             ready = true;
        //         }
        //         if(!playerController.isGoTarget){
        //             saving = true;
        //             playerController.Saving();
        //             inkAni.SetTrigger("inkUp");
        //         }
        //         return;
        //     }

        //     timer += Time.deltaTime;
        //     if(timer > 1f)
        //     {
        //         playerController.OtherAni.SetInteger("action", 0);
        //     }
        //     if(timer > 1.11f)
        //     {
        //         playerController.DeSaving();
        //         interaction.canInteract = false;
        //     }
        // }
    }
}
