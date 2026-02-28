using UnityEngine;
using MouseInput;

public class DrawMesh : MonoBehaviour
{
    public bool isComplete;
    public float existTime = 4.0f;

    private Mesh mesh;
    private Vector3 lastMousePosition;
    private float smooth = 0.1f;
    private float sensitivity = 1000f;
    private float totalDistance = 0f;
    private float currentThickness = 0.5f;
    private bool isAct;
    private float timer;
    private GameObject particleSystem;
    [SerializeField] private float uvTiling = 2f; //貼圖重複度

    private void Start()
    {
        timer = 0f;
        isComplete = false;
        isAct = false;
        particleSystem = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if(!isAct){
            if (mesh == null)
            {
                mesh = new Mesh();
                totalDistance = 0f;

                Vector3[] vertices = new Vector3[4];
                Vector2[] uv = new Vector2[4];
                int[] triangles = new int[6];

                vertices[0] = MouseUtils.GetMouseWorldPosition(); //左上
                vertices[1] = MouseUtils.GetMouseWorldPosition(); //左下
                vertices[2] = MouseUtils.GetMouseWorldPosition(); //右上
                vertices[3] = MouseUtils.GetMouseWorldPosition(); //右下

                uv[0] = new Vector2(0, 1);
                uv[1] = new Vector2(0, 0);
                uv[2] = new Vector2(0, 1);
                uv[3] = new Vector2(0, 0);

                triangles[0] = 0;
                triangles[1] = 2;
                triangles[2] = 1;

                triangles[3] = 1;
                triangles[4] = 2;
                triangles[5] = 3;

                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.triangles = triangles;
                mesh.MarkDynamic();

                GetComponent<MeshFilter>().mesh = mesh;

                lastMousePosition = MouseUtils.GetMouseWorldPosition();
                particleSystem.SetActive(true);
            }

            if (Input.GetMouseButton(0) && mesh != null)
            {
                Vector3 currentMousePos = MouseUtils.GetMouseWorldPosition();
                float moveDistance = Vector3.Distance(currentMousePos, lastMousePosition);
                //可調整平滑度
                if(moveDistance < smooth) return;

                totalDistance += moveDistance;

                //筆畫粗細
                float speed = moveDistance/Time.deltaTime;
                float targetThickness = 0.5f - (speed/sensitivity);
                if(targetThickness > 0.5f) targetThickness = 0.5f;
                if(targetThickness < 0.1f) targetThickness = 0.1f;
                currentThickness = Mathf.Lerp(currentThickness, targetThickness, 100f*Time.deltaTime);
                

                Vector3[] vertices = new Vector3[mesh.vertices.Length + 2];
                Vector2[] uv = new Vector2[mesh.uv.Length + 2];
                int[] triangles = new int[mesh.triangles.Length + 6];

                mesh.vertices.CopyTo(vertices, 0);
                mesh.uv.CopyTo(uv, 0);
                mesh.triangles.CopyTo(triangles, 0);

                int vIndex = vertices.Length - 4;
                //previous(left)
                int vIndex0 = vIndex + 0; //up
                int vIndex1 = vIndex + 1; //down
                //new(right)
                int vIndex2 = vIndex + 2;
                int vIndex3 = vIndex + 3;

                Vector3 mouseForwardVector = (currentMousePos - lastMousePosition).normalized;

                Vector3 side = Vector3.Cross(mouseForwardVector, new Vector3(0,0,-1)).normalized * currentThickness;
                if (mesh.vertices.Length == 4)
                {
                    vertices[0] = lastMousePosition + side; //左上撐開
                    vertices[1] = lastMousePosition - side; //左下撐開
                    vertices[2] = lastMousePosition + side;
                    vertices[3] = lastMousePosition - side;

                    uv[0] = new Vector2(0, 1f);
                    uv[1] = new Vector2(0, 0f);
                }

                //設定這幀產生的新右側
                vertices[vIndex2] = currentMousePos + side;
                vertices[vIndex3] = currentMousePos - side;

                float currentU = totalDistance / uvTiling;
                uv[vIndex2] = new Vector2(currentU, 1f);
                uv[vIndex3] = new Vector2(currentU, 0f);

                int tIndex = triangles.Length - 6;

                triangles[tIndex + 0] = vIndex0;
                triangles[tIndex + 1] = vIndex2;
                triangles[tIndex + 2] = vIndex1;

                triangles[tIndex + 3] = vIndex1;
                triangles[tIndex + 4] = vIndex2;
                triangles[tIndex + 5] = vIndex3;

                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.triangles = triangles;
                mesh.RecalculateBounds();

                lastMousePosition = MouseUtils.GetMouseWorldPosition();
            }
            if (Input.GetMouseButtonUp(0))
            {
                isAct = true;
            }
        }
        else
        {
            timer += Time.deltaTime;
            if (!isComplete)
            {
                if(timer >= existTime)
                {
                    isComplete = true;
                    timer = 0f;
                }
            }
            else
            {
                if(timer >= 0.01f){
                    int vl = mesh.vertices.Length;
                    int uvl = mesh.uv.Length;
                    int tl = mesh.triangles.Length;
                    if(vl-1 <= 0 || tl-3 <= 0)
                    {
                        Destroy(mesh);
                        Destroy(gameObject);
                        return;
                    }

                    Vector3[] vertices = new Vector3[vl - 1];
                    Vector2[] uv = new Vector2[uvl - 1];
                    int[] triangles = new int[tl - 3];

                    System.Array.Copy(mesh.vertices, 1, vertices, 0, vl-1);
                    System.Array.Copy(mesh.uv, 1, uv, 0, uvl-1);
                    System.Array.Copy(mesh.triangles, 3, triangles, 0, tl-3);

                    for(int i = 0; i< triangles.Length; i++)
                    {
                        triangles[i]-=1;
                        if(triangles[i] < 0) triangles[i] = 0;
                    }

                    mesh.Clear();
                    mesh.vertices = vertices;
                    mesh.uv = uv;
                    mesh.triangles = triangles;
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();

                    timer = 0f;
                }
            }
        }
    }
}