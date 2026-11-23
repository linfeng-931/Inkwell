using UnityEngine;
using UnityEngine.Events;
using TriInspector;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    public InteractActionType actionType = InteractActionType.Other;

    [Tooltip("dialogue")]
    [ShowIf(nameof(actionType), InteractActionType.Dialogue)]
    public string dialogueId;
    [ShowIf(nameof(actionType), InteractActionType.Dialogue)]
    public DialogueType dialogueType = DialogueType.Large;
    [ShowIf(nameof(actionType), InteractActionType.Dialogue)]
    public Transform dialogueAnchor;

    [Tooltip("other")]
    [ShowIf(nameof(actionType), InteractActionType.Other)]
    public UnityEvent onPlayerEnter;
    [ShowIf(nameof(actionType), InteractActionType.Other)]
    public UnityEvent onPlayerExit;

    private bool hasTriggered;
    private Transform TargetAnchor => dialogueAnchor != null ? dialogueAnchor : transform;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;

        switch (actionType)
        {
            case InteractActionType.Dialogue:
                DialogueManager.Instance.TryPlayDialogue(
                    dialogueId,
                    dialogueType,
                    TargetAnchor,
                    false
                );
                break;

            case InteractActionType.Other:
                onPlayerEnter?.Invoke();
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        switch (actionType)
        {
            case InteractActionType.Dialogue:
                if (dialogueType == DialogueType.Small)
                {
                    DialogueManager.Instance.CloseSmallDialogue(TargetAnchor);
                }
                break;

            case InteractActionType.Other:
                onPlayerExit?.Invoke();
                break;
        }
    }
}