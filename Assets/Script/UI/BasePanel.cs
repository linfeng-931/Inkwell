using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected float duration = 0.3f;
    protected CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OnEnter()
    {
        StartCoroutine(EnterCoroutine());
    }

    private IEnumerator EnterCoroutine()
    {
        float timer = 0f;
        canvasGroup.alpha = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void OnExit()
    {
        StartCoroutine(ExitCoroutine());
    }

    private IEnumerator ExitCoroutine()
    {
        float timer = 0f;
        canvasGroup.alpha = 1;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// when open sub window, pausing the main window 
    /// </summary>
    public virtual void OnPause()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// re active main window
    /// </summary>
    public virtual void OnResume()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}
