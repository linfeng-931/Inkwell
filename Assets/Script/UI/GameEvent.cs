using System;
using UnityEngine;

public static class GameEvent
{
    // player action
    public static Action<bool> OnInteractStateChanged;

    // Action<current, max>
    public static Action<int, int> OnHealthChanged;
    public static Action<float, float> OnEnergyChanged;

    // other ui event
    public static Action OnToggleNote;
}
