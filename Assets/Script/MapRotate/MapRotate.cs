using UnityEngine;

public class MapRotate : MonoBehaviour
{
    public float returnSpeed = 4f;
    public float rotateThreshold = 5f;

    private GameObject[] case1;
    private GameObject player;
    private bool flag;
    private int currentArea;
    private Vector3 startPos;
    private Vector3 center;
    private float speed;
    private float radius;
    private float totalRotatedAngle = 0f;
    private float lastPlayerX;

    private Quaternion originalRotation;
    private bool isRotatingStarted = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        case1 = GameObject.FindGameObjectsWithTag("CameraCase1");
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!flag)
        {
            for (int i = 0; i < case1.Length; i++)
            {
                if (case1[i].GetComponent<CameraChangeArea>().flag)
                {
                    flag = true;
                    currentArea = i;
                    center = case1[i].transform.position;
                    speed = case1[i].GetComponent<CameraChangeArea>().speed;
                    radius = case1[i].GetComponent<CameraChangeArea>().radius;
                    startPos = player.transform.position;
                    lastPlayerX = player.transform.position.x;
                    originalRotation = case1[i].GetComponent<CameraChangeArea>().oriQua;
                }
            }
        }
        else
        {
            if (!case1[currentArea].GetComponent<CameraChangeArea>().flag)
            {
                flag = false;
            }
        }

        HandleRotation();
    }

    void HandleRotation()
    {
        if (flag)
        {
            center = case1[currentArea].transform.position;
            float currentXMovement = startPos.x - player.transform.position.x;

            if (Mathf.Abs(currentXMovement) > rotateThreshold && !isRotatingStarted)
            {
                isRotatingStarted = true;
                startPos = player.transform.position;
                lastPlayerX = player.transform.position.x;
                return; 
            }
            else if(!isRotatingStarted) return;

            float targetTotalAngle = (currentXMovement / (2 * Mathf.PI * radius)) * 360f;
            float angleToRotate = targetTotalAngle - totalRotatedAngle;

            Vector3 cylinderAxis = new Vector3(center.x, player.transform.position.y, center.z);
            transform.RotateAround(cylinderAxis, Vector3.up, -angleToRotate);

            float deltaX = player.transform.position.x - lastPlayerX;
            transform.position += new Vector3(deltaX, 0, 0);

            totalRotatedAngle += angleToRotate;
            lastPlayerX = player.transform.position.x;
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -0.1f);
        }
        else
        {
            if (Quaternion.Angle(transform.rotation, originalRotation) > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * returnSpeed);
                totalRotatedAngle = 0f;
            }
            else
            {
                transform.rotation = originalRotation;
                totalRotatedAngle = 0f;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (flag && player != null)
        {
            Gizmos.color = Color.red;
            Vector3 cylinderAxis = new Vector3(center.x, player.transform.position.y, center.z);
            Gizmos.DrawSphere(cylinderAxis, 1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cylinderAxis, player.transform.position);

            Gizmos.color = Color.cyan;
        }
    }
}
