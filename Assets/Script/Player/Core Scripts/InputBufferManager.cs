using System.Collections.Generic;
using UnityEngine;

public class InputBufferManager : MonoBehaviour
{
    public float defaultBufferTimer = 0.2f;
    private Dictionary<PlayerActionType, float> inputTimers = new Dictionary<PlayerActionType, float>();
    private readonly Dictionary<PlayerActionType, float> blockedUntil = new Dictionary<PlayerActionType, float>();

    //continuous type actions
    public float moveInputX => GameInputManager.Instance != null ? GameInputManager.Instance.MoveInputX : 0f;
    public bool isWalking => GameInputManager.Instance != null && GameInputManager.Instance.IsWalking;
    public bool isJumpHeld => GameInputManager.Instance != null && GameInputManager.Instance.IsJumpHeld;
    
    private void Start()
    {
        if (GameInputManager.Instance != null)
        {
            GameEvent.OnActionTriggered += BufferInput;
        }
    }

    private void OnDestroy()
    {
        if (GameInputManager.Instance != null)
        {
            GameEvent.OnActionTriggered -= BufferInput;
        }
    }

    public void BlockInput(PlayerActionType type, float duration)
    {
        blockedUntil[type] = Time.time + duration;
        ConsumeInput(type);
    }
    
    private void BufferInput(PlayerActionType type)
    {
        if (blockedUntil.TryGetValue(type, out float until) && Time.time < until) return;
        //timestamp: now + 0.2f
        inputTimers[type] = Time.time + defaultBufferTimer; 
    }

    /// <summary>
    /// let other script to get input buffers
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasBufferedInput(PlayerActionType type)
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
    public void ConsumeInput(PlayerActionType type)
    {
        if (inputTimers.ContainsKey(type))
        {
            inputTimers[type] = 0f;
        }
    }

    public void ClearAllBuffers()
    {
        List<PlayerActionType> keys = new List<PlayerActionType>(inputTimers.Keys);
        foreach (PlayerActionType key in keys)
        {
            inputTimers[key] = 0f;
        }
    }
}
