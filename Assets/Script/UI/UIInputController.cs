using Unity.Microsoft.GDK;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputController : MonoBehaviour
{
    [Header("Player Input")]
    public InputActionReference toggleNoteAction;
    public InputActionReference dialogueAction;

    [Header("Presenter")]
    public NotePresenter notePresenter;

    private bool isInCutscene = false;

    private void OnEnable()
    {
        toggleNoteAction.action.performed += OnToggleNoteInput;
        GameEvent.OnCutsceneStateChanged += HandleCutsceneStateChanged;
        dialogueAction.action.performed += OnDialogueInput;
    }

    private void OnDisable()
    {
        toggleNoteAction.action.performed -= OnToggleNoteInput;
        dialogueAction.action.performed -= OnDialogueInput;
        GameEvent.OnCutsceneStateChanged -= HandleCutsceneStateChanged;
    }

    private void OnToggleNoteInput(InputAction.CallbackContext context)
    {
        if (isInCutscene) return;
        GameEvent.OnToggleNote.Invoke();
    }

    private void OnDialogueInput(InputAction.CallbackContext context)
    {
        if (isInCutscene)
        {
            GameEvent.OnDialogueNextPressed.Invoke();
        }
    }

    private void HandleCutsceneStateChanged(bool state)
    {
        isInCutscene = state;
    }
}
