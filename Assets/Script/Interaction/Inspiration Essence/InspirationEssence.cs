using UnityEngine;

public class InspirationEssence : MonoBehaviour
{
    public string playerTag = "Player"; 
    public LayerMask groundLayer;
    public float detectRange;
    
    private int num;
    private float maxSpeed;

    private Vector3 currentVelocity;
    private float burstTimer = 0.5f;
    private Transform player;

    // effect
    private Vector3 randomSpin;
    private float windRandomizer;
    private bool isHoming = false;

    // detect ground and rig
    private float gravity = 2f;
    private float groundY;

    public void Initialize(int num, float maxSpeed, Vector3 burstVelocity)
    {
        this.num = num;
        this.maxSpeed = maxSpeed;
        this.currentVelocity = burstVelocity;
    }

    void Start()
    {
        Destroy(gameObject, 30f);

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // wind effect
        randomSpin = new Vector3(
            Random.Range(-360f, 360f), 
            Random.Range(-360f, 360f), 
            Random.Range(-360f, 360f)
        );
        windRandomizer = Random.Range(0f, 100f);

        // detect ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            groundY = hit.point.y + 0.2f;
        }
        else
        {
            groundY = transform.position.y - 0.5f;
        }
    }

    void Update()
    {
        // step1
        if (burstTimer > 0)
        {
            burstTimer -= Time.deltaTime;

            currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, Time.deltaTime * 4f);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, 0, Time.deltaTime * 4f);
            if (transform.position.y > groundY)
            {
                transform.Rotate(randomSpin * Time.deltaTime, Space.World);
            }
            
            isHoming = false;
        }
        else // step2 (follow player)
        {
            float dist = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

            if (dist <= detectRange)
            {
                isHoming = true;
                Vector3 directDir = (player.position - transform.position).normalized;
                
                Vector3 sideDir = Vector3.Cross(directDir, Vector3.up);
                if (sideDir == Vector3.zero) sideDir = Vector3.right; // 防呆

                float swayForce = Mathf.Sin(Time.time * 8f + windRandomizer) * 0.8f; 

                Vector3 windDirection = (directDir + sideDir * swayForce).normalized;

                currentVelocity = Vector3.Lerp(currentVelocity, windDirection * maxSpeed, Time.deltaTime * 2.5f);

                if (dist < 0.5f) 
                {
                    GameEvent.OnInspirationCollected(num);
                    Destroy(gameObject);
                }
            }
            else
            {
                isHoming = false;
                currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, Time.deltaTime * 5f);
                currentVelocity.z = Mathf.Lerp(currentVelocity.z, 0, Time.deltaTime * 5f);
            }
        }

        if (!isHoming)
        {
            // drop
            currentVelocity.y -= gravity * Time.deltaTime;
        }

        // move
        transform.position += currentVelocity * Time.deltaTime;
        if (!isHoming && transform.position.y <= groundY)
        {
            // on ground
            Vector3 pos = transform.position;
            pos.y = groundY;
            transform.position = pos;

            currentVelocity.y = 0;
        }

        if (isHoming && currentVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }
}
