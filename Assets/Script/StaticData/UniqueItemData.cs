using UnityEngine;

[CreateAssetMenu(fileName = "UniqueItemData", menuName = "Scriptable Objects/UniqueItemData")]
public class UniqueItemData : ScriptableObject
{
    public string itemID;          // for editor (ex. pen)
    public Sprite[] stageIcons;    // stage/level different face
    public string[] stageNames;    // for player
    public string[] stageDescs;
}
