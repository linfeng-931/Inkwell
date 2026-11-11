using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventroyController : MonoBehaviour
{
    // item face cell
    public Transform itemContainer;
    public Transform diaryContainer;
    public Sprite defaultSprite;
    public string defaultContentID;

    private List<Image> itemImage = new List<Image>();
    private List<Image> diaryImage = new List<Image>();
    private List<ItemCell> itemCell = new List<ItemCell>();
    private List<ItemCell> diaryCell = new List<ItemCell>();
    private List<TextMeshProUGUI> itemText = new List<TextMeshProUGUI>();

    // record item data (static data)
    private List<ItemData> itemDatas = new List<ItemData>();

    // data
    // public int penLevel {get; private set;}
    // public int heartLevel {get; private set;}
    // public int healthLevel {get; private set;}
    // public bool[] diaries {get; private set;}
    // public Dictionary<string, int> items {get; private set;}

    void Start()
    {
        foreach (Transform item in itemContainer)
        {
            itemImage.Add(item.GetComponent<Image>());
            itemCell.Add(item.GetComponent<ItemCell>());
            itemText.Add(item.GetChild(0).GetComponent<TextMeshProUGUI>());
        }

        foreach (Transform diary in diaryContainer)
        {
            diaryImage.Add(diary.GetComponent<Image>());
            diaryCell.Add(diary.GetComponent<ItemCell>());
        }
    }

    void OnEnable()
    {
        GameEvent.OnGameLoaded += Load;
        GameEvent.OnGameSaved += Save;
        GameEvent.OnNormalItemChanged += UpdateItem;
    }

    void OnDestroy()
    {
        GameEvent.OnGameLoaded -= Load;
        GameEvent.OnGameSaved -= Save;
        GameEvent.OnNormalItemChanged -= UpdateItem;
    }

    private void Load()
    {
        NoteData noteData = SaveManager.Instance.currentSave.note;

        // get item data (text, imagePath)
        foreach (string id in noteData.items.Keys)
        {
            LoadNewItem(id);
        }

        // handle items

        // handle diaries
        bool[] diaries = noteData.diaries;
        // for(int i = 0; i<diaries.Length; i++)
        // {
        //     if (diaries[i])
        //     {
        //         diaryCell[i]
        //     }
        // }
    }

    private void Save()
    {

    }

    public void UpdateItem(string id, int addAmount)
    {
        // item aleardy in inventory
        if (itemDatas.Find(item => item.id == id) != null)
        {
            int currentAmount = SaveManager.Instance.currentSave.note.items[id];
            SaveManager.Instance.currentSave.note.items[id] += addAmount;

            // if spend item
            if(addAmount < 0 && currentAmount >= Math.Abs(addAmount))
            {
                int finalAmount = currentAmount + addAmount;
                if(finalAmount > 0){
                    SaveManager.Instance.currentSave.note.items[id] = finalAmount;
                    UpdateTextAmount(id, finalAmount);
                }
                else
                {
                    SaveManager.Instance.currentSave.note.items.Remove(id);
                    RemoveItem(id);
                }
            }
            else if(addAmount > 0)
            {
                SaveManager.Instance.currentSave.note.items[id] += addAmount;
                UpdateTextAmount(id, SaveManager.Instance.currentSave.note.items[id]);
            }
        }
        else if(addAmount > 0)
        {
            // handle new item
            LoadNewItem(id);
            SaveManager.Instance.currentSave.note.items[id] = addAmount;
            UpdateTextAmount(id, addAmount);
        }
    }

    public void UnlockDiary(string id)
    {

    }

    /// <summary>
    /// if item not in current list
    /// </summary>
    /// <param name="id"></param>
    private void LoadNewItem(string id)
    {
        // find data
        ItemData itemData = ItemManager.Instance.GetItemData(id);
        itemDatas.Add(itemData);
        string imagePath = itemData.stageIconPaths[itemData.currentStage];
        Sprite loadedSprite = Resources.Load<Sprite>(imagePath);

        // find empty cell
        int targetIndex = -1;
        for (int i = 0; i < itemCell.Count; i++)
        {
            if (itemCell[i].itemID == defaultContentID)
            {
                targetIndex = i;
                break;
            }
        }

        // set data to cell
        if (targetIndex != -1)
        {
            itemCell[targetIndex].itemID = id;
            if (loadedSprite != null)
            {
                itemImage[targetIndex].sprite = loadedSprite;
            }
        }
        else
        {
            // alert words
        }
    }

    /// <summary>
    /// remove item from inventory
    /// </summary>
    /// <param name="id"></param>
    private void RemoveItem(string id)
    {
        for (int i = 0; i < itemCell.Count; i++)
        {
            if (itemCell[i].itemID == id)
            {
                itemCell[i].itemID = defaultContentID;
                itemImage[i].sprite = defaultSprite;
                itemText[i].gameObject.SetActive(false);
                break;
            }
        }

        itemDatas.RemoveAll(item => item.id == id);
    }

    /// <summary>
    /// update the display number
    /// </summary>
    /// <param name="id"></param>
    /// <param name="num"></param>
    private void UpdateTextAmount(string id, int num)
    {
        for (int i = 0; i < itemCell.Count; i++)
        {
            if (itemCell[i].itemID == id)
            {
                itemText[i].gameObject.SetActive(true);
                itemText[i].text = num.ToString();
                break;
            }
        }
    }
}
