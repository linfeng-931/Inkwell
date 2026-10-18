using System;
using UnityEngine;

public static class GameEvent
{
    // load and save data
    public static Action OnGameLoaded;
    public static Action OnGameSaved;
    
    // player action
    public static Action<bool> OnInteractStateChanged;

    // Action<current, max>
    public static Action<int, int> OnHealthChanged;
    public static Action<float, float> OnEnergyChanged;

    // other ui event
    public static Action OnEscapePressed;
    public static Action OnToggleNote;
    public static Action<bool> OnToggleFade;
    public static Action OnWorldInteractPressed;

    // cutscene (true = enter, false = exist)
    public static Action<bool> OnCutsceneStateChanged;

    // dialogue
    public static Action OnDialogueOpened;
    public static Action OnDialogueClosed;
    public static Action<string> OnDialogueLineUpdated;
    public static Action OnDialogueNextPressed;

    // item introduction
    public static Action OnItemIntroOpened;
    public static Action OnItemIntroClosed;
}
