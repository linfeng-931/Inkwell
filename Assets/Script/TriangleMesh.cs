using UnityEngine;

public class TriangleMesh : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = new Mesh();
        mesh.name = "SingleTriangle";

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 1f, 0f),
            new Vector3(1f, -1f, 0f),
            new Vector3(-1f, -1f, 0f)
        };

        int[] triangles = new int[] { 0, 1, 2 };

        Vector2[] uv = new Vector2[]
        {
            new Vector2(0.5f, 1f),
            new Vector2(1f, 0f),
            new Vector2(0f, 0f)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
