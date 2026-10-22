using UnityEngine;
using System.Collections;
using TMPro;

public class TextEffect : MonoBehaviour
{
    public CanvasGroup textCanvasGroup;
    public TextMeshProUGUI tmpComponent;

    public float fadeDuration = 0.5f;

    public float blinkSpeed = 2.5f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1.0f;

    private Coroutine fadeCoroutine;
    private bool isBlinking = false;

    void Start()
    {
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 0f;
            textCanvasGroup.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isBlinking && tmpComponent != null)
        {
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) * 0.5f;
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            tmpComponent.color = new Color(1f, 1f, 1f, currentAlpha);
        }
    }

    public void StartBlinking()
    {
        if (textCanvasGroup == null) return;

        textCanvasGroup.gameObject.SetActive(true);
        isBlinking = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
    }

    public void StopBlinking()
    {
        if (textCanvasGroup == null) return;

        isBlinking = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(textCanvasGroup.alpha, 0f, fadeDuration, () =>
        {
            textCanvasGroup.gameObject.SetActive(false);
        }));
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration, System.Action onComplete = null)
    {
        float timer = 0f;
        textCanvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        textCanvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}