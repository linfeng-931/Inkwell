using UnityEngine;

public class CameraPeekController : MonoBehaviour
{
    [Header("Target Setting")]
    public Transform cameraTarget; //empty obj

    [Header("Move Parameter")]
    public float timeToWait = 1.0f;
    public float peekDistance = 4.0f;
    public float peekSpeed = 2.0f;

    private float idleTimer = 0f;
    private Vector3 defaultLocalPos;

    private Rigidbody rb;

    void Start()
    {
        defaultLocalPos = cameraTarget.localPosition;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        float targetOffsetX = 0f;

        if (!isMoving)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= timeToWait)
            {
                targetOffsetX = -peekDistance;
            }

            Vector3 targetLocalPos = new Vector3(defaultLocalPos.x + targetOffsetX, defaultLocalPos.y, defaultLocalPos.z);
            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, targetLocalPos, Time.deltaTime * peekSpeed);
        }
        else
        {
            idleTimer = 0f;
            targetOffsetX = 0f;

            Vector3 targetLocalPos = new Vector3(defaultLocalPos.x, defaultLocalPos.y, defaultLocalPos.z);
            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, targetLocalPos, Time.deltaTime * (peekSpeed * 3f));
        }
    }
}
