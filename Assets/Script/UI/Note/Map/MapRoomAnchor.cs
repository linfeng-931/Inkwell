using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoomZone
{
    [Tooltip("命名後請勿更改!!")]
    public string zoneId;
    public Vector2 center;   // 相對 Anchor 的局部座標
    public Vector2 size;     // 局部尺寸（世界單位）
}

/// <summary>放在每個房間場景的根物件。回報該房間「當下」的世界範圍。不支援旋轉。</summary>
public class MapRoomAnchor : MonoBehaviour
{
    public static readonly HashSet<MapRoomAnchor> Active = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Active.Clear();

    public MapRoomData data;
    public bool autoBake = true;
    public RoomZone[] zones =
    {
        new RoomZone { zoneId = "A", center = Vector2.zero, size = new Vector2(32, 18) }
    };

    public Rect ZoneToWorld(RoomZone z)
    {
        var s = transform.lossyScale;
        var size = new Vector2(z.size.x * Mathf.Abs(s.x), z.size.y * Mathf.Abs(s.y));
        var c = (Vector2)transform.position + Vector2.Scale(z.center, new Vector2(s.x, s.y));
        return new Rect(c - size * 0.5f, size);
    }

    public Rect WorldBounds
    {
        get
        {
            if (zones == null || zones.Length == 0) return default;
            var r = ZoneToWorld(zones[0]);
            for (int i = 1; i < zones.Length; i++)
            {
                var z = ZoneToWorld(zones[i]);
                r.xMin = Mathf.Min(r.xMin, z.xMin);
                r.yMin = Mathf.Min(r.yMin, z.yMin);
                r.xMax = Mathf.Max(r.xMax, z.xMax);
                r.yMax = Mathf.Max(r.yMax, z.yMax);
            }
            return r;
        }
    }

    public bool Contains(Vector2 p, out float area)
    {
        bool hit = false;
        area = 0;
        if (zones == null) return false;
        foreach (var z in zones)
        {
            var r = ZoneToWorld(z);
            area += r.width * r.height;
            if (r.Contains(p)) hit = true;
        }
        return hit;
    }

    public int FindZone(Vector2 p)
    {
        if (zones == null) return -1;
        int best = -1;
        float bestArea = float.MaxValue;
        for (int i = 0; i < zones.Length; i++)
        {
            var r = ZoneToWorld(zones[i]);
            if (!r.Contains(p)) continue;
            float a = r.width * r.height;
            if (a < bestArea) { bestArea = a; best = i; }
        }
        return best;
    }

