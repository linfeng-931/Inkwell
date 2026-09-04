using System;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Camera Shake")]
    public CameraShake cameraShake;

    [Header("Camera Priority")]
    public int defaultPriority = 10;
    public int activeCamPriority = 20;
    private CinemachineVirtualCameraBase currentOverrideCam; // Current Active Camera

    public int drawKey = 1;

    //ground check for camera
    //public Transform groundCheckPoint;
    //public LayerMask groundLayer;
    //public float checkRadius = 0.3f;

    public bool eligibleExtrude;
    public bool canExtrude;

    //[Header("Closer Camera")]
    //public float maxDis;
    //public float closerSpeed;

    //private float oriZ;
    //private int closerStep;

    //private GameObject player;
    //private Rigidbody playerRig;
    //private Vector3 playerPos;
    //private PlayerController playerController;
    //private float delayTimer;
    //private bool leaveGround;
    //private Vector3 startTarget;
    //private bool onDrawPoint;

    [Header("Draw Line")]
    private GameObject lastDrawPoint;
    private GameObject[] drawPointList;
    private int drawPointNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onDrawPointInit();

        //player = GameObject.FindWithTag("Player");
        //playerRig = player.GetComponent<Rigidbody>();
        //playerController = player.GetComponent<PlayerController>();
        //delayTimer = 0f;
        //startTarget = new Vector3(0,0,0);
        //leaveGround = false;
        //onDrawPoint = false;
        //drawPointList = new GameObject[2];
        //drawPointNum = 0;
        //canExtrude = false;
        //eligibleExtrude = false;
        //cameraStatus = 0;
        //closerTimer = 0f;
        //oriZ = transform.position.z;
        //closerStep = 0;
    }

    void onDrawPointInit()
    {
        drawPointList = new GameObject[2];
        drawPointNum = 0;
        canExtrude = false;
        eligibleExtrude = false;
    }

    void MouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hitObject))
        {
            DrawPoint dr = null;
            if (hitObject.transform.CompareTag("DrawPoint"))
            {
                Debug.Log("打中了: " + hitObject.transform.name);
                lastDrawPoint = hitObject.transform.gameObject;
                dr = hitObject.transform.GetComponent<DrawPoint>();
                if (dr != null)
                {
                    if (dr.isAct != true) dr.isAct = true;
                }

                if (Input.GetMouseButtonDown(drawKey))
                {
                    drawPointList[0] = lastDrawPoint;
                    eligibleExtrude = true;
                    drawPointNum++;
                }
                else if (Input.GetMouseButtonUp(drawKey))
                {
                    if (drawPointList[0] != lastDrawPoint) drawPointList[1] = lastDrawPoint;
                    drawPointNum++;
                }
            }
            else
            {
                if (lastDrawPoint != null) lastDrawPoint.GetComponent<DrawPoint>().isAct = false;
                lastDrawPoint = null;
                if (Input.GetMouseButtonDown(drawKey) || Input.GetMouseButtonUp(drawKey))
                {
                    drawPointNum = 0;
                    drawPointList = new GameObject[2];
                }
            }
        }
        else
        {
            if (lastDrawPoint != null)
            {
                lastDrawPoint.GetComponent<DrawPoint>().isAct = false;
                lastDrawPoint = null;
            }
        }
    }

    void Update()
    {
        MouseRay();
        if (drawPointNum >= 2)
        {
            canExtrude = true;
            drawPointNum = 0;
            drawPointList = new GameObject[2];
        }
    }

    // Switch Camera
    public void SwitchCam(CinemachineVirtualCameraBase targetCam) {
        if (targetCam == null) return;

        if (currentOverrideCam != null && currentOverrideCam != targetCam) {
            currentOverrideCam.Priority = defaultPriority;
        }

        targetCam.Priority = activeCamPriority;
        currentOverrideCam = targetCam;
    }

    // Reset Target Camera
    public void ResetCam(CinemachineVirtualCameraBase targetCam) {
        if (targetCam == null) return;

        if (currentOverrideCam != targetCam) return;

        targetCam.Priority = defaultPriority;
        currentOverrideCam = null;
    }

    // Call this function when player hurts or something changes in the scene
    public void CameraShake(ShakePreset prese) {
        if (cameraShake == null) {
            Debug.Log("找不到 CameraShake");
            return;
        }
        

        cameraShake.Shake(prese);
    }

    //void FollowPlayer()
    //{
    //    playerPos = player.transform.position;
    //    int dirX = playerRig.linearVelocity.x > 0 ? 1 : -1;
    //    int dirY = playerRig.linearVelocity.y > 0 ? 1: -1;
    //    float offsetY = playerRig.linearVelocity.y * 0.1f;
    //    offsetY = offsetY > 0.2f ? 0.2f : offsetY;

    //    float moveOffsetX = -0.2f * dirX;
    //    float stopOffsetX = 0.2f * dirX;
    //    float moveOffsetY = offsetY * dirY;
        
    //    if(playerRig.linearVelocity.x != 0)
    //    {   
    //        Vector3 target = new Vector3(playerPos.x+moveOffsetX, transform.position.y, transform.position.z);
    //        float targetX = Mathf.Lerp(transform.position.x, target.x, 0.5f*Time.deltaTime);
    //        float targetX2 = Mathf.Lerp(transform.position.x, target.x, 3f*Time.deltaTime); //certain camera following
    //        if(Math.Abs(transform.position.x - playerPos.x) > 0.3f) transform.position = new Vector3(targetX2, transform.position.y, transform.position.z);
    //        else transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    //    }
    //    else if(playerRig.linearVelocity.x == 0)
    //    {
    //        startTarget = new Vector3(0,0,0);
    //        if(Math.Abs(transform.position.x- (playerPos.x+stopOffsetX)) > 0.05f)
    //        transform.position = Vector3.MoveTowards(transform.position, new Vector3(playerPos.x+stopOffsetX, transform.position.y, transform.position.z), Time.deltaTime*1.5f);
    //        else
    //        transform.position = new Vector3(playerPos.x+stopOffsetX, transform.position.y, transform.position.z);
    //    }

    //    if(playerRig.linearVelocity.y >= 0.00001 || playerRig.linearVelocity.y <= -0.00001)
    //    {   
    //        if(delayTimer > 0.2f)
    //        {
    //            if(!Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer) || leaveGround){
    //                leaveGround = true;
    //                Vector3 target = new Vector3(transform.position.x, playerPos.y+moveOffsetY, transform.position.z);
    //                float targetY = Mathf.Lerp(transform.position.y, target.y, 3f*Time.deltaTime);
    //                float targetY2 = Mathf.Lerp(transform.position.y, target.y, Math.Abs(playerRig.linearVelocity.y)*2f *Time.deltaTime);
    //                if(Math.Abs(transform.position.y - playerPos.y) > 0.21f) transform.position = new Vector3(transform.position.x, targetY2, transform.position.z);  
    //                else transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    //            }
    //        }
    //        else delayTimer += Time.deltaTime;
    //    }
    //    else
    //    {
    //        delayTimer = 0;
    //        leaveGround = false;
    //        if(Math.Abs(transform.position.y- (playerPos.y+stopOffsetY)) > 0.005f)
    //        {
    //            float targetY = Mathf.Lerp(transform.position.y, playerPos.y+stopOffsetY, 3f*Time.deltaTime);
    //            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    //        }
    //        else{
    //            transform.position = new Vector3(transform.position.x, playerPos.y+stopOffsetY, transform.position.z);
    //        }
    //    }
    //}
}
