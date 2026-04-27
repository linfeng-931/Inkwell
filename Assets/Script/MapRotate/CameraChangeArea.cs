using UnityEngine;

public class CameraChangeArea : MonoBehaviour
{
    public bool flag;
    public bool isOutTunnel;
    public float speed;
    public GameObject Map;
    public Quaternion oriQua;
    public float radius = 5f;


    void Start()
    {
        flag = false;
    }

    void AutoGetRadiusFromMesh(GameObject cylinderObj)
    {
        Renderer cylinderRenderer = cylinderObj.GetComponent<Renderer>();
        Bounds bounds;
        if(cylinderRenderer != null){
            bounds = cylinderRenderer.bounds;
            radius = bounds.extents.x; 
        }
        
        Debug.Log("自動偵測半徑為: " + radius);
    }

    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !flag)
        {
            flag = true;
            if(Map != null) oriQua = Map.transform.rotation;
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

    public void ExitArea()
    {
        flag = false;
        isOutTunnel = true;
    }
}
