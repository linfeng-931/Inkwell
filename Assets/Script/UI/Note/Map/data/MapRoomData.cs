using UnityEngine;

/// <summary>某個 zone 在「地圖座標」中的範圍（由 MapRoomAnchor 烘焙，房間卸載後迷霧仍靠它繪製）</summary>
[System.Serializable]
public struct ZoneMapRect
{
    public string zoneId;
    public Rect mapRect;
}

[CreateAssetMenu(menuName = "Map/Room Data")]
public class MapRoomData : ScriptableObject
{
    public string roomId;
    public Sprite mapSprite;
    public Rect mapRect;
    public int priority;

    [Tooltip("由 MapRoomAnchor 烘焙，請勿手改")]
    public ZoneMapRect[] zoneRects;

    [Tooltip("由 MapRoomAnchor 烘焙（來自場景內的 MapMarkerSource），請勿手改")]
    public MarkerMapData[] markers;

    public Vector2 WorldToMap(Vector2 world, Rect worldBounds)
    {
        var t = new Vector2(
            (world.x - worldBounds.xMin) / worldBounds.width,
            (world.y - worldBounds.yMin) / worldBounds.height);
        return NormToMap(t);
    }

    public Vector2 NormToMap(Vector2 n) =>
        new Vector2(mapRect.xMin + n.x * mapRect.width, mapRect.yMin + n.y * mapRect.height);
}
