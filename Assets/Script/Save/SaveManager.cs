using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public GameSaveData currentSave{get; private set;} = new GameSaveData();

    private string SaveDirectory => Application.persistentDataPath;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool SaveGame(string fileName)
    {
        try{
            currentSave.meta.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            string json = JsonConvert.SerializeObject(currentSave, Formatting.Indented);
            string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
            File.WriteAllText(filePath, json);

            GameEvent.OnGameSaved?.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }

    public bool LoadGame(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        if (!File.Exists(filePath))
        {
            return false;
        }

        try{
            string json = File.ReadAllText(filePath);
            currentSave = JsonConvert.DeserializeObject<GameSaveData>(json);

            GameEvent.OnGameLoaded?.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }

    public void DeleteSave(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        if (!File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public List<string> GetAllSaveFiles()
    {
        if(!Directory.Exists(SaveDirectory)) return new List<string>();
        DirectoryInfo dir = new DirectoryInfo(SaveDirectory);
        FileInfo[] info = dir.GetFiles("*.json").OrderByDescending(p => p.LastWriteTime).ToArray();

        return info.Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToList();
    }
}
