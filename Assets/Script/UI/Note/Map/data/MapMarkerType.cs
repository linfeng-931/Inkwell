using UnityEngine;

[CreateAssetMenu(menuName = "Map/Marker Type")]
public class MapMarkerType : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public Sprite completedIcon; // if finish sth
    public Color tint = Color.white;
    public float iconSize = 32f;
    public int sortOrder;
    public bool hideWhenCompleted;
}
