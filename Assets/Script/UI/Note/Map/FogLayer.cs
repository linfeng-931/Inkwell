using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogLayer : MonoBehaviour
{
    [SerializeField] RectTransform root;
    [SerializeField] Sprite fogSprite;
    [SerializeField] Color fogColor = new(0.05f, 0.07f, 0.1f, 0.92f);

    readonly Dictionary<string, Image> tiles = new();
    MapExploration ex;
    bool inited;

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

        foreach (var room in svc.Database.rooms)
        {
            if (room == null || room.zoneRects == null) continue;
            foreach (var z in room.zoneRects)
            {
                var go = new GameObject($"Fog {room.roomId}/{z.zoneId}", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(root, false);
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = z.mapRect.center;
                rt.sizeDelta = z.mapRect.size;

                var img = go.GetComponent<Image>();
                img.sprite = fogSprite;
                img.color = fogColor;
                img.raycastTarget = false;
                tiles[MapExploration.Key(room.roomId, z.zoneId)] = img;
            }
        }

        ex.OnZoneRevealed += HandleRevealed;
        ex.OnLoaded += Refresh;
        inited = true;
        Refresh();
    }

    void OnDestroy()
    {
        if (ex == null) return;
        ex.OnZoneRevealed -= HandleRevealed;
        ex.OnLoaded -= Refresh;
    }

    void HandleRevealed(string roomId, string zoneId)
    {
        if (tiles.TryGetValue(MapExploration.Key(roomId, zoneId), out var img)) img.enabled = false;
    }

    void Refresh()
    {
        foreach (var kv in tiles) kv.Value.enabled = !ex.IsRevealedKey(kv.Key);
    }
}
