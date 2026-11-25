using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// list of all rooms
/// </summary>
[CreateAssetMenu(menuName = "Map/Database")]
public class MapDatabase : ScriptableObject
{
    public List<MapRoomData> rooms = new();

    public Rect mapBounds;

    public Rect GetPanBounds()
    {
        if (mapBounds.width > 0f && mapBounds.height > 0f) return mapBounds;

        bool any = false;
        Rect r = default;
        foreach (var room in rooms)
        {
            if (room == null) continue;
            if (!any) { r = room.mapRect; any = true; continue; }
            r.xMin = Mathf.Min(r.xMin, room.mapRect.xMin);
            r.yMin = Mathf.Min(r.yMin, room.mapRect.yMin);
            r.xMax = Mathf.Max(r.xMax, room.mapRect.xMax);
            r.yMax = Mathf.Max(r.yMax, room.mapRect.yMax);
        }
        return r;
    }

    public bool TryGetRoom(string roomId, out MapRoomData room)
    {
        foreach (var r in rooms)
            if (r != null && r.roomId == roomId) { room = r; return true; }
        room = null;
        return false;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        var seen = new HashSet<string>();
        foreach (var r in rooms)
        {
            if (r == null) { Debug.LogWarning($"[{name}] rooms 內有空項目", this); continue; }
            if (string.IsNullOrEmpty(r.roomId)) Debug.LogWarning($"[{name}] {r.name} 未填 roomId", this);
            else if (!seen.Add(r.roomId)) Debug.LogWarning($"[{name}] roomId 重複：{r.roomId}", this);
        }
    }
#endif
}
