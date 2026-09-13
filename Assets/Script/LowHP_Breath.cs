using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LowHPEffectController : MonoBehaviour
{
    
    [SerializeField] private GameObject lowHPEffect;
    [SerializeField] private Image maskImage;
    [SerializeField] private Material mask1;
    [SerializeField] private Material mask2;

    
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine loopCoroutine;

    private void OnEnable()
    {
        if (maskImage != null)
        {
            loopCoroutine = StartCoroutine(LowHPLoop());
        }
    }

    private void OnDisable()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
        }
    }

    private IEnumerator LowHPLoop()
    {
        bool useMask1 = true;
        maskImage.material = mask1;

        while (true)
        {
            yield return StartCoroutine(FadeAlpha(0f, 1f, fadeDuration));

            yield return StartCoroutine(FadeAlpha(1f, 0f, fadeDuration));

            useMask1 = !useMask1;
            maskImage.material = useMask1 ? mask1 : mask2;
        }
    }

    private IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color currentColor = maskImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);

            maskImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null; 
        }

        maskImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, endAlpha);
    }
}