using UnityEngine;

public enum MarkerReveal
{
    WhenZoneRevealed,
    Manual,
    Always 
}

[System.Serializable]
public struct MarkerMapData
{
    public string markerId; 
    public MapMarkerType type;
    public Vector2 normPos; 
    public string zoneId;
    public string groupId;
    public MarkerReveal reveal;
    public string title;
    public string description;
}
