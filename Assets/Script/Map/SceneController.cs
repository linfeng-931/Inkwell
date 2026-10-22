using UnityEngine;

public class SceneController : MonoBehaviour
{
    public string sceneName;
    public SpawnPointInfo[] spawnPointInfos;

    void Start()
    {
        GameEvent.OnGameLoaded += Load;
        GameEvent.OnGameSaved += Save;
    }

    void OnDestroy()
    {
        GameEvent.OnGameLoaded -= Load;
        GameEvent.OnGameSaved -= Save;
    }

    private void Load()
    {
        
    }

    private void Save()
    {
        var data = SaveManager.Instance.currentSave.meta;

        data.currentLocation = sceneName;
    }
}
