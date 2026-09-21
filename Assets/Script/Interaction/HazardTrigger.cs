using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class HazardTrigger : MonoBehaviour
{
    public int damageOnHit = 1;
    public Transform respawnPosition;

    [Header("Animation")]
    public Animator fadeAni;
    public float fadeOutWaitTime = 0.5f;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered && !other.CompareTag("Player")){
            Debug.Log("找不到玩家");
            return; }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            isTriggered = true;
            StartCoroutine(HandleHazardRoutine(playerHealth));
        }
        else {
            Debug.Log("找不到 playerHealth ");
        }
    }

    private IEnumerator HandleHazardRoutine(PlayerHealth playerHealth) {
        // Play Animation
        if (fadeAni != null)
        {
            fadeAni.SetTrigger("changeScene");
        }

        if (fadeOutWaitTime > 0f)
        {
            yield return new WaitForSeconds(fadeOutWaitTime);
        }


        Vector3 respawnTarget = respawnPosition != null ? respawnPosition.position : transform.position;
        StartCoroutine(playerHealth.RespawnFromHazard(respawnTarget, damageOnHit));

        if (fadeAni != null)
        {
            fadeAni.SetTrigger("returnStart");
        }

        yield return new WaitForSeconds(0.2f);
        isTriggered = false;
    }
}
