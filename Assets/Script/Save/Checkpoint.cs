using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            Vector3 respawnPos = transform.position;

            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.UpdateCheckpoint(respawnPos);
        }
    }
}
