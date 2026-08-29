using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource;

    void Start()
    {
    }

    public void Shake(float intensity, float time)
    {
        if (impulseSource != null)
        {
            Debug.Log("晃動強度" + intensity);
            impulseSource.ImpulseDefinition.ImpulseDuration = time;
            impulseSource.GenerateImpulseWithForce(intensity);
        }
        else{
            Debug.Log("找不到晃動元件");

        }
    }

    //void Update()
    //{
    //    if (shakeTimer > 0)
    //    {
    //        shakeTimer -= Time.deltaTime;
    //        if (shakeTimer <= 0 && noise != null)
    //        {
    //            noise.AmplitudeGain = 0f;
    //        }
    //    }
    //}
}
