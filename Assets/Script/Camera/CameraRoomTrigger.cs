using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraRoomTrigger : MonoBehaviour
{
    public CinemachineCamera targetCamera;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            CameraManager.Instance.AddCamera(targetCamera);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            CameraManager.Instance.RemoveCamera(targetCamera);
        }
    }
}
