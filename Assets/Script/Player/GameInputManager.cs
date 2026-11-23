using System;
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

    private const string REBINDS_KEY = "PlayerInputRebinds";
    private InputActionRebindingExtensions.RebindingOperation currentRebindOperation;

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

        LoadBindingOverrides();

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

    #region Get, Save, Reset Key Binding
    public void SaveBindingOverrides()
    {
        string rebinds = inputAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(REBINDS_KEY, rebinds);
        PlayerPrefs.Save();
    }

    public void LoadBindingOverrides()
    {
        string rebinds = PlayerPrefs.GetString(REBINDS_KEY, string.Empty);
        if (!string.IsNullOrEmpty(rebinds))
        {
            inputAsset.LoadBindingOverridesFromJson(rebinds);
        }
    }

    public void ResetAllBindings()
    {
        inputAsset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(REBINDS_KEY);
        PlayerPrefs.Save();
    }
    #endregion

    #region Customize, Check Key Binding
    public void StartRebind(
        InputActionReference actionRef,
        int bindingIndex,
        Action onComplete,
        Action onCancel = null,
        Action onDuplicate = null
    )
    {
        InputAction action = actionRef.action;
        if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            return;

        // has current action => cancel
        currentRebindOperation?.Cancel();

        // record current key
        string previousOverridePath = action.bindings[bindingIndex].overridePath;

        // stop listening key action (global)
        bool wasEnabled = action.enabled;
        action.Disable();

        currentRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            // exclude some mouse action
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            // cancel action
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                operation.Dispose();
                currentRebindOperation = null;

                // if has repeat binding key
                if (CheckDuplicateBindings(action, bindingIndex))
                {
                    // reset
                    if (string.IsNullOrEmpty(previousOverridePath))
                        action.RemoveBindingOverride(bindingIndex);
                    else
                        action.ApplyBindingOverride(bindingIndex, previousOverridePath);

                    if (wasEnabled) action.Enable();
                    onDuplicate?.Invoke();
                    return;
                }

                // save
                SaveBindingOverrides();
                if (wasEnabled) action.Enable();
                onComplete?.Invoke();
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                currentRebindOperation = null;

                if (wasEnabled) action.Enable();
                onCancel?.Invoke();
            })
            .Start();
    }    

    private bool CheckDuplicateBindings(InputAction targetAction, int targetBindingIndex)
    {
        InputBinding newBinding = targetAction.bindings[targetBindingIndex];

        foreach (InputBinding binding in targetAction.actionMap.bindings)
        {
            if (binding.id == newBinding.id || binding.isComposite)
                continue;

            if (binding.effectivePath == newBinding.effectivePath) return true;
        }
        return false;
    }

    public string GetBindingName(InputActionReference actionRef, int bindingIndex)
    {
        if (actionRef?.action == null) return string.Empty;
        return actionRef.action.GetBindingDisplayString(bindingIndex);
    }

    #endregion
}
