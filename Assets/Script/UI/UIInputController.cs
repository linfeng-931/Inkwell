using Unity.Microsoft.GDK;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputController : MonoBehaviour
{
    [Header("General Player Input")]
    public InputActionReference cancelAction;

    [Header("Player Input")]
    public InputActionReference toggleNoteAction;
    public InputActionReference dialogueAction;
    public InputActionReference interactAction;

    [Header("Presenter")]
    public NotePresenter notePresenter;

    private bool isInCutscene = false;
    private bool canInteract = true;
    private bool isNoteOpen = false;

    private void OnEnable()
    {
        toggleNoteAction.action.performed += OnToggleNoteInput;
        dialogueAction.action.performed += OnDialogueInput;
        interactAction.action.performed += OnInteractionInput;
        cancelAction.action.performed += OnCancelInput;

        GameEvent.OnCutsceneStateChanged += HandleCutsceneStateChanged;
        GameEvent.OnInteractStateChanged += HandleInteractStateChanged;
        GameEvent.OnNoteStateChanged += HandleNoteStateChanged;
    }

    private void OnDisable()
    {
        toggleNoteAction.action.performed -= OnToggleNoteInput;
        dialogueAction.action.performed -= OnDialogueInput;
        interactAction.action.performed -= OnInteractionInput;
        cancelAction.action.performed -= OnCancelInput;

        GameEvent.OnCutsceneStateChanged -= HandleCutsceneStateChanged;
        GameEvent.OnInteractStateChanged -= HandleInteractStateChanged;
        GameEvent.OnNoteStateChanged -= HandleNoteStateChanged;
    }

    private void OnToggleNoteInput(InputAction.CallbackContext context)
    {
        if (isInCutscene) return;
        if(!canInteract && !isNoteOpen) return;

        GameEvent.OnToggleNote?.Invoke();
    }

    private void HandleNoteStateChanged(bool state)
    {
        isNoteOpen = state;
    }

    private void OnDialogueInput(InputAction.CallbackContext context)
    {
        if (isInCutscene)
        {
            GameEvent.OnDialogueNextPressed?.Invoke();
        }
    }

    private void OnInteractionInput(InputAction.CallbackContext context)
    {
        if(isInCutscene || !canInteract) return;
        GameEvent.OnWorldInteractPressed?.Invoke();
    }

    private void OnCancelInput(InputAction.CallbackContext context)
    {
        if (isInCutscene) return; 
        GameEvent.OnEscapePressed?.Invoke();
    }

    private void HandleCutsceneStateChanged(bool state)
    {
        isInCutscene = state;
    }

    private void HandleInteractStateChanged(bool state)
    {
        canInteract = !state;
    }
}
