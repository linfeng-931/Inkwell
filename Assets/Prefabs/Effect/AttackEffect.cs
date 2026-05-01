using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public GameObject attackEffect0;
    public GameObject attackEffect1;
    public int attackAmount;

    private float timer;
    private bool canEffect0;

    void Start()
    {
        timer = 0f;
        canEffect0 = true;
        attackAmount = 0;
    }

    void Update()
    {
        if (!canEffect0)
        {
            timer += Time.deltaTime;
            if(timer > 0.5f){
                canEffect0 = true;
                timer = 0f;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(attackAmount == 0) return;

        Vector3 impactPoint = other.ClosestPoint(transform.position);
        if (other.CompareTag("Enemy"))
        {
            Instantiate(attackEffect1, impactPoint, Quaternion.identity);
            attackAmount--;
            if(attackAmount<0) attackAmount = 0;
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("UNAttack"))
        {
            Instantiate(attackEffect0, impactPoint, Quaternion.identity);
            attackAmount--;
            if(attackAmount<0) attackAmount = 0;
        }
    }
}
