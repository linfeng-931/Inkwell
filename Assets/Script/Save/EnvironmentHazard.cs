using System.Collections;
using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    public Transform safeRespawnPoint;
    public int damage = 1;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerController != null)
        {
            Vector3 respawnPos = safeRespawnPoint != null
                ? safeRespawnPoint.position
                : playerController.lastCheckpointPosition;

            playerHealth.RespawnFromHazard(respawnPos, damage);
        }
    }

}
