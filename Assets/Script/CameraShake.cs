using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource;

    void Start()
    {
    }

    public void Shake(float intensity, float duration)
    {
        if (impulseSource == null) return;

        impulseSource.ImpulseDefinition.ImpulseDuration = duration;
        impulseSource.GenerateImpulseWithForce(intensity);
    }

    public void Shake(ShakePreset preset)
    {
        if (preset == null)
        {
            Debug.LogWarning("ShakePreset is null");
            return;
        }
        Shake(preset.intensity, preset.duration);
    }
}

[CreateAssetMenu(fileName = "New Shake Preset", menuName = "Camera/Shake Preset")]
public class ShakePreset : ScriptableObject
{
    // Shake intensity & duration
    public float intensity = 0.1f;
    public float duration = 0.2f;
}