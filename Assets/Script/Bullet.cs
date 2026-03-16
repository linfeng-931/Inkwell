using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 target;
    public float speed;
    public float isAct;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = GameObject.FindWithTag("Player").transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime*speed);
    }
}
