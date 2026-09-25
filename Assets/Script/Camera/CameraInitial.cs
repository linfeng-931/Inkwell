using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraInitial : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;

    void OnEnable()
    {
        MapManager.Instance.OnMapSetupComplete += SetupCamera;
    }

    void OnDisable()
    {
        MapManager.Instance.OnMapSetupComplete -= SetupCamera;
    }
    private void SetupCamera()
    {
        cinemachineCamera.Follow = MapManager.Instance.player.transform;
        cinemachineCamera.PreviousStateIsValid = false;
    }
}
