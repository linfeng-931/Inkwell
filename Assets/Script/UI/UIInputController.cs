using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputController : MonoBehaviour
{
    [Header("Player Input")]
    public InputActionReference toggleNoteAction;

    [Header("Presenter")]
    public NotePresenter notePresenter;

    private bool isInCutscene = false;

    private void OnEnable()
    {
        toggleNoteAction.action.performed += OnToggleNoteInput;
        GameEvent.OnCutsceneStateChanged += state => isInCutscene = state;
    }

    private void OnDisable()
    {
        toggleNoteAction.action.performed -= OnToggleNoteInput;
    }

    private void OnToggleNoteInput(InputAction.CallbackContext context)
    {
        if (isInCutscene) return;
        GameEvent.OnToggleNote.Invoke();
    }
}
