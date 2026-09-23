using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputController : MonoBehaviour
{
    [Header("Player Input")]
    public InputActionReference toggleNoteAction;

    [Header("Presenter")]
    public NotePresenter notePresenter;

    private void OnEnable()
    {
        toggleNoteAction.action.performed += OnToggleNoteInput;
    }

    private void OnDisable()
    {
        toggleNoteAction.action.performed -= OnToggleNoteInput;
    }

    private void OnToggleNoteInput(InputAction.CallbackContext context)
    {
        GameEvent.OnToggleNote.Invoke();
    }
}
