using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public GameSaveData currentSave{get; private set;} = new GameSaveData();
    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    public void SaveGame(string fileName)
    {
        currentSave.meta.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string json = JsonConvert.SerializeObject(currentSave, Formatting.Indented);
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        File.WriteAllText(filePath, json);
    }

    public void LoadGame(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        
        string json = File.ReadAllText(filePath);
        currentSave = JsonConvert.DeserializeObject<GameSaveData>(json);
    }
}
