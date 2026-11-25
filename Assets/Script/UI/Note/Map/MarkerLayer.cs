using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerLayer : MonoBehaviour
{
    [SerializeField] RectTransform root;
    [SerializeField] MapView view;

    class Widget
    {
        public MapRoomData room;
        public MarkerMapData data;
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Image image;
        public string key;
    }

    readonly List<Widget> widgets = new();
    readonly Dictionary<string, Widget> byKey = new();
    MapExploration ex;
    bool inited;
    float lastZoom = -1f;

    void Awake()
    {
        if (root == null) root = (RectTransform)transform;
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.localScale = Vector3.one;
    }

    void OnEnable() => TryInit();

    void TryInit()
    {
        if (inited) return;
        var svc = MapService.Instance;
        ex = MapExploration.Instance;
        if (svc == null || svc.Database == null || ex == null) return;

        var all = new List<Widget>();
        foreach (var room in svc.Database.rooms)
        {
            if (room == null || room.markers == null) continue;
            foreach (var m in room.markers)
            {
                if (m.type == null) continue;
                all.Add(new Widget { room = room, data = m, key = MapExploration.Key(room.roomId, m.markerId) });
            }
        }
        all.Sort((a, b) => a.data.type.sortOrder.CompareTo(b.data.type.sortOrder));

        foreach (var w in all)
        {
            w.gameObject = new GameObject($"Marker {w.key}", typeof(RectTransform), typeof(Image));
            w.rectTransform = (RectTransform)w.gameObject.transform;
            w.rectTransform.SetParent(root, false);
            w.rectTransform.anchorMin = w.rectTransform.anchorMax = w.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            w.rectTransform.anchoredPosition = w.room.NormToMap(w.data.normPos);
            w.rectTransform.sizeDelta = Vector2.one * w.data.type.iconSize;

            w.image = w.gameObject.GetComponent<Image>();
            w.image.color = w.data.type.tint;
            w.image.preserveAspect = true;
            w.image.raycastTarget = false;

            widgets.Add(w);
            byKey[w.key] = w;
        }

        ex.OnZoneRevealed += HandleZoneRevealed;
        ex.OnMarkerChanged += HandleMarkerChanged;
        ex.OnGroupChanged += HandleGroupChanged;
        ex.OnLoaded += RefreshAll;
        inited = true;
        RefreshAll();
    }

    void OnDestroy()
    {
        if (ex == null) return;
        ex.OnZoneRevealed -= HandleZoneRevealed;
        ex.OnMarkerChanged -= HandleMarkerChanged;
        ex.OnGroupChanged -= HandleGroupChanged;
        ex.OnLoaded -= RefreshAll;
    }

    void LateUpdate()
    {
        if (!inited || view == null) return;
        float z = view.Zoom;
        if (Mathf.Approximately(z, lastZoom)) return;
        lastZoom = z;
        ApplyScale();
    }

    void ApplyScale()
    {
        float s = view != null ? view.InvZoom : 1f;
        foreach (var w in widgets) w.rectTransform.localScale = Vector3.one * s;
    }

    bool IsVisible(Widget w)
    {
        string room = w.room.roomId, id = w.data.markerId;
        if (!string.IsNullOrEmpty(w.data.groupId) && !ex.IsGroupActive(w.data.groupId, room, id)) return false;
        if (w.data.type.hideWhenCompleted && ex.IsMarkerCompleted(room, id)) return false;
        if (ex.IsMarkerUnlocked(room, id)) return true;

        switch (w.data.reveal)
        {
            case MarkerReveal.Always: return true;
            case MarkerReveal.WhenZoneRevealed:
                return !string.IsNullOrEmpty(w.data.zoneId) && ex.IsRevealed(room, w.data.zoneId);
            default: return false;
        }
    }

    void Refresh(Widget w)
    {
        var t = w.data.type;
        bool done = ex.IsMarkerCompleted(w.room.roomId, w.data.markerId);
        w.image.sprite = done && t.completedIcon != null ? t.completedIcon : t.icon;
        w.gameObject.SetActive(IsVisible(w));
    }

    void RefreshAll()
    {
        foreach (var w in widgets) Refresh(w);
        ApplyScale();
    }

    void HandleZoneRevealed(string roomId, string zoneId)
    {
        foreach (var w in widgets)
            if (w.room.roomId == roomId && w.data.zoneId == zoneId) Refresh(w);
    }

    void HandleGroupChanged(string groupId)
    {
        foreach (var w in widgets)
            if (w.data.groupId == groupId) Refresh(w);
    }

    void HandleMarkerChanged(string roomId, string markerId)
    {
        if (byKey.TryGetValue(MapExploration.Key(roomId, markerId), out var w)) Refresh(w);
    }
}
