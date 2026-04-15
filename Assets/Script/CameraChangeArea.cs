using UnityEngine;

public class CameraChangeArea : MonoBehaviour
{
    public bool flag;
    public float speed;
    public GameObject Map;
    public Quaternion oriQua;
    public RotatePoint[] rotatePoint;

    private bool isRotate;

    void Start()
    {
        flag = false;
        isRotate = false;
    }
    void Update()
    {
        RotateStatus();
        
        if (isRotate && !flag)
        {
            flag = true;
            oriQua = Map.transform.rotation;
        }
        else if(!isRotate && flag)
        {
            flag = false;
        }
    }

    void RotateStatus()
    {
        int i = 0;
        foreach (RotatePoint rp in rotatePoint)
        {
            if(rp.flag) i++;
        }

        if(i%2 == 0) isRotate = false;
        else isRotate = true;
    }

    /*void OnTriggerEnter(Collider other)
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
    }*/
}
