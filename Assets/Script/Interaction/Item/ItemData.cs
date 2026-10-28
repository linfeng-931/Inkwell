using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public string id;
    public string[] stageNames; // for player
    public string[] stageIconPaths;
    public Sprite[] stageIcons; // stage/level different face
    public string[] stageDescs; // function
    public string[] stageSubDescs; // story
    public int currentStage = 0;
    public bool isUniqueItem;
    public UnityEngine.UI.Image[] itemImage;
}

public class ItemDatabase
{
    public List<ItemData> items;
}