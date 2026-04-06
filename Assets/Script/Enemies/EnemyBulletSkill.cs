using UnityEngine;

public class EnemyBulletSkill : MonoBehaviour
{
    public GameObject bulletPrefab;
    
    private Transform playerTrans;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= 2.0f)
        {
            GameObject bullet1 = Instantiate(bulletPrefab, transform.position, transform.rotation);
            bullet1.GetComponent<Bullet>().target = playerTrans.position;
            timer = 0f;
        }
        
    }
}
