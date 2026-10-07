using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string targetDialogueId;

    public void StartDialogue()
    {
        DialogueManager.Instance.TryPlayDialogue(targetDialogueId);
    }
}
