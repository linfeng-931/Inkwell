using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public CinemachineCamera[] allCameras;

    // camera stack (handle overlap area)
    public List<CinemachineCamera> activeCameras = new List<CinemachineCamera>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// add target camera to list
    /// </summary>
    /// <param name="cam"></param>
    public void AddCamera(CinemachineCamera cam)
    {
        if (!activeCameras.Contains(cam))
        {
            activeCameras.Add(cam);
        }
        UpdateCameraPriorities();
    }

    /// <summary>
    /// remove camera from list
    /// </summary>
    /// <param name="cam"></param>
    public void RemoveCamera(CinemachineCamera cam)
    {
        if (activeCameras.Contains(cam))
        {
            activeCameras.Remove(cam);
        }
        UpdateCameraPriorities();
    }

    /// <summary>
    /// switch camera by priority
    /// </summary>
    private void UpdateCameraPriorities()
    {
        if(activeCameras.Count == 0) return;

        CinemachineCamera targetCam = activeCameras[activeCameras.Count - 1];
        foreach (CinemachineCamera cam in allCameras)
        {
            cam.Priority = (cam == targetCam) ? 20 : 10;
        }
    }
}
