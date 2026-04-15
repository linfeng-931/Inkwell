using UnityEngine;

public class CameraChangeArea : MonoBehaviour
{
    public bool flag;
    public float speed;
    public GameObject Map;
    public Quaternion oriQua;
    public float radius = 5f;

    private bool isRotate;

    void Start()
    {
        flag = false;
        isRotate = false;
    }

    void AutoGetRadiusFromMesh(GameObject cylinderObj)
    {
        Bounds bounds = cylinderObj.GetComponent<Renderer>().bounds;
        
        radius = bounds.extents.x; 
        
        Debug.Log("自動偵測半徑為: " + radius);
    }

    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !flag)
        {
            flag = true;
            oriQua = Map.transform.rotation;
            AutoGetRadiusFromMesh(gameObject);
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
