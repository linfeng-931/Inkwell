using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MapService : MonoBehaviour
{
    public static MapService Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    [SerializeField] MapDatabase database;
    [SerializeField] Transform player;

    public MapDatabase Database => database;
    public Transform Player { get => player; set => player = value; }

    public MapRoomAnchor CurrentAnchor { get; private set; }
    public MapRoomData CurrentRoom => CurrentAnchor ? CurrentAnchor.data : null;
    public string CurrentZoneId { get; private set; }

    public bool HasLocation { get; private set; }
    public bool IsInsideRoom { get; private set; }
    public Vector2 PlayerMapPos { get; private set; }

    public event Action<MapRoomData> OnCurrentRoomChanged;

    MapRoomAnchor lastRevealAnchor;
    int lastRevealZone = -1;
    int lastExploreVersion = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void LateUpdate()
    {
        if (player == null || database == null)
        {
            HasLocation = false;
            IsInsideRoom = false;
            return;
        }
        Locate(player.position);
    }

    void Locate(Vector3 worldPos)
    {
        Vector2 p = worldPos;   // z = 0 (XY)

        // find the scene player inside
        MapRoomAnchor best = null;
        float bestArea = float.MaxValue;
        foreach (var a in MapRoomAnchor.Active)
        {
            if (a == null || a.data == null) continue;
            if (!a.Contains(p, out float area)) continue;

            if (best == null
                || a.data.priority > best.data.priority
                || (a.data.priority == best.data.priority && area < bestArea))
            {
                best = a;
                bestArea = area;
            }
        }

        bool inside = best != null;
        IsInsideRoom = inside;

        // change scene
        if (inside && best != CurrentAnchor)
        {
            var prevRoom = CurrentRoom;
            CurrentAnchor = best;
            if (prevRoom != best.data) OnCurrentRoomChanged?.Invoke(best.data);
        }

        if (CurrentAnchor == null || !MapRoomAnchor.Active.Contains(CurrentAnchor))
        {
            CurrentAnchor = null;
            CurrentZoneId = null;
            HasLocation = false;
            return;
        }

        // 
        var wb = CurrentAnchor.WorldBounds;
        var data = CurrentAnchor.data;
        if (wb.width <= 0f || wb.height <= 0f || data.mapRect.width <= 0f || data.mapRect.height <= 0f)
        {
            HasLocation = false;
            return;
        }

        var mapPos = data.WorldToMap(p, wb);

        // 
        if (!inside)
        {
            mapPos.x = Mathf.Clamp(mapPos.x, data.mapRect.xMin, data.mapRect.xMax);
            mapPos.y = Mathf.Clamp(mapPos.y, data.mapRect.yMin, data.mapRect.yMax);
        }

        PlayerMapPos = mapPos;
        HasLocation = true;

        //
        int zi = inside ? CurrentAnchor.FindZone(p) : -1;
        CurrentZoneId = zi >= 0 ? CurrentAnchor.zones[zi].zoneId : null;

        var ex = MapExploration.Instance;
        if (ex == null) return;

        if (ex.Version != lastExploreVersion)
        {
            lastExploreVersion = ex.Version;
            lastRevealAnchor = null;
            lastRevealZone = -1;
        }

        if (zi >= 0 && (CurrentAnchor != lastRevealAnchor || zi != lastRevealZone))
        {
            lastRevealAnchor = CurrentAnchor;
            lastRevealZone = zi;
            if (!string.IsNullOrEmpty(CurrentZoneId) && !string.IsNullOrEmpty(data.roomId))
                ex.Reveal(data.roomId, CurrentZoneId);
        }
    }
}
