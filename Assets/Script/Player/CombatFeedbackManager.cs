using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CombatFeedbackManager : MonoBehaviour
{
    public static CombatFeedbackManager Instance {get; private set;}

    private Coroutine hitStopCoroutine;
    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        Instance = this;
        
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void TriggerHitFeedback(float hitStopDuration = 0.01f, float shakeForce = 1f)
    {
        TriggerHitStop(hitStopDuration);
        TriggerCameraShake(shakeForce);
    }

    private void TriggerHitStop(float duration)
    {
        if(hitStopCoroutine != null)
        {
            StopCoroutine(hitStopCoroutine);
        }
        hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }
    
    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.01f;

        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    private void TriggerCameraShake(float force)
    {
        impulseSource.GenerateImpulseWithForce(force);
    }
}
