using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string targetDialogueId;

    [SerializeField] private DialogueEndTrigger endWatcher;

    private void Awake()
    {
        if (endWatcher == null) endWatcher = GetComponent<DialogueEndTrigger>();
    }

    public void StartDialogue()
    {
        if (endWatcher != null) endWatcher.Watch();
        DialogueManager.Instance.TryPlayDialogue(targetDialogueId);
    }
}
