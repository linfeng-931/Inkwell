using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform content;
    [SerializeField] RectTransform roomLayer;
    [SerializeField] RectTransform playerMarker;

    [Header("Pan / Zoom")]
    [SerializeField] float panSpeed = 600f;
    [SerializeField] float zoomSpeed = 1.5f; 
    [SerializeField] float scrollZoomStep = 0.2f;
    [SerializeField] Vector2 zoomRange = new(0.5f, 4f);
    [SerializeField] float smoothing = 12f;
    [SerializeField] bool keepMarkerScreenSize = true;

    [Header("Input")]
    [SerializeField] InputActionReference pan, zoom, zoomScroll, recenter;
    Vector2 PanInput => pan.action.ReadValue<Vector2>();
    float ZoomInput => zoom.action.ReadValue<float>();
    bool RecenterPressed => recenter.action.WasPressedThisFrame();
    float ScrollInput
    {
        get
        {
            if (zoomScroll == null) return 0f;
            float y = zoomScroll.action.ReadValue<Vector2>().y;
            return Mathf.Abs(y) > 0.01f ? Mathf.Sign(y) : 0f;
        }
    }

    Vector2 focus, targetFocus;
    float currentZoom = 1f, targetZoom = 1f;
    MapDatabase db;
    Rect panBounds;
    bool inited;
    bool pendingCenter;

    public float Zoom => currentZoom;
    public float InvZoom => keepMarkerScreenSize ? 1f / currentZoom : 1f; 

    void Awake()
    {
        // set center according to Anchor / Pivot 
        Normalize(content, false);
        Normalize(roomLayer, true);
        Normalize(playerMarker, false);
    }

    static void Normalize(RectTransform rt, bool resetTransform)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        if (resetTransform)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
    }

    bool EnsureInit()
    {
        if (inited) return true;
        var svc = MapService.Instance;
        if (svc == null || svc.Database == null) return false;

        db = svc.Database;
        panBounds = db.GetPanBounds();
        BuildRooms();
        inited = true;
        return true;
    }

    void OnEnable()
    {
        pan.action.Enable();
        zoom.action.Enable();
        recenter.action.Enable();
        zoomScroll?.action.Enable();

        // set player to center
        pendingCenter = true;
        if (EnsureInit()) TryCenterPending(instant: true);
    }

    void BuildRooms()
    {
        var sorted = new List<MapRoomData>(db.rooms);
        sorted.RemoveAll(r => r == null);
        sorted.Sort((a, b) => a.priority.CompareTo(b.priority));

        foreach (var r in sorted)
        {
            var go = new GameObject(r.roomId, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(roomLayer, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = r.mapRect.center;
            rt.sizeDelta = r.mapRect.size;

            var img = go.GetComponent<Image>();
            img.sprite = r.mapSprite;
            img.raycastTarget = false;
        }
    }

    void Update()
    {
        if (!EnsureInit()) return;
        float dt = Time.unscaledDeltaTime;

        var panIn = PanInput;
        if (panIn.sqrMagnitude > 0.0001f) pendingCenter = false;
        targetFocus += panIn * (panSpeed / currentZoom) * dt;
        ClampFocus();

        float zoomDelta = ZoomInput * zoomSpeed * dt + ScrollInput * scrollZoomStep;
        targetZoom = Mathf.Clamp(targetZoom * Mathf.Exp(zoomDelta), zoomRange.x, zoomRange.y);

        if (RecenterPressed) CenterOnPlayer(instant: false);
        TryCenterPending(instant: true);

        float k = 1f - Mathf.Exp(-smoothing * dt);
        focus = Vector2.Lerp(focus, targetFocus, k);
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, k);

        content.localScale = Vector3.one * currentZoom;
        content.anchoredPosition = -focus * currentZoom;
    }


    void LateUpdate()
    {
        if (!inited || playerMarker == null) return;
        var svc = MapService.Instance;
        bool found = svc != null && svc.HasLocation;

        if (playerMarker.gameObject.activeSelf != found) playerMarker.gameObject.SetActive(found);
        if (!found) return;

        playerMarker.anchoredPosition = svc.PlayerMapPos;
        playerMarker.localScale = Vector3.one * InvZoom;
    }

    void ClampFocus()
    {
        if (panBounds.width <= 0f || panBounds.height <= 0f) return;
        targetFocus.x = Mathf.Clamp(targetFocus.x, panBounds.xMin, panBounds.xMax);
        targetFocus.y = Mathf.Clamp(targetFocus.y, panBounds.yMin, panBounds.yMax);
    }

    void TryCenterPending(bool instant)
    {
        if (!pendingCenter) return;
        if (CenterOnPlayer(instant)) pendingCenter = false;
    }

    bool CenterOnPlayer(bool instant)
    {
        var svc = MapService.Instance;
        if (svc == null || !svc.HasLocation) return false;

        targetFocus = svc.PlayerMapPos;
        ClampFocus();
        if (instant) focus = targetFocus;
        return true;
    }
}
