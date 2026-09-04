using UnityEngine;
using Unity.Cinemachine;

public class ImpulseListenerSetup : MonoBehaviour
{
    // According to each Cinemachines give different shake gain value
    public float gainShake = 1f;
    private CinemachineImpulseListener listener;

    private void Awake()
    {
        listener.Gain = gainShake;
    }

    public void SetGain(float newGain)
    {
        gainShake = newGain;
        if (listener != null) listener.Gain = newGain;
    }
}
