using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraInitial : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private bool activateOnCameraStart = false;

    void OnEnable()
    {
        MapManager.Instance.OnMapSetupComplete += SetupCamera;
        MapManager.Instance.OnCameraTransitionStart += ActivateEntryCamera;
    }

    void OnDisable()
    {
        MapManager.Instance.OnMapSetupComplete -= SetupCamera;
        MapManager.Instance.OnCameraTransitionStart -= ActivateEntryCamera;
    }
    private void SetupCamera()
    {
        cinemachineCamera.Follow = MapManager.Instance.player.transform;
        cinemachineCamera.PreviousStateIsValid = false;
    }
    private void ActivateEntryCamera()
    {
        if (!activateOnCameraStart || cinemachineCamera == null) return;

        cinemachineCamera.PreviousStateIsValid = false;
        CameraManager.Instance.AddCamera(cinemachineCamera);
    }
}
