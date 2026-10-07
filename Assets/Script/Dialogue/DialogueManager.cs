using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Microsoft.GDK;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // save all loading dialogue
    private Dictionary<string, DialogueData> dialogueDictionary = new Dictionary<string, DialogueData>();
    private HashSet<string> loadedFiles = new HashSet<string>();

    private int currentLineIndex = 0;
    private DialogueData currentPlayingDialogue;
    private bool isFromTimeline = false;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvent.OnDialogueNextPressed += NextLine;
    }

    private void OnDisable()
    {
        GameEvent.OnDialogueNextPressed -= NextLine;
    }

    public void TryPlayDialogue(string targetId, bool fromTimeline = false)
    {
        isFromTimeline = fromTimeline;
        string[] splitStrings = targetId.Split('_');
        string fileName = splitStrings[0];

        if (!loadedFiles.Contains(fileName))
        {
            bool success = LoadDialogueFile(fileName);
            if (!success) return;
        }

        if (dialogueDictionary.TryGetValue(targetId, out DialogueData data))
        {
            if (CheckCondition(data.playCondition))
            {
                ExecuteDialogue(data);
            }
        }
    }

    private bool LoadDialogueFile(string fileName)
    {
        string path = $"Dialogues/{fileName}";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        DialogueDatabase db = JsonConvert.DeserializeObject<DialogueDatabase>(jsonFile.text);

        foreach (var data in db.dialogues)
        {
            if (!dialogueDictionary.ContainsKey(data.id))
            {
                dialogueDictionary.Add(data.id, data);
            }
        }

        loadedFiles.Add(fileName);
        Resources.UnloadAsset(jsonFile);
        return true;
    }

    /// <summary>
    /// can play the dialogue?
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private bool CheckCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        var flags = SaveManager.Instance.currentSave.flags;
        if (flags.TryGetValue(condition, out bool isTrue))
        {
            return isTrue;
        }
        return false;
    }

    /// <summary>
    /// start to play dialogue
    /// </summary>
    /// <param name="data"></param>
    private void ExecuteDialogue(DialogueData data)
    {
        currentPlayingDialogue = data;
        currentLineIndex = 0;

        if (!isFromTimeline)
        {
            // disable player action
            GameEvent.OnCutsceneStateChanged.Invoke(true);
            GameEvent.OnInteractStateChanged.Invoke(true);
        }

        // show ui
        GameEvent.OnDialogueOpened.Invoke();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        string currentText = currentPlayingDialogue.texts[currentLineIndex];

        //text ui
        GameEvent.OnDialogueLineUpdated.Invoke(currentText);
    }

    public void NextLine()
    {
        if (currentPlayingDialogue == null) return;

        currentLineIndex++;
        if (currentLineIndex < currentPlayingDialogue.texts.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            CompleteDialogue();
        }
    }

    public void CompleteDialogue()
    {
        // check if has any action after dialogue
        if (currentPlayingDialogue.actions != null)
        {
            foreach (var action in currentPlayingDialogue.actions)
            {
                ProcessAction(action);
            }
        }

        currentPlayingDialogue = null;
        currentLineIndex = 0;

        // close ui
        GameEvent.OnDialogueClosed.Invoke();

        if(!isFromTimeline){
            GameEvent.OnCutsceneStateChanged.Invoke(false);
            GameEvent.OnInteractStateChanged.Invoke(false);
        }
    }

    private void ProcessAction(DialogueAction action)
    {
        switch (action.type)
        {
            case "set_flag": // don't repeat the dialogue
                break;
            case "give_item":
                break;
            case "play_sound":
                break;
            case "play_timeline":
                break;
            case "auto_save":
                string fileName = string.IsNullOrEmpty(action.key) ? "AutoSave" : action.key;
                SaveManager.Instance.SaveGame(fileName);
                break;
            default:
                break;
        }
    }

    public void ClearDialogueMemory()
    {
        dialogueDictionary.Clear();
        loadedFiles.Clear();
    }
}
