using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PlayerTriggerZone : MonoBehaviour
{
    public UnityEvent onPlayerEnter;

    public bool triggerOnlyOnce = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayerEnter?.Invoke();

            if (triggerOnlyOnce)
            {
                other.isTrigger = false;
            }
        }
    }
}