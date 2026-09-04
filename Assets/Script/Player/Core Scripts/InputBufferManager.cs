using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBufferManager : MonoBehaviour
{
    /// <summary>
    /// triggered type the actions
    /// </summary>
    public enum InputActionType
    {
        Jump,
        Dash,
        Attack,
        Interact,
        Hook
    }

    public float defaultBufferTimer = 0.2f;
    private Dictionary<InputActionType, float> inputTimers = new Dictionary<InputActionType, float>();

    //continuous type actions
    public float moveInputX { get; private set; }
    public bool isRunning { get; private set; }
    public bool isWalking { get; private set; }
    public bool isJumpReleased { get; private set; }

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference walkAction; //default way to move is running

    public InputActionReference jumpAction;
    public InputActionReference dashAction;
    public InputActionReference attackAction;
    public InputActionReference interactAction;
    public InputActionReference hookAction;

    private void Update()
    {
        //continuous type
        moveInputX = moveAction.action.ReadValue<float>();
        isWalking = walkAction.action.IsPressed();
        isJumpReleased = jumpAction.action.WasReleasedThisFrame();

        //triggered type
        if (jumpAction.action.WasPressedThisFrame())
            BufferInput(InputActionType.Jump);
        if (dashAction.action.WasPressedThisFrame())
            BufferInput(InputActionType.Dash);
        if (attackAction.action.WasPressedThisFrame())
            BufferInput(InputActionType.Attack);
        if (interactAction.action.WasPressedThisFrame())
            BufferInput(InputActionType.Interact);
        if (hookAction.action.WasPressedThisFrame())
            BufferInput(InputActionType.Hook);
    }

    private void BufferInput(InputActionType type)
    {
        //timestamp: now + 0.2f
        inputTimers[type] = Time.time + defaultBufferTimer; 
    }

    /// <summary>
    /// let other script to get input buffers
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasBufferedInput(InputActionType type)
    {
        if (inputTimers.ContainsKey(type))
        {
            return Time.time <= inputTimers[type];
        }
        return false;
    }

    /// <summary>
    /// discard expired input buffers
    /// </summary>
    /// <param name="type"></param>
    public void ConsumeInput(InputActionType type)
    {
        if (inputTimers.ContainsKey(type))
        {
            inputTimers[type] = 0f;
        }
    }
}
