using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    public string flagToSet;
    public bool targetState = true;
    public bool autoSave = true;

    public void ExecuteTrigger()
    {
        var flags = SaveManager.Instance.currentSave.flags;
        flags[flagToSet] = targetState;

        GameEvent.OnFlagUpdated?.Invoke(flagToSet);

        if(autoSave){
            SaveManager.Instance.SaveGame();
        }
    }
}
