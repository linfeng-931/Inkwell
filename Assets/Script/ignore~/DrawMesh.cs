using UnityEngine;
using MouseInput;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;
using System.Linq;

public class DrawMesh : MonoBehaviour
{
    public bool isComplete;
    public bool isAct;
    public float existTime = 4.0f;
    public float depth = 0.5f;
    public CameraController cameraController;
    public int drawKey = 1;

    [Header("Shader Settings")]
    public Material[] shaders;
    private MeshRenderer meshRenderer;

    [Header("Collider Settings")]
    public float colliderDepth = 10f;
    private List<BoxCollider> colliders = new List<BoxCollider>();
    private GameObject colliderContainer;
    private List<Vector3> pathPoints = new List<Vector3>();

    private Mesh mesh;
    private Vector3 lastMousePosition;
    private float smooth = 0.1f;
    private float sensitivity = 500f;
    private float totalDistance = 0f;
    private float currentThickness = 0.01f;
    private float timer;
    private GameObject particleSystem;
    [SerializeField] private float uvTiling = 2f; //貼圖重複度
    private bool isExtrude;
    private bool eligibleExtrude;
    private bool canExtrude;
    private float zFace = -1f; 
    private bool hasCollider;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = shaders[0];
        timer = 0f;
        isComplete = false;
        isAct = false;
        particleSystem = transform.GetChild(0).gameObject;
        isExtrude = false;
        eligibleExtrude = false;
        hasCollider = false;
        cameraController = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

        //Collider
        colliderContainer = new GameObject("ColliderContainer");
        colliderContainer.transform.SetParent(this.transform);
        colliderContainer.transform.localPosition = Vector3.zero;
        colliderContainer.transform.localRotation = Quaternion.identity;
        colliderContainer.layer = 3;
    }

    private void Update()
    {
        if(!isAct){
            if (cameraController.eligibleExtrude)
            {
                eligibleExtrude = true;
                cameraController.eligibleExtrude = false;
            }

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

            //for collider
             if (Input.GetMouseButtonDown(drawKey))
            {
                pathPoints.Clear();
                pathPoints.Add(MouseUtils.GetMouseWorldPosition());
            }
            //

            if (Input.GetMouseButton(drawKey) && mesh != null)
            {
                Vector3 currentMousePos = MouseUtils.GetMouseWorldPosition();
                float moveDistance = Vector3.Distance(currentMousePos, lastMousePosition);
                Vector3 moveVector = currentMousePos - lastMousePosition;
                //可調整平滑度
                if(moveDistance < smooth) return;

                //for collider
                if(pathPoints.Count > 0)
                {
                    if(Vector3.Distance(currentMousePos, pathPoints[pathPoints.Count -1]) > smooth)
                    {
                        pathPoints.Add(currentMousePos);
                    }
                }
                else
                {
                    pathPoints.Add(currentMousePos);
                }
                //

                totalDistance += moveDistance;

                float deltaX = Mathf.Abs(moveVector.x);
                float deltaY = Mathf.Abs(moveVector.y);

                float speed = moveDistance / Time.deltaTime;

                float baseThickness = 0.05f + (speed / sensitivity);

                //0 代表完全水平，1 代表完全垂直
                float t = deltaY / (deltaX + deltaY + 0.0001f);

                float horizontalWeight = 0.05f;
                float verticalWeight = 3.0f;

                float directionalMultiplier = Mathf.Lerp(horizontalWeight, verticalWeight, t);
                float targetThickness = baseThickness * directionalMultiplier;

                targetThickness = Mathf.Clamp(targetThickness, 0.05f, 0.35f);
                currentThickness = Mathf.Lerp(currentThickness, targetThickness, 10f*Time.deltaTime);
                

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

            if (Input.GetMouseButtonUp(drawKey))
            {
                isAct = true;
                meshRenderer.material = shaders[1];
                GenerateSegmentColliders();
            }
        }
        else
        {
            if (cameraController.canExtrude && eligibleExtrude)
            {
                canExtrude = true;
                eligibleExtrude = false;
                cameraController.canExtrude = false;
            }
            else
            {
                eligibleExtrude = false;
            }
            if(canExtrude && !isExtrude){
                Extrude();
            }

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

    public void Extrude()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if(mf == null || mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0) return;
        Mesh sourceMesh = mf.sharedMesh; //ensure that mf.sharedMesh exist
        
        ProBuilderMesh pbMesh = GetComponent<ProBuilderMesh>();
        if(pbMesh == null) pbMesh = gameObject.AddComponent<ProBuilderMesh>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        Material[] materials = (mr != null) ? mr.sharedMaterials : new Material[0];
        var importer = new MeshImporter(sourceMesh, materials, pbMesh);
        importer.Import();  

        List<Face> allFaces = pbMesh.faces.ToList();
        pbMesh.Extrude(allFaces, ExtrudeMethod.FaceNormal, depth);

        pbMesh.ToMesh();
        pbMesh.Refresh();
        
        isExtrude = true;
        transform.position += new Vector3(0, 0, 0.1f);

        // 子物件負責物理
        GameObject physProxy = new GameObject(gameObject.name + "_PhysicsProxy");
        physProxy.transform.SetParent(this.transform);
        physProxy.transform.localPosition = Vector3.zero;
        physProxy.transform.localRotation = Quaternion.identity;
        physProxy.transform.localScale = Vector3.one;

        physProxy.layer = 3; 

        MeshCollider col = physProxy.AddComponent<MeshCollider>();
        col.sharedMesh = mf.sharedMesh;
    }

    public void DrawMeshDestory()
    {
        //Destroy(this.transform.root.gameObject);
        isAct = true;
        isComplete = true;
    }

    private void GenerateSegmentColliders()
    {
        if(pathPoints.Count < 2) return;
        for(int i = 0; i< pathPoints.Count - 1; i++)
        {
            
            Vector3 start = pathPoints[i];
            Vector3 end = pathPoints[i+1];

            Vector3 center = (start+end) /2f;
            Vector3 direction = end - start;
            float length = direction.magnitude;

            if(length < 0.001f) continue;

            GameObject segment = new GameObject("Collider_" + i);
            segment.tag = "DrawMesh";
            segment.transform.SetParent(colliderContainer.transform);
            segment.transform.position = center;

            segment.transform.right = direction.normalized;

            BoxCollider box = segment.AddComponent<BoxCollider>();
            box.size = new Vector3(length, 0.2f, colliderDepth);
            box.isTrigger = true;
        }
    }
}