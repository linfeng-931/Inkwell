using UnityEngine;

public class CameraChangeArea : MonoBehaviour
{
    public bool flag;
    public float speed;

    void Start()
    {
        flag = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !flag)
        {
            flag = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && flag)
        {
            flag = false;
        }
    }
}
