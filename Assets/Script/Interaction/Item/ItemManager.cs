using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public ItemData currentItem {get; private set;}

    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();
    private HashSet<string> loadedFiles = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) 
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
        LoadItemFile("items");
    }

    private bool LoadItemFile(string fileName)
    {
        string path = $"Items/{fileName}";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        ItemDatabase db = JsonConvert.DeserializeObject<ItemDatabase>(jsonFile.text);

        foreach (var data in db.items)
        {
            if (!itemDictionary.ContainsKey(data.id))
            {
                itemDictionary.Add(data.id, data);
            }
        }

        loadedFiles.Add(fileName);
        Resources.UnloadAsset(jsonFile);
        return true;
    }

    public void ExecuteItemIntro(string id, bool isInNote)
    {
        // disable player action
        GameEvent.OnInteractStateChanged.Invoke(true);

        currentItem = GetItemData(id);

        // show ui
        if (isInNote)
        {
            GameEvent.OnItemIntroChange.Invoke();
        }
        else
        {
            GameEvent.OnItemIntroOpened.Invoke();
        }
    }

    public ItemData GetItemData(string id)
    {
        itemDictionary.TryGetValue(id, out ItemData item);
        return item;
    }

    /// <summary>
    /// catch obj and record in saveManager
    /// </summary>
    /// <param name="id"></param>
    /// <param name="amount"></param>
    public void PlayerGetItem(string id, bool autoSave = true, int amount = 1)
    {
        if(SaveManager.Instance.currentSave.bag == null)
        {
            SaveManager.Instance.currentSave.bag = new NoteData();
        }
        else
        {
            if (SaveManager.Instance.currentSave.bag.items.ContainsKey(id))
            {
                SaveManager.Instance.currentSave.bag.items[id] += amount;
            }
            else{
                SaveManager.Instance.currentSave.bag.items.Add(id, amount);
            }
        }

        if (autoSave)
        {
            SaveManager.Instance.SaveGame();
        }
    }
}
