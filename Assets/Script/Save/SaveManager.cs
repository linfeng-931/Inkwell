using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public bool canSave = true; // for Demo

    public static SaveManager Instance;
    public GameSaveData currentSave{get; private set;} = new GameSaveData();
    
    // global parameter (prevent destory)
    public static string currentSaveFileName {get; private set;}
    public static string pendingFileName = ""; 
    public static bool isPendingLoad = false;
    public static bool isPendingNewGame = false;

    private string lastSaveKey = "lastPlayedSaveSlot";
    private string SaveDirectory => Application.persistentDataPath;
    private float startTime;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (isPendingLoad)
        {
            if (LoadGame(pendingFileName))
            {  
                startTime = Time.time;
                isPendingLoad = false;
            }
        }
        else if (isPendingNewGame)
        {
            if(StartNewGame(pendingFileName, true))
            {
                startTime = Time.time;
                isPendingNewGame = false;
            }
        }
    }

    public void Button_SaveGame()
    {
        SaveGame();
    }

    public void Button_LoadGame(string fileName)
    {
        string filePath = Path.Combine(SaveDirectory, $"{fileName}.json");
        
        if (File.Exists(filePath))
        {
            pendingFileName = fileName;
            isPendingLoad = true;
            isPendingNewGame = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning("找不到存檔！");
        }
    }

    public bool StartNewGame(string fileName, bool overwrite = false)
    {
        string filePath = Path.Combine(SaveDirectory, $"{fileName}.json");

        if(!overwrite && File.Exists(filePath))
        {
            return false;
        }
        currentSaveFileName = fileName;
        currentSave = new GameSaveData();

        UpdateLastPlayedSaveName(fileName);

        bool isSuccess = SaveGame();
        if (isSuccess)
        {
            GameEvent.OnGameLoaded?.Invoke();
        }

        return isSuccess;
    }

    public bool SaveGame(string fileName = "")
    {
        if(!canSave) return false;

        fileName = string.IsNullOrEmpty(fileName) ? currentSaveFileName : fileName;
        GameEvent.OnGameSaved?.Invoke();
        
        try{
            currentSave.meta.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            currentSave.meta.playTime += Time.time - startTime;
            startTime = Time.time;
            
            string json = JsonConvert.SerializeObject(currentSave, Formatting.Indented);
            string filePath = Path.Combine(SaveDirectory, $"{fileName}.json");
            File.WriteAllText(filePath, json);

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
        string filePath = Path.Combine(SaveDirectory, $"{fileName}.json");
        if (!File.Exists(filePath))
        {
            return false;
        }

        try{
            string json = File.ReadAllText(filePath);
            currentSave = JsonConvert.DeserializeObject<GameSaveData>(json);

            currentSaveFileName = fileName;
            UpdateLastPlayedSaveName(fileName);

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
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// for main menu slot box (button status)
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllSaveFiles()
    {
        if(!Directory.Exists(SaveDirectory)) return new List<string>();
        DirectoryInfo dir = new DirectoryInfo(SaveDirectory);
        FileInfo[] info = dir.GetFiles("*.json").OrderByDescending(p => p.LastWriteTime).ToArray();

        return info.Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToList();
    }

    /// <summary>
    /// for main menu slot box (face)
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public SaveMetaData GetSaveMetaData(string fileName)
    {
        string filePath = Path.Combine(SaveDirectory, $"{fileName}.json");
        try
        {
            string json = File.ReadAllText(filePath);
            // only get meta data
            var data = JsonConvert.DeserializeObject<GameSaveData>(json);
            return data.meta;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return null;
        }
    }

    public string GetLastPlayedSaveName()
    {
        return PlayerPrefs.GetString(lastSaveKey, "");
    }

    private void UpdateLastPlayedSaveName(string fileName)
    {
        PlayerPrefs.SetString(lastSaveKey, fileName);
        PlayerPrefs.Save();
    }
}
