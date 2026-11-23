using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(DialogueTypewriter))]
public class WorldDialogueView : MonoBehaviour, IDialogueView
{
    [Header("Components")]
    [SerializeField] private DialogueTypewriter typewriter;
    [SerializeField] private GameObject uiRoot; // display
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private RectTransform rectTransform;

    [Header("Animator Settings")]
    [SerializeField] private string showTrigger = "show";
    [SerializeField] private string hideTrigger = "hide";
    [SerializeField] private float hideAnimDuration = 0.25f;

    [Header("Follow & Scale Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2f, 0);
    [SerializeField] private float referenceDistance = 10f; // distance of normal size
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.3f;

    [Header("Small Dialogue Settings")]
    [SerializeField] private float smallDialogueWaitAfterTyping = 2.0f; // for auto typing (delay time)

    private Transform followTarget;
    private Camera mainCam;
    private bool isOpen = false;
    private bool isClosing = false;
    private Coroutine smallDialogueCoroutine;
    private Coroutine closeCoroutine;

    public bool IsTyping => typewriter.IsTyping;
    public float StartTypingTime => typewriter.StartTypingTime;
    public Transform CurrentTarget => followTarget;

    private void Awake()
    {
        mainCam = Camera.main;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (typewriter == null) typewriter = GetComponent<DialogueTypewriter>();
        Close();
    }

    private void LateUpdate()
    {
        if (!isOpen || followTarget == null) return;
        if (mainCam == null) mainCam = Camera.main;

        Vector3 targetWorldPos = followTarget.position + worldOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(targetWorldPos);

        // if target not in front of camera
        bool isInFront = screenPos.z > 0f;
        if (uiRoot.activeSelf != isInFront)
        {
            uiRoot.SetActive(isInFront);
        }

        if (!isInFront) return;

        // update screen pos
        rectTransform.position = screenPos;

        // update size
        float distance = Vector3.Distance(mainCam.transform.position, targetWorldPos);
        float scale = Mathf.Clamp(referenceDistance / Mathf.Max(distance, 0.01f), minScale, maxScale);
        rectTransform.localScale = Vector3.one * scale;
    }

    // mid dialogue
    public void Open(Transform target)
    {
        StopSmallDialogue();
        StopCloseRoutine();

        followTarget = target;
        isOpen = true;
        isClosing = false;
        uiRoot.SetActive(true);

        if (backgroundAnimator != null)
        {
            backgroundAnimator.ResetTrigger(hideTrigger);
            backgroundAnimator.SetTrigger(showTrigger);
        }
    }

    public void UpdateDialogueText(string text)
    {
        typewriter.PlayText(text);
    }

    public void CompleteText()
    {
        typewriter.CompleteText();
    }

    public void Close()
    {
        StopSmallDialogue();
        typewriter.ClearText();
        
        if (!isOpen || isClosing) return;

        isClosing = true;
        if (backgroundAnimator != null)
        {
            backgroundAnimator.ResetTrigger(showTrigger);
            backgroundAnimator.SetTrigger(hideTrigger);
        }

        StopCloseRoutine();
        closeCoroutine = StartCoroutine(CloseDelayRoutine());
    }

    public void CloseImmediate()
    {
        StopSmallDialogue();
        StopCloseRoutine();
        typewriter.ClearText();

        isOpen = false;
        isClosing = false;
        followTarget = null;
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    // small dialogue
    public void PlaySmallDialogue(Transform target, List<string> texts, Action onComplete)
    {
        Open(target);
        smallDialogueCoroutine = StartCoroutine(SmallDialogueRoutine(texts, onComplete));
    }

    private IEnumerator SmallDialogueRoutine(List<string> texts, Action onComplete)
    {
        foreach (string line in texts)
        {
            typewriter.PlayText(line);
            yield return new WaitUntil(() => !typewriter.IsTyping);
            yield return new WaitForSeconds(smallDialogueWaitAfterTyping);
        }

        Close();
        onComplete?.Invoke();
    }

    private IEnumerator CloseDelayRoutine()
    {
        yield return new WaitForSecondsRealtime(hideAnimDuration);
        CloseImmediate();
    }

    private void StopSmallDialogue()
    {
        if (smallDialogueCoroutine != null)
        {
            StopCoroutine(smallDialogueCoroutine);
            smallDialogueCoroutine = null;
        }
    }

    private void StopCloseRoutine()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }
    }
}