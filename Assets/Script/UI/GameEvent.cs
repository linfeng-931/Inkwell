using System;
using UnityEngine;

public static class GameEvent
{
    // Action<current, max>
    public static Action<int, int> OnHealthChanged;
    public static Action<float, float> OnEnergyChanged;

    // other ui event
}
