using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// save ui
/// </summary>
[Serializable]
public class SaveMetaData
{
    public string saveTime;
    public float playTime = 0f;
    public string currentLocation = "核心";
}

[Serializable]
public class PlayerStateData
{
    public int health = 5;
    public int maxHealth = 5;
    public float energy = 100f;
    public float maxEnergy = 100f;
    public string spawnPoint = "TheCore_1";
}

[Serializable]
public class NoteData
{
    // id, amount
    public Dictionary<string, int> items = new Dictionary<string, int>(){
            {"inspiration", 0}
        };
}

/// <summary>
/// main save data
/// </summary>
[Serializable]
public class GameSaveData
{
    public SaveMetaData meta = new SaveMetaData();
    public PlayerStateData player = null;
    public NoteData note = new NoteData();

    // story flag
    public Dictionary<string, bool> flags = new Dictionary<string, bool>();
}