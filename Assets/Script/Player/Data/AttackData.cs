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
    public Vector3 hitboxSize = new Vector3(1.5f, 1.5f, 1f);
    public Vector3 hitboxOffset = new Vector3(1f, 0f, 0f);
}
