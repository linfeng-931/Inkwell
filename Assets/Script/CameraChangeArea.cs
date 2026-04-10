using UnityEngine;

public class CameraChangeArea : MonoBehaviour
{
    public bool flag;
    public float speed;
    public GameObject Map;
    public Quaternion oriQua;

    void Start()
    {
        flag = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !flag)
        {
            flag = true;
            oriQua = Map.transform.rotation;
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
