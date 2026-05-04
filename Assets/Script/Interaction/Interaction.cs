using UnityEngine;

public class Interaction : MonoBehaviour
{
    public float detectionRange;
    public GameObject InteractionKey;
    public bool canInteract;

    private bool readyInteractionKey;
    private GameObject player;
    private Transform playerTrans;
    private float interactionKeyScale;
    private float scaleChangeSpeed;
    private float timer;
    private bool puzzleComplete = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerTrans = player.transform;
        canInteract = false;
        readyInteractionKey = false;
        interactionKeyScale = 0f;
        InteractionKey.transform.localScale = new Vector3(interactionKeyScale, interactionKeyScale, 1);
        scaleChangeSpeed = 1;
        timer = 0f;
    }

    void Update()
    {
        if (puzzleComplete)
            if (puzzleComplete)
            {
                if (InteractionKey.transform.localScale != Vector3.zero)
                {
                    InteractionKey.transform.localScale = Vector3.zero;
                }
            }

        if (Vector3.Distance(playerTrans.position, transform.position) < detectionRange)
        {
            InteractionKeyIn();
            if (Input.GetKeyDown(KeyCode.E))
            {
                canInteract = true;
            }
        }
        else
        {
            InteractionKeyOut();
        }

        if (canInteract)
        {
            if(timer < 0.5f) timer += Time.deltaTime;
            
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) && timer >= 0.5f)
            {
                canInteract = false;
                timer = 0f;
            }
        }
    }

    void InteractionKeyIn()
    {
        if(readyInteractionKey) return;

        interactionKeyScale += Time.deltaTime*scaleChangeSpeed;
        if(interactionKeyScale >= 0.1f){
            interactionKeyScale = 0.1f;
            readyInteractionKey = true;
        }
        InteractionKey.transform.localScale = new Vector3(interactionKeyScale, interactionKeyScale, 1);
    }
    void InteractionKeyOut()
    {
        if(!readyInteractionKey) return;

        interactionKeyScale -= Time.deltaTime*scaleChangeSpeed;
        if(interactionKeyScale <= 0f){
            interactionKeyScale = 0f;
            readyInteractionKey = false;
        }
        InteractionKey.transform.localScale = new Vector3(interactionKeyScale, interactionKeyScale, 1);
    }

    public bool GetCanInteract()
    {
        return canInteract;
    }

    public void SetPuzzleComplete()
    {
        puzzleComplete = true;
    }
}
