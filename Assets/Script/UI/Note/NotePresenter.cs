using UnityEngine;

public class NotePresenter : MonoBehaviour
{
    [SerializeField] private NoteView view;
    private CanvasGroup viewCanvasGroup;

    private void Awake()
    {
        viewCanvasGroup = view.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        view.OnExit();
    }

    private void OnEnable()
    {
        GameEvent.OnToggleNote += ToggleNote;
    }

    private void OnDisable()
    {
        GameEvent.OnToggleNote -= ToggleNote;
    }

    public void ToggleNote()
    {
        if (viewCanvasGroup.alpha > 0)
        {
            UIManager.Instance.CloseTopPanel();
            GameEvent.OnInteractStateChanged.Invoke(false);
        }
        else
        {
            UIManager.Instance.OpenPanel(view);
            GameEvent.OnInteractStateChanged.Invoke(true);
        }
    }
}
