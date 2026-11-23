using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueView : BasePanel, IDialogueView
{

    [SerializeField] private DialogueTypewriter typewriter;

    public bool IsTyping => typewriter.IsTyping;
    public float StartTypingTime => typewriter.StartTypingTime;

    private void Reset()
    {
        typewriter = GetComponent<DialogueTypewriter>();
    }

    public void UpdateDialogueText(string text)
    {
        typewriter.PlayText(text);
    }

    public void CompleteText()
    {
        typewriter.CompleteText();
    }
}
