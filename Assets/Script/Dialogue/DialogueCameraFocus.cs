using Unity.Cinemachine;
using UnityEngine;

public class DialogueCameraFocus : MonoBehaviour
{
    [SerializeField] private CinemachineCamera focusCamera;

    private bool isDialogueOpen;
    private bool isFocused;
    private int armedFrame = -1;

    private void OnEnable()
    {
        GameEvent.OnDialogueOpened += HandleDialogueOpened;
        GameEvent.OnDialogueClosed += HandleDialogueClosed;
    }

    private void OnDisable()
    {
        GameEvent.OnDialogueOpened -= HandleDialogueOpened;
        GameEvent.OnDialogueClosed -= HandleDialogueClosed;

        if (isFocused) Release(); // 物件被關閉 / 場景卸載時，不要把鏡頭留在開啟狀態
        isDialogueOpen = false;
    }

    /// <summary>給 UnityEvent 呼叫（例如 ItemTrigger.onIntroClosed）</summary>
    public void Activate()
    {
        if (isFocused || focusCamera == null) return;

        if (isDialogueOpen)
        {
            Focus();                       // 對話已經先打開了（排在 StartDialogue 後面）
        }
        else
        {
            armedFrame = Time.frameCount;  // 對話還沒打開：同一幀內若隨後打開，才算這一次的對話
        }
    }

    private void HandleDialogueOpened()
    {
        isDialogueOpen = true;
        if (!isFocused && armedFrame == Time.frameCount) Focus();
    }

    private void HandleDialogueClosed()
    {
        isDialogueOpen = false;
        if (isFocused) Release();
    }

    private void Focus()
    {
        isFocused = true;
        focusCamera.enabled = true;
    }

    private void Release()
    {
        isFocused = false;
        armedFrame = -1;
        if (focusCamera != null) focusCamera.enabled = false;
    }
}