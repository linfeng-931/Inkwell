using UnityEngine;

[System.Serializable]
public class AttackData
{
    public string animationName;
    public float duration;
    public float comboStartTime;
    public float comboEndTime;

    [Header("Physics")]
    public float forwardThrust = 15f;
    public float thrustDuration = 0.08f;

    [Header("HitBox")]
    public float activeHitBoxStartTime = 0.005f;
    public float activeHitBoxEndTime = 0.04f;
}
