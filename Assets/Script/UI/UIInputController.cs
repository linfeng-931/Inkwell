using UnityEngine;

public class UIInputController : MonoBehaviour
{
    [Header("Presenter")]
    public NotePresenter notePresenter;

    private bool isInCutscene = false;
    private bool canInteract = true;
    private bool isNoteOpen = false;
    private bool isDialogue = false;

    private void OnEnable()
    {
        // input event
        GameEvent.OnUIActionTriggered += HandleUIInput;
        GameEvent.OnActionTriggered += HandlePlayerInput; // for player interaction

        // state change event
        GameEvent.OnCutsceneStateChanged += HandleCutsceneStateChanged;
        GameEvent.OnInteractStateChanged += HandleInteractStateChanged;
        GameEvent.OnDialogueOpened += HandleDialogueOpened;
        GameEvent.OnDialogueClosed += HandleDialogueClosed;
        GameEvent.OnNoteStateChanged += HandleNoteStateChanged;
    }

    private void OnDisable()
    {
        GameEvent.OnUIActionTriggered -= HandleUIInput;
        GameEvent.OnActionTriggered -= HandlePlayerInput;

        GameEvent.OnCutsceneStateChanged -= HandleCutsceneStateChanged;
        GameEvent.OnInteractStateChanged -= HandleInteractStateChanged;
        GameEvent.OnDialogueOpened -= HandleDialogueOpened;
        GameEvent.OnDialogueClosed -= HandleDialogueClosed;
        GameEvent.OnNoteStateChanged -= HandleNoteStateChanged;
    }

    private void HandleUIInput(UIActionType type)
    {
        switch (type)
        {
            case UIActionType.ToggleNote:
                if (isInCutscene) return;
                if(!canInteract && !isNoteOpen) return;
                GameEvent.OnToggleNote?.Invoke();
                break;

            case UIActionType.DialogueNext:
                if (isInCutscene)
                {
                    GameEvent.OnDialogueNextPressed?.Invoke();
                }
                break;

            case UIActionType.Cancel:
                if (isInCutscene) return; 
                GameEvent.OnEscapePressed?.Invoke();
                GameEvent.currentCloseCallback?.Invoke();
                GameEvent.currentCloseCallback = null;

                if(isNoteOpen){
                    GameEvent.OnToggleNote?.Invoke();
                }
                break;
        }
    }

    private void HandlePlayerInput(PlayerActionType type)
    {
        if(type == PlayerActionType.Interact)
        {
            if(isInCutscene || !canInteract) return;
            GameEvent.OnWorldInteractPressed?.Invoke();
        }
    }

    private void HandleNoteStateChanged(bool state)
    {
        isNoteOpen = state;
        UpdateInputMode();
    }

    private void HandleDialogueOpened()
    {
        isDialogue = true;
        UpdateInputMode();
    }

    private void HandleDialogueClosed()
    {
        isDialogue = false;
        UpdateInputMode();
    }

    private void HandleCutsceneStateChanged(bool state)
    {
        isInCutscene = state;
        UpdateInputMode();
    }

    private void HandleInteractStateChanged(bool state)
    {
        canInteract = !state;
    }

    /// <summary>
    /// change input mod
    /// </summary>
    private void UpdateInputMode()
    {
        if(GameInputManager.Instance == null) return;

        if(isNoteOpen || isInCutscene || !canInteract || isDialogue) // all condition you want to change input mod
        {
            GameInputManager.Instance.EnableUIInput();

            var bufferManager = FindFirstObjectByType<InputBufferManager>();
            if (bufferManager != null)
            {
                bufferManager.ClearAllBuffers();
            }
        }
        else
        {
            GameInputManager.Instance.EnablePlayerInput();
        }
    }
}
