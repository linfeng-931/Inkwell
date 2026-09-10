using UnityEngine;

public enum HookTargetType
{
    AirEnemy,
    GroundEnemy,
    HookPoint
}

public class HookTarget : MonoBehaviour
{
    public HookTargetType targetType;
}