    void OnEnable() => Active.Add(this);
    void OnDisable() => Active.Remove(this);

#if UNITY_EDITOR
    // bake
    ZoneMapRect[] ComputeZoneRects()
    {
        if (data == null || zones == null || zones.Length == 0) return null;
        var b = WorldBounds;
        if (b.width <= 0f || b.height <= 0f) return null;

        var result = new ZoneMapRect[zones.Length];
        for (int i = 0; i < zones.Length; i++)
        {
            var w = ZoneToWorld(zones[i]);
            var min = data.WorldToMap(w.min, b);
            var max = data.WorldToMap(w.max, b);
            result[i] = new ZoneMapRect
            {
                zoneId = zones[i].zoneId,
                mapRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y)
            };
        }
        return result;
    }

    MapMarkerSource[] FindMarkerSources()
    {
        var list = new List<MapMarkerSource>();
        foreach (var root in gameObject.scene.GetRootGameObjects())
            list.AddRange(root.GetComponentsInChildren<MapMarkerSource>(true));
        return list.ToArray();
    }

    MarkerMapData[] ComputeMarkers(bool log)
    {
        if (data == null || zones == null || zones.Length == 0) return null;
        var b = WorldBounds;
        if (b.width <= 0f || b.height <= 0f) return null;

        var result = new List<MarkerMapData>();
        var ids = new HashSet<string>();
        foreach (var s in FindMarkerSources())
        {
            if (string.IsNullOrEmpty(s.markerId))
            { if (log) Debug.LogWarning($"[{name}] 地標 {s.name} 未填 markerId，已略過", s); continue; }
            if (s.type == null)
            { if (log) Debug.LogWarning($"[{name}] 地標 {s.name} 未指定種類，已略過", s); continue; }
            if (!ids.Add(s.markerId))
            { if (log) Debug.LogWarning($"[{name}] markerId 重複：{s.markerId}（{s.name}），已略過", s); continue; }

            Vector2 p = s.transform.position;
            var n = new Vector2((p.x - b.xMin) / b.width, (p.y - b.yMin) / b.height);
            int zi = FindZone(p);

            if (log)
            {
                if (n.x < 0f || n.x > 1f || n.y < 0f || n.y > 1f)
                    Debug.LogWarning($"[{name}] 地標 {s.name} 在房間外框之外，地圖上會跑出房間圖", s);
                else if (zi < 0 && s.reveal == MarkerReveal.WhenZoneRevealed)
                    Debug.LogWarning($"[{name}] 地標 {s.name} 不在任何 zone 內，不會因探索而顯示（請移入 zone 或改為 Manual／Always）", s);
            }

            result.Add(new MarkerMapData
            {
                markerId = s.markerId,
                groupId = s.groupId ?? "",
                type = s.type,
                normPos = n,
                zoneId = zi >= 0 ? zones[zi].zoneId : "",
                reveal = s.reveal,
                title = s.title ?? "",
                description = s.description ?? ""
            });
        }
        result.Sort((x, y) => string.CompareOrdinal(x.markerId, y.markerId));   // 順序固定，才能比對是否過期
        return result.ToArray();
    }

    [ContextMenu("烘焙 Zone 與地標")]
    public void Bake()
    {
        var rects = ComputeZoneRects();
        var markers = ComputeMarkers(true);
        if (rects == null || markers == null)
        {
            Debug.LogWarning($"[{name}] 無法烘焙：未指定 MapRoomData，或 zones 為空／尺寸為 0", this);
            return;
        }
        UnityEditor.Undo.RecordObject(data, "Bake Map Room");
        data.zoneRects = rects;
        data.markers = markers;
        UnityEditor.EditorUtility.SetDirty(data);
    }

    public bool IsBakeUpToDate()
    {
        var expectedZones = ComputeZoneRects();
        var expectedMarkers = ComputeMarkers(false);
        if (expectedZones == null || expectedMarkers == null) return false;

        var hz = data.zoneRects;
        if (hz == null || hz.Length != expectedZones.Length) return false;
        for (int i = 0; i < hz.Length; i++)
        {
            if (hz[i].zoneId != expectedZones[i].zoneId) return false;
            if (!Approx(hz[i].mapRect, expectedZones[i].mapRect)) return false;
        }

        var hm = data.markers;
        if (hm == null || hm.Length != expectedMarkers.Length) return false;
        for (int i = 0; i < hm.Length; i++)
            if (!MarkerEqual(hm[i], expectedMarkers[i])) return false;

        return true;
    }

    static bool Approx(Rect a, Rect b) =>
        Mathf.Abs(a.xMin - b.xMin) < 0.01f && Mathf.Abs(a.yMin - b.yMin) < 0.01f &&
        Mathf.Abs(a.xMax - b.xMax) < 0.01f && Mathf.Abs(a.yMax - b.yMax) < 0.01f;

    static bool MarkerEqual(MarkerMapData a, MarkerMapData b) =>
        a.markerId == b.markerId && a.type == b.type && a.reveal == b.reveal &&
        a.zoneId == b.zoneId && a.groupId == b.groupId && a.title == b.title && a.description == b.description &&
        Mathf.Abs(a.normPos.x - b.normPos.x) < 0.0005f && Mathf.Abs(a.normPos.y - b.normPos.y) < 0.0005f;

    void ValidateZoneIds()
    {
        var seen = new HashSet<string>();
        foreach (var z in zones)
        {
            if (string.IsNullOrEmpty(z.zoneId))
                Debug.LogWarning($"[{name}] 有 zone 未填 zoneId", this);
            else if (!seen.Add(z.zoneId))
                Debug.LogWarning($"[{name}] zoneId 重複：{z.zoneId}", this);
        }
    }

    void OnDrawGizmos()
    {
        if (zones == null) return;

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
        foreach (var z in zones)
        {
            var r = ZoneToWorld(z);
            Gizmos.DrawWireCube(r.center, r.size);
        }

        var b = WorldBounds;
        Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
        Gizmos.DrawWireCube(b.center, b.size);
        UnityEditor.Handles.Label(b.center, data ? data.roomId : "⚠ 未指定 MapRoomData");
    }

    void OnValidate()
    {
        if (zones == null || zones.Length == 0) return;
        ValidateZoneIds();

        if (data == null) return;
        var b = WorldBounds;
        if (b.height <= 0f || data.mapRect.height <= 0f) return;
        float a = b.width / b.height, m = data.mapRect.width / data.mapRect.height;
        if (Mathf.Abs(a - m) > 0.01f)
            Debug.LogWarning($"[{name}] 外框比例 {a:F3} 與地圖圖比例 {m:F3} 不一致，地圖會變形", this);
    }
#endif
}
