using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    [Header("Setting")]
    public float detectionRange;
    public GameObject interactionKeyUI;
    public Animator keyAnimator;

    [Header("Interaction Type")]
    public string targetDialogueId;
    public TimelineSequenceController targetTimeline;

    private Transform playerTrans;
    private bool isPlayerInRange = false;
    private bool hasInteracted = false;
    private bool puzzleComplete = false;

    void Start()
    {
        playerTrans = MapManager.Instance.player.transform;;
    }

    void Update()
    {
        if(puzzleComplete || hasInteracted) return;

        // detect player position
        float distance = Vector3.Distance(playerTrans.position, transform.position);
        bool inRange = distance < detectionRange;

        // change status
        if(inRange != isPlayerInRange)
        {
            isPlayerInRange = inRange;
            UpdateUIState(isPlayerInRange);

            if (isPlayerInRange)
            {
                GameEvent.OnWorldInteractPressed += HandleInteractPressed;
            }
            else
            {
                GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
            }
        }
    }

    private void OnDisable()
    {
        GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
    }

    private void UpdateUIState(bool show)
    {
        if (show)
        {
            keyAnimator.SetTrigger("show");
        }
        else
        {
            keyAnimator.SetTrigger("hide");
        }
    }
    
    private void HandleInteractPressed()
    {
        if(!isPlayerInRange || hasInteracted) return;

        hasInteracted = true;
        UpdateUIState(false);
        GameEvent.OnWorldInteractPressed -= HandleInteractPressed;

        if(targetDialogueId != "")
        {
            DialogueManager.Instance.TryPlayDialogue(targetDialogueId, false);
        }
        
        if(targetTimeline != null)
        {
            targetTimeline.TriggerTimeline();
        }
    }

    public void SetPuzzleComplete()
    {
        puzzleComplete = true;
        UpdateUIState(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
