using UnityEngine;

public class CatchObj : MonoBehaviour
{
    public Interaction interaction;
    public GameObject interactionObj;
    public GameObject appearGameOject;

    void Update()
    {
        if (interaction.canInteract)
        {
            appearGameOject.SetActive(true);
            interactionObj.SetActive(false);
        }
    }
}
