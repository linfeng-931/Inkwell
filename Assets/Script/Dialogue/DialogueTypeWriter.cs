using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTypewriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private AudioClip typeClip;
    [SerializeField] private float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    public bool IsTyping { get; private set; }
    public float StartTypingTime { get; private set; }

    public void PlayText(string text)
    {
        StopTyping();

        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private IEnumerator TypeTextRoutine()
    {
        StartTypingTime = Time.time;
        IsTyping = true;

        dialogueText.ForceMeshUpdate();
        int totalCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        if (AudioManager.Instance != null && typeClip != null)
        {
            AudioManager.Instance.PlayLoop(typeClip, LoopChannel.Typing);
        }

        while (visibleCount <= totalCharacters)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        StopTypingSFX();
        IsTyping = false;
        typingCoroutine = null;
    }

    /// <summary>
    /// show all text
    /// </summary>
    public void CompleteText()
    {
        StopTyping();

        StartTypingTime = Time.time;
        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        IsTyping = false;
    }

    public void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        StopTypingSFX();
        IsTyping = false;
    }

    public void ClearText()
    {
        StopTyping();
        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = 0;
        }
    }

    private void StopTypingSFX()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoop(LoopChannel.Typing);
        }
    }

    private void OnDisable()
    {
        StopTypingSFX();
    }
}