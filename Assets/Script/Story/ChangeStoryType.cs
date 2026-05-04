using UnityEngine;

public class ChangeStoryType : MonoBehaviour
{
    public bool story0;
    public bool story1;

    private PlayerController playerController;

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(story0){
                playerController.isStory = true;
                playerController.isStory2 = false;
            }
            else if(story1){
                playerController.isStory = false;
                playerController.isStory2 = true;
            }
        }
    }
}
