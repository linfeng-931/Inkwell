using UnityEngine;

public class RotatePoint : MonoBehaviour
{
    public bool flag;
    
    void Start()
    {
        flag = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            flag = !flag;
        }
    }
}
