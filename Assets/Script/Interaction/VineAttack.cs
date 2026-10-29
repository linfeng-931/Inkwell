using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VineAttack : MonoBehaviour
{
    [Header("Respawn Setting")]
    public Transform hazardRespawnPoint;

    public int damageAmount = 1;

    [Header("Transition Setting")]
    public float fallDelayTime = 1f;
    public float fadeWaitTime = 1f;

    private Collider vineCollider;
    private bool isTriggered = false; 

    private void Awake()
    {
        vineCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerHealth != null && playerController != null)
            {
                if (playerHealth.currentHealth <= 0 || playerController.isHazardHurt)
                    return;

                StartCoroutine(HazardRespawnSequence(playerHealth, playerController));
            }
        }
    }

    private IEnumerator HazardRespawnSequence(PlayerHealth playerHealth, PlayerController playerController)
    {
        isTriggered = true;

        playerController.isHazardHurt = true;

        PlayerLock.Lock(this);
        PlayerLock.Lock(this, PlayerLockType.Combat);
        vineCollider.enabled = false;

        yield return new WaitForFixedUpdate();
        playerController.TransitionToState<FallState>();

        float t = 0f;
        while (t < fallDelayTime && !playerController.isGrounded)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Vector3 targetPos = hazardRespawnPoint != null
            ? hazardRespawnPoint.position
            : playerController.transform.position;

        playerHealth.RespawnFromHazard(targetPos, damageAmount);

        yield return new WaitUntil(() =>
            !(playerController.currentState is HurtState) &&
            !(playerController.currentState is DeathState));

        PlayerLock.Unlock(this);
        PlayerLock.Unlock(this, PlayerLockType.Combat);
        vineCollider.enabled = true;
        isTriggered = false;
    }
}