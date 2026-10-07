using UnityEngine;

public class DialoguePresenter : MonoBehaviour
{
    [SerializeField] private DialogueView view;

    void Start()
    {
        view.OnExit();
    }

    private void OnEnable()
    {
        GameEvent.OnDialogueOpened += HandleDialogueOpened;
        GameEvent.OnDialogueLineUpdated += HandleLineUpdated;
        GameEvent.OnDialogueClosed += HandleDialogueClosed;
    }

    private void OnDisable()
    {
        GameEvent.OnDialogueOpened -= HandleDialogueOpened;
        GameEvent.OnDialogueLineUpdated -= HandleLineUpdated;
        GameEvent.OnDialogueClosed -= HandleDialogueClosed;
    }

    private void HandleDialogueOpened()
    {
        UIManager.Instance.OpenPanel(view);
    }
    
    private void HandleLineUpdated(string text)
    {
        view.UpdateDialogueText(text);
    }

    private void HandleDialogueClosed()
    {
        UIManager.Instance.CloseTopPanel();
    }
}
