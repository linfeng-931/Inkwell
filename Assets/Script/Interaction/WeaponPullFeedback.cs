using System;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class WeaponPullFeedback : MonoBehaviour
{
    [Serializable]
    public class Stage
    {
        public AudioClip sfx;
        [Range(0f, 3f)] public float shakeForce = 0.5f;
    }

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private Stage[] stages = new Stage[3];

    private int currentStage;

    private void Awake()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Reset()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void SetStage(int index)
    {
        currentStage = Mathf.Clamp(index, 0, Mathf.Max(0, stages.Length - 1));
    }

    public void PlayImpact()
    {
        if (stages == null || stages.Length == 0) return;
        Stage stage = stages[currentStage];

        if (impulseSource != null) impulseSource.GenerateImpulseWithForce(stage.shakeForce);
        if (stage.sfx != null && audioManager != null) audioManager.PlaySFX(stage.sfx);
    }
}