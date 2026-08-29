using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineCamera vcam;

    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;

    void Start()
    {
        if (vcam != null)
        {
            noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else {
            Debug.Log("§ä¤£¨ìvcam");
        }
    }

    public void Shake(float intensity, float time)
    {
        if (noise != null)
        {
            noise.AmplitudeGain = intensity;
            shakeTimer = time;
        }
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0 && noise != null)
            {
                noise.AmplitudeGain = 0f;
            }
        }
    }
}
