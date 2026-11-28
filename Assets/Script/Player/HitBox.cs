using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Transform hitBoxCenter;
    public Vector3 hitBoxSize = new Vector3(1.5f, 1.5f, 2f);
    public LayerMask enemyLayer;
    public LayerMask hookObjectLayer;

    [Header("VFX")]
    public GameObject hitVfxPrefab;
    private int hitTypeCount = 3;

    [Header("SFX")]
    public AudioManager audioManager;
    public AudioClip atkSfx;

    private HashSet<IEnemy> alreadyHitEnemies = new HashSet<IEnemy>(); // avoid hit same enemy twice
    private bool isHitBoxActive = false;
    private int currentDamage = 1;
    private Vector3 currentSize;

    private readonly Collider[] hitBuffer = new Collider[32];

    void FixedUpdate()
    {
        if (isHitBoxActive)
        {
            DetectHits();
        }
    }

    private void DetectHits()
    {
        Vector3 center = hitBoxCenter.position;
        Vector3 halfExtents = hitBoxSize / 2f;
        Quaternion rotation = hitBoxCenter.rotation;

        // enemy
        int count = Physics.OverlapBoxNonAlloc(
            center, halfExtents, hitBuffer, rotation, enemyLayer,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider col = hitBuffer[i];

            IEnemy enemy = col.GetComponent<IEnemy>();
            if (enemy == null) enemy = col.GetComponentInParent<IEnemy>();

            if (enemy != null && alreadyHitEnemies.Add(enemy))
            {
                CombatFeedbackManager.Instance.TriggerHitFeedback(0.08f, 1.5f);
                enemy.TakeDamage(currentDamage);

                if (hitVfxPrefab != null)
                {
                    Vector3 hitPoint = col.ClosestPoint(center);
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
        vfx.GetComponent<Animator>().Play("HitEffect"+Random.Range(0, hitTypeCount), 0, 0f);
        audioManager.PlaySFX(atkSfx);
        vfx.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    public void EnableHitBox(int damage)
    {
        currentDamage = damage;
        alreadyHitEnemies.Clear();
        isHitBoxActive = true;

        DetectHits();
    }

    public void DisableHitBox()
    {
        if (!isHitBoxActive) return;

        DetectHits();
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
