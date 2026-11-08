using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerActionType
{
    Jump,
    Dash,
    Attack,
    Interact,
    Hook,
    Shoot
}

public enum UIActionType
{
    Cancel, 
    ToggleNote, 
    DialogueNext
}

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance {get; private set;}

    [Header("Input Asset")]
    public InputActionAsset inputAsset; 
    private InputActionMap playerMap;
    private InputActionMap uiMap;
    private InputActionMap globalMap;

    [Header("Input Actions (Player)")]
    public InputActionReference moveAction;
    public InputActionReference walkAction;
    public InputActionReference jumpAction;
    public InputActionReference dashAction;
    public InputActionReference attackAction;
    public InputActionReference interactAction;
    public InputActionReference hookAction;
    public InputActionReference shootAction;

    [Header("Input Actions (UI)")]
    public InputActionReference cancelAction;
    public InputActionReference toggleNoteAction;
    public InputActionReference dialogueAction;

    // Continuous states
    public Vector2 MoveInput => moveAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
    public float MoveInputX => MoveInput.x;

    public bool IsMoveInputFromGamepad
    {
        get
        {
            if (moveAction?.action?.activeControl == null) return false;
            return moveAction.action.activeControl.device is Gamepad;
        }
    }

    public bool IsWalking => walkAction.action.IsPressed();
    public bool IsJumpHeld => jumpAction.action.IsPressed();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerMap = inputAsset.FindActionMap("Player");
        uiMap = inputAsset.FindActionMap("UI");
        globalMap = inputAsset.FindActionMap("Global");
        globalMap.Enable();

        EnablePlayerInput();
    }

    private void OnEnable()
    {
        // Player
        jumpAction.action.performed += OnJump;
        dashAction.action.performed += OnDash;
        attackAction.action.performed += OnAttack;
        interactAction.action.performed += OnInteract;
        hookAction.action.performed += OnHook;
        shootAction.action.performed += OnShoot;

        // UI
        cancelAction.action.performed += OnCancel;
        toggleNoteAction.action.performed += OnToggleNote;
        dialogueAction.action.performed += OnDialogue;
    }

    private void OnDisable()
    {
        // Player
        jumpAction.action.performed -= OnJump;
        dashAction.action.performed -= OnDash;
        attackAction.action.performed -= OnAttack;
        interactAction.action.performed -= OnInteract;
        hookAction.action.performed -= OnHook;
        shootAction.action.performed -= OnShoot;

        // UI
        cancelAction.action.performed -= OnCancel;
        toggleNoteAction.action.performed -= OnToggleNote;
        dialogueAction.action.performed -= OnDialogue;
    }

    // Player event
    private void OnJump(InputAction.CallbackContext ctx) => GameEvent.OnActionTriggered?.Invoke(PlayerActionType.Jump);
    private void OnDash(InputAction.CallbackContext ctx) => GameEvent.OnActionTriggered?.Invoke(PlayerActionType.Dash);
    private void OnAttack(InputAction.CallbackContext ctx) => GameEvent.OnActionTriggered?.Invoke(PlayerActionType.Attack);
    private void OnInteract(InputAction.CallbackContext ctx) => GameEvent.OnActionTriggered?.Invoke(PlayerActionType.Interact);
    private void OnHook(InputAction.CallbackContext ctx) => GameEvent.OnActionTriggered?.Invoke(PlayerActionType.Hook);
    private void OnShoot(InputAction.CallbackContext ctx) => GameEvent.OnActionTriggered?.Invoke(PlayerActionType.Shoot);

    // UI event
    private void OnCancel(InputAction.CallbackContext ctx) => GameEvent.OnUIActionTriggered?.Invoke(UIActionType.Cancel);
    private void OnToggleNote(InputAction.CallbackContext ctx) => GameEvent.OnUIActionTriggered?.Invoke(UIActionType.ToggleNote);
    private void OnDialogue(InputAction.CallbackContext ctx) => GameEvent.OnUIActionTriggered?.Invoke(UIActionType.DialogueNext);
    
    /// <summary>
    /// change to ui mod
    /// </summary>
    public void EnableUIInput()
    {
        playerMap.Disable();
        uiMap.Enable();
    }

    /// <summary>
    /// change to game mod
    /// </summary>
    public void EnablePlayerInput()
    {
        uiMap.Disable();
        playerMap.Enable();
    }
}
