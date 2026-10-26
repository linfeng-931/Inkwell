using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DialogueEndTrigger : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool lockPlayerUntilEnded = true;

    public UnityEvent onDialogueEnded;

    private bool isDialogueOpen;
    private bool isWatching;
    private bool hasTriggered;
    private int armedFrame = -1;

    private void OnEnable()
    {
        GameEvent.OnDialogueOpened += HandleDialogueOpened;
        GameEvent.OnDialogueClosed += HandleDialogueClosed;
    }

    private void OnDisable()
    {
        GameEvent.OnDialogueOpened -= HandleDialogueOpened;
        GameEvent.OnDialogueClosed -= HandleDialogueClosed;
        isDialogueOpen = false;
        isWatching = false;
        PlayerLock.UnlockAll(this);
    }

    public void Watch()
    {
        if (isWatching || (triggerOnce && hasTriggered)) return;

        if (isDialogueOpen)
        {
            BeginWatching();
        }
        else
        {
            armedFrame = Time.frameCount;
        }
    }

    private void HandleDialogueOpened()
    {
        isDialogueOpen = true;
        if (!isWatching && armedFrame == Time.frameCount) BeginWatching();
    }

    private void HandleDialogueClosed()
    {
        isDialogueOpen = false;
        if (!isWatching) return;

        isWatching = false;
        hasTriggered = true;
        StartCoroutine(InvokeAfterDelay());
    }

    private IEnumerator InvokeAfterDelay()
    {
        if (delay <= 0f) yield return null;
        else yield return new WaitForSeconds(delay);

        onDialogueEnded?.Invoke();

        PlayerLock.Unlock(this);
    }

    private void BeginWatching()
    {
        isWatching = true;
        if (lockPlayerUntilEnded) PlayerLock.Lock(this);
    }
}