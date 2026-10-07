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
    public float playTime;
    public string currentLocation;
    public int playerLevel;
}

[Serializable]
public class PlayerStateData
{
    public int health;
    public int maxHealth;
    public float energy;
    public float maxEnergy;
    public Vector3 position;
}

[Serializable]
public class NoteData
{
    public Dictionary<string, int> items = new Dictionary<string, int>(); // id, amount
}

/// <summary>
/// main save data
/// </summary>
[Serializable]
public class GameSaveData
{
    public SaveMetaData meta = new SaveMetaData();
    public PlayerStateData player = new PlayerStateData();
    public NoteData bag = new NoteData();
    
    // story flag
    public Dictionary<string, bool> flags = new Dictionary<string, bool>();
}