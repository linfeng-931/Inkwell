using UnityEngine;

public class SeeThroughFollow : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Material[] SeeThroughMaterials;
    public Camera Camera;
    public LayerMask Mask;

    public float SphereRadius = 0.8f;
    public float HeightOffset = 1.0f;
    public float SeeThroughSize = 1.0f;

    void Update()
    {
        Vector3 origin = transform.position + Vector3.up * HeightOffset;

        Vector3 dir = Camera.transform.position - origin;

        bool blocked = Physics.SphereCast(
            origin,
            SphereRadius,
            dir.normalized,
            out RaycastHit hit,
            dir.magnitude,
            Mask
        );

        float size = blocked ? SeeThroughSize : 0;

        Vector3 view = Camera.WorldToViewportPoint(origin);

        foreach (Material mat in SeeThroughMaterials)
        {
            if (mat == null) continue;

            mat.SetFloat(SizeID, size);
            mat.SetVector(PosID, view);
        }
    }
}