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

        if (isFocused) Release();
        isDialogueOpen = false;
    }

    public void Activate()
    {
        if (isFocused || focusCamera == null) return;

        if (isDialogueOpen)
        {
            Focus();
        }
        else
        {
            armedFrame = Time.frameCount;
        }
    }

    private void HandleDialogueOpened(DialogueType type, Transform target)
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