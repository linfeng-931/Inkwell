using Unity.Cinemachine;
using UnityEngine;

public class IntroCameraFocus : MonoBehaviour
{
    [SerializeField] private CinemachineCamera focusCamera;
    [SerializeField] private int focusPriority = 30;
    [SerializeField] private int idlePriority = 0;

    private bool isFinished;

    private void Awake()
    {
        if (focusCamera != null) focusCamera.Priority = idlePriority;
    }

    public void Activate()
    {
        if (isFinished || focusCamera == null) return;

        if (focusCamera.Follow == null)
        {
            focusCamera.Follow = MapManager.Instance.player.transform;
        }

        focusCamera.gameObject.SetActive(true);
        focusCamera.Priority = focusPriority;
    }

    public void Deactivate()
    {
        isFinished = true;
        if (focusCamera == null) return;

        focusCamera.Priority = idlePriority;
        focusCamera.gameObject.SetActive(false);
    }
}