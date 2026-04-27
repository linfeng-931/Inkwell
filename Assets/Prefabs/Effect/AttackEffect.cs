using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public GameObject attackEffect;

    void OnTriggerEnter(Collider other)
    {
        Vector3 impactPoint = other.ClosestPoint(transform.position);
        if (other.CompareTag("Enemy"))
        {
            Instantiate(attackEffect, impactPoint, Quaternion.identity);
        }
    }
}
