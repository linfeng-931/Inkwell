using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Transform hitBoxCenter;
    public Vector3 hitBoxSize = new Vector3(1.5f, 1.5f, 1f);
    public LayerMask enemyLayer;

    private HashSet<Collider> alreadyHitEnemies = new HashSet<Collider>(); // avoid hit same enemy twice
    private bool isHitBoxActive = false;
    private int currentDamage = 1;
    private Vector3 currentSize;
    private Vector3 currentOffset;

    void Update()
    {
        if (isHitBoxActive)
        {
            DetectHits();
        }
    }

    private void DetectHits()
    {
        Vector3 actualCenter = hitBoxCenter.position + hitBoxCenter.TransformDirection(currentOffset);

        // find all collider in hit box
        Collider[] hitColliders = Physics.OverlapBox(hitBoxCenter.position, hitBoxSize / 2f, hitBoxCenter.rotation, enemyLayer);

        foreach(Collider col in hitColliders)
        {
            if (!alreadyHitEnemies.Contains(col))
            {
                alreadyHitEnemies.Add(col);

                // camera shake
                CombatFeedbackManager.Instance.TriggerHitFeedback(0.08f, 1.5f);
                col.GetComponent<IEnemy>().TakeDamage(currentDamage);

                // later add voice
            }
        }
    }

    public void EnableHitBox(int damage, Vector3 size, Vector3 offset)
    {
        isHitBoxActive = true;
        currentDamage = damage;
        currentSize = size;
        currentOffset = offset;
        alreadyHitEnemies.Clear();
    }

    public void DisableHitBox()
    {
        isHitBoxActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isHitBoxActive ? Color.red : new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(hitBoxCenter.position, hitBoxCenter.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
    }
}
