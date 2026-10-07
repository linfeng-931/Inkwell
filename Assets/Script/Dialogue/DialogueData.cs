using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueAction
{
    public string type; // ex. "play_sound"
    public string key; // target ID
    public string value; // value or status
}

[System.Serializable]
public class DialogueData
{
    public string id;
    public List<string> texts;
    public string playCondition;
    public List<DialogueAction> actions;
}

public class DialogueDatabase
{
    public List<DialogueData> dialogues;
}
