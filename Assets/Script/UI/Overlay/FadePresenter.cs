using UnityEngine;

public class FadePresenter : MonoBehaviour
{
    [SerializeField] private FadeView view;

    private void OnEnable()
    {
        GameEvent.OnToggleFade += ToggleFade;
    }

    private void OnDisable()
    {
        GameEvent.OnToggleFade -= ToggleFade;
    }

    public void ToggleFade(bool isStart)
    {
        if (isStart)
        {
            view.PlayFadeIn();
        }
        else
        {
            view.PlayFadeOut();
        }
    }
}
