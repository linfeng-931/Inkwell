using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueView : BasePanel
{

    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip typeClip;
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    public bool IsTyping { get; private set; }

    public void UpdateDialogueText(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            if (audioManager != null) audioManager.StopSFX();
        }
        
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private IEnumerator TypeTextRoutine()
    {
        IsTyping = true;

        dialogueText.ForceMeshUpdate();
        int totalCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        if (audioManager != null)
        {
            audioManager.PlaySFX(typeClip, 0);
        }

        while (visibleCount <= totalCharacters)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        audioManager.StopSFX();
        IsTyping = false;
    }

    /// <summary>
    /// show all text
    /// </summary>
    public void CompleteText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            audioManager.StopSFX();
        }
        
        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        IsTyping = false;
    }
}
