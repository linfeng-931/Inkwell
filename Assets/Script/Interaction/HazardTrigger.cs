using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class HazardTrigger : MonoBehaviour
{
    public int damageOnHit = 1;
    public Transform respawnPosition;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered && !other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        isTriggered = true;
        StartCoroutine(HandleHazardRoutine(playerHealth));
    }

    /// <summary>
    /// connect with playerHealth
    /// </summary>
    /// <param name="playerHealth"></param>
    /// <returns></returns>
    private IEnumerator HandleHazardRoutine(PlayerHealth playerHealth)
    {
        Vector3 respawnTarget = respawnPosition.position;
        playerHealth.RespawnFromHazard(respawnTarget, damageOnHit);

        yield return new WaitForSeconds(1.2f);
        isTriggered = false;
    }
}
