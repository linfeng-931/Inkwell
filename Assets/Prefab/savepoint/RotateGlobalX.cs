using UnityEngine;

public class RotateGlobalX : MonoBehaviour
{
    public float speed = 90f; // ¨C¬í±ÛÂà¨¤«×

    void Update()
    {
        transform.Rotate(Vector3.back * speed * Time.deltaTime, Space.World);
    }
}