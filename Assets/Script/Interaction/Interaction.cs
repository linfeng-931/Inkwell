using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

public enum InteractActionType
{
    Dialogue,
    Timeline,
    Other
}

[System.Serializable]
public class InteractAction
{
    public InteractActionType actionType;

    [ShowIf(nameof(actionType), InteractActionType.Dialogue)]
    public string dialogueId;

    [ShowIf(nameof(actionType), InteractActionType.Dialogue)]
    public DialogueType dialogueType = DialogueType.Large;

    [ShowIf(nameof(actionType), InteractActionType.Timeline)]
    public TimelineSequenceController timeline;

    [ShowIf(nameof(actionType), InteractActionType.Other)]
    public UnityEvent otherInteract;
}

public class Interaction : MonoBehaviour
{
    [Header("Setting")]
    public float detectionRange;
    public GameObject interactionKeyUI;
    public Animator keyAnimator;
    public Transform dialogueAnchor;
    public bool interactOnce = true;

    [Header("After Interact")]
    public List<InteractAction> interactionSequence = new List<InteractAction>();

    private Transform playerTrans;
    private bool isPlayerInRange = false;
    private bool hasInteracted = false;
    private bool puzzleComplete = false;

    void Start()
    {
        playerTrans = MapManager.Instance.player.transform;
    }

    void Update()
    {
        if (puzzleComplete || (interactOnce && hasInteracted)) return;

        // detect player position
        float distance = Vector3.Distance(playerTrans.position, transform.position);
        bool inRange = distance < detectionRange;

        // change status
        if (inRange != isPlayerInRange)
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
        if (!isPlayerInRange || puzzleComplete || (interactOnce && hasInteracted)) return;

        if (interactOnce)
        {
            hasInteracted = true;
            UpdateUIState(false);
            GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
        }

        Transform targetAnchor = dialogueAnchor != null ? dialogueAnchor : transform;

        foreach (var action in interactionSequence)
        {
            switch (action.actionType)
            {
                case InteractActionType.Dialogue:
                    DialogueManager.Instance.TryPlayDialogue(
                        action.dialogueId,
                        action.dialogueType,
                        targetAnchor,
                        false
                    );
                    break;

                case InteractActionType.Timeline:
                    action.timeline.TriggerTimeline();
                    break;

                case InteractActionType.Other:
                    action.otherInteract?.Invoke();
                    break;
            }
        }
    }

    public void SetPuzzleComplete()
    {
        puzzleComplete = true;
        UpdateUIState(false);

        if (isPlayerInRange)
        {
            GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
            isPlayerInRange = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
