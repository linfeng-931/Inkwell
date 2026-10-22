using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ShowCanvas : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;
    private Coroutine fadeCoroutine;

    public string dialogueIdAfterHide;
    public bool playDialogueOnce = true;
    public UnityEvent onHidden;

    private bool hasPlayedDialogue;

    public bool MovementEnabled => !PlayerLock.IsLockedBy(this);
    public void LockMovement() => PlayerLock.Lock(this);
    public void UnlockMovement() => PlayerLock.Unlock(this);
    public void LockCombat() => PlayerLock.Lock(this, PlayerLockType.Combat);
    public void UnLockCombat() => PlayerLock.Unlock(this, PlayerLockType.Combat);

    private bool isActive = false;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.Space))
        {
            Hide();
        }
    }

    private void OnDisable()
    {
        PlayerLock.UnlockAll(this);
    }

    public void Show()
    {
        if (canvasGroup == null) return;
        LockMovement();
        LockCombat();
        canvasGroup.gameObject.SetActive(true);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
        isActive = true;
    }

    public void Hide()
    {
        if (canvasGroup == null) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 0f, fadeDuration, () =>
        {
            canvasGroup.gameObject.SetActive(false);
            PlayAfterHide();
            OnDisable();
        }));
        isActive = false;
    }

    private void PlayAfterHide()
    {
        bool canPlayDialogue = !string.IsNullOrEmpty(dialogueIdAfterHide)
                               && !(playDialogueOnce && hasPlayedDialogue);

        if (canPlayDialogue)
        {
            hasPlayedDialogue = true;
            DialogueManager.Instance.TryPlayDialogue(dialogueIdAfterHide, false);
        }

        onHidden?.Invoke();
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration, System.Action onComplete = null)
    {
        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}