using System;
using UnityEngine;

public static class GameEvent
{
    // load and save data
    public static Action OnGameLoaded;
    public static Action OnGameSaved;
    public static Action<string> OnFlagUpdated; // for story
    
    // player action
    public static Action<PlayerActionType> OnActionTriggered;
    public static Action<bool> OnInteractStateChanged;
    public static Action OnPlayerReborn;
    public static Action<Transform> OnPlayerSave;

    // Action<current, max>
    public static Action<int, int> OnHealthChanged;
    public static Action<float, float> OnEnergyChanged;

    // other ui event
    public static Action<UIActionType> OnUIActionTriggered;
    public static Action OnEscapePressed;
    public static Action currentCloseCallback;
    public static Action OnToggleNote;
    public static Action<bool> OnNoteStateChanged;
    public static Action<bool> OnToggleFade;
    public static Action OnWorldInteractPressed;
    public static Action<int> OnInspirationCollected;
    public static Action<CursorMode> OnCursorModeChanged;
    public static Action<string> OnRefreshItemPage;
    public static Action<RectTransform> OnUpdateSelectionFrame;

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
    public static Action OnItemIntroChange; // for note

    // note
    public static Action<string, int> OnNormalItemChanged;
}
