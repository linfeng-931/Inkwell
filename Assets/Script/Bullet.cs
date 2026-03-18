using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 target;
    public float speed;
    public float isAct;

    private bool end;
    private float timer;
    private float existTimer;
    private bool hasDir;
    private Vector3 dir;
    private Vector3 currentTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = GameObject.FindWithTag("Player").transform.position;
        end = false;
        timer = 0f;
        existTimer = 0f;
        hasDir = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null && !hasDir){
            dir = (target- transform.position).normalized;
            dir.z = 0;
            dir = dir*1000f;
            currentTarget = transform.position + dir;
            hasDir = true;
        }
        if (hasDir && !end)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, Time.deltaTime*speed);
        }
        
        existTimer+=Time.deltaTime;
        if(existTimer > 3f && !end)
        {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(false);
            end = true;
        }

        if (end)
        {
            timer+=Time.deltaTime;
            if(timer > 0.5f) Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")) return;
        transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(false);
        end = true;
    }
}
