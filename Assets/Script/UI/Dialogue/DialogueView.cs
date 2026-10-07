using UnityEngine;
using TMPro;

public class DialogueView : BasePanel
{
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void UpdateDialogueText(string text)
    {
        dialogueText.text = text;
    }
}
