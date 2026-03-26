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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        case1 = GameObject.FindGameObjectsWithTag("CameraCase1");
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

        Rotate();
    }

    void Rotate()
    {
        if(!flag) return;

        float moveDistance = startPos.x - player.transform.position.x;
        transform.RotateAround(center, Vector3.up, -1f*moveDistance*speed);
        startPos = player.transform.position;
        
    }
}
