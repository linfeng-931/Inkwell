using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected float duration = 0.3f;
    protected CanvasGroup canvasGroup;

    public GameObject defaultSelectedObj;

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

        SetFocusToDefault();
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
        SetCanvasVisible(false);
    }

    /// <summary>
    /// re active main window
    /// </summary>
    public virtual void OnResume()
    {
        SetCanvasVisible(true);

        SetFocusToDefault();
    }

    protected void SetCanvasVisible(bool visible)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    protected virtual void SetFocusToDefault()
    {
        if (defaultSelectedObj != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultSelectedObj);
        }
    }
}
