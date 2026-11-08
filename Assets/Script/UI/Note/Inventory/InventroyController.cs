using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventroyController : MonoBehaviour
{
    // item face cell
    public Image[] itemCell;
    public Image[] diaryCell;

    // data
    // public int penLevel {get; private set;}
    // public int heartLevel {get; private set;}
    // public int healthLevel {get; private set;}
    // public bool[] diaries {get; private set;}
    // public Dictionary<string, int> items {get; private set;}

    void OnEnable()
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
        NoteData noteData = SaveManager.Instance.currentSave.note;

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
}
