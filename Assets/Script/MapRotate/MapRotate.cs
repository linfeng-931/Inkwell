using UnityEngine;

public class MapRotate : MonoBehaviour
{
    private GameObject[] case1;
    private PlayerController playerController;
    private GameObject player;
    private bool flag;
    private int currentArea;
    private Vector3 startPos;
    private Vector3 center;
    private float speed;
    private float totalRotatedAngle = 0f;

    private Quaternion originalRotation;
    public float returnSpeed = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        case1 = GameObject.FindGameObjectsWithTag("CameraCase1");
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!flag)
        {
            for(int i = 0; i<case1.Length; i++)
            {
                if (case1[i].GetComponent<CameraChangeArea>().flag)
                {
                    flag = true;
                    currentArea = i;
                    center = case1[i].transform.position;
                    speed = case1[i].GetComponent<CameraChangeArea>().speed;
                    startPos = player.transform.position;
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

    void Rotate()
    {
        if(!flag) return;

        float moveDistance = startPos.x - player.transform.position.x;
        transform.RotateAround(center, Vector3.up, -1f*moveDistance*speed);
        startPos = player.transform.position;
        
    }
    void HandleRotation()
    {
        if (flag)
        {
            // 正在區域內：隨玩家移動旋轉
            /*float moveDistance = startPos.x - player.transform.position.x;
            transform.RotateAround(center, Vector3.up, -1f * moveDistance * speed);
            startPos = player.transform.position;*/
            float currentXMovement = startPos.x - player.transform.position.x;
            float targetTotalAngle = currentXMovement * speed;
            float angleToRotate = targetTotalAngle - totalRotatedAngle;
            transform.RotateAround(center, Vector3.up, -angleToRotate);
            totalRotatedAngle += angleToRotate;
        }
        else
        {
            // 不在區域內：緩慢插值回原位
            // 如果角度差距很小，就直接等於原始值，避免微小抖動
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
}
