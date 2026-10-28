using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PlayerTriggerZone : MonoBehaviour
{
    public UnityEvent onPlayerEnter;

    public bool triggerOnlyOnce = true;
    private bool hasTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;

        if (collision.collider.CompareTag("Player"))
        {
            onPlayerEnter?.Invoke();

            if (triggerOnlyOnce)
            {
                hasTriggered = true;
            }
        }
    }
}