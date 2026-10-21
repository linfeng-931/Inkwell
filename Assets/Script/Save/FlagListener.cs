using UnityEngine;
using UnityEngine.Events;

public class FlagListener : MonoBehaviour
{
    public string flagID;
    public UnityEvent onUnlocked;
    public UnityEvent onLocked;

    void Start()
    {
        GameEvent.OnGameLoaded += CheckFlag;
        GameEvent.OnFlagUpdated += HandleFlagUpdate;

        CheckFlag(); // init
    }

    void OnDestroy()
    {
        GameEvent.OnGameLoaded -= CheckFlag;
        GameEvent.OnFlagUpdated -= HandleFlagUpdate;
    }

    private void HandleFlagUpdate(string updatedFlagID)
    {
        if(this.flagID == updatedFlagID)
        {
            CheckFlag();
        }
    }

    private void CheckFlag()
    {
        var flags = SaveManager.Instance.currentSave.flags;

        bool isUnlocked = flags.ContainsKey(flagID) && flags[flagID] == true;

        if (isUnlocked)
        {
            onUnlocked?.Invoke();
        }
        else
        {
            onLocked?.Invoke();
        }
    }
}
