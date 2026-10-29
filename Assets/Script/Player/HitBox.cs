using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Transform hitBoxCenter;
    public Vector3 hitBoxSize = new Vector3(1.5f, 1.5f, 1f);
    public LayerMask enemyLayer;
    public LayerMask hookObjectLayer;

    [Header("VFX")]
    public GameObject hitVfxPrefab;

    [Header("SFX")]
    public AudioManager audioManager;
    public AudioClip atkSfx;

    private HashSet<IEnemy> alreadyHitEnemies = new HashSet<IEnemy>(); // avoid hit same enemy twice
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
        Collider[] hitColliders = Physics.OverlapBox(hitBoxCenter.position, hitBoxSize / 2f, hitBoxCenter.rotation, enemyLayer);

        foreach (Collider col in hitColliders)
        {
            IEnemy enemy = col.GetComponent<IEnemy>();
            if (enemy == null) enemy = col.GetComponentInParent<IEnemy>(); 

            if (enemy != null && !alreadyHitEnemies.Contains(enemy))
            {
                alreadyHitEnemies.Add(enemy);

                CombatFeedbackManager.Instance.TriggerHitFeedback(0.08f, 1.5f);
                enemy.TakeDamage(currentDamage);

                if (hitVfxPrefab != null)
                {
                    Vector3 hitPoint = col.ClosestPoint(hitBoxCenter.position);
                    SpawnHitEffect(hitPoint);
                }
            }
        }

        // find all hook objects which was hitted
        Collider[] hookObjectColliders = Physics.OverlapBox(hitBoxCenter.position, currentSize / 2f, hitBoxCenter.rotation, hookObjectLayer);
        foreach (Collider col in hookObjectColliders)
        {
            HookTarget hookTarget = col.GetComponentInParent<HookTarget>();
            if (hookTarget != null)
            {
                hookTarget.TriggerHitAnimation();
            }
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        GameObject vfx = Instantiate(hitVfxPrefab, position, Quaternion.identity);
        audioManager.PlaySFX(atkSfx);
        vfx.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    public void EnableHitBox(int damage)
    {
        isHitBoxActive = true;
        currentDamage = damage;
        alreadyHitEnemies.Clear();
    }

    public void DisableHitBox()
    {
        isHitBoxActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitBoxCenter == null) return;

        Gizmos.color = isHitBoxActive ? Color.red : new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(hitBoxCenter.position, hitBoxCenter.rotation, Vector3.one);
        
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
    }
}
