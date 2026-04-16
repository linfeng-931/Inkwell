using UnityEngine;

public class OutTunnel : MonoBehaviour
{
    public bool flag;
    public CameraChangeArea cameraChangeArea;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && cameraChangeArea.flag)
        {
            cameraChangeArea.ExitArea();
        }
    }
}
