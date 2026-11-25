using UnityEngine;

public class MapMarkerSource : MonoBehaviour
{
    [Tooltip("命名後請勿更改!!")]
    public string markerId;
    public MapMarkerType type;
    public MarkerReveal reveal = MarkerReveal.WhenZoneRevealed;
    [Tooltip("同群組的地標同一時間只會顯示一個（例如商人的各個停留點）空白＝一般地標")]
    public string groupId;
    public string title;
    [TextArea] public string description;

    string roomId;

    string ResolveRoomId()
    {
        if (roomId != null) return roomId;
        foreach (var a in MapRoomAnchor.Active)
        {
            if (a != null && a.data != null && a.gameObject.scene == gameObject.scene)
            {
                roomId = a.data.roomId;
                break;
            }
        }
        return roomId;
    }

    /// <summary>
    /// display sign
    /// </summary>
    public void Unlock()
    {
        var id = ResolveRoomId();
        if (id != null && MapExploration.Instance != null)
            MapExploration.Instance.UnlockMarker(id, markerId);
    }

    public void Complete()
    {
        var id = ResolveRoomId();
        if (id != null && MapExploration.Instance != null)
            MapExploration.Instance.CompleteMarker(id, markerId);
    }

    public void ActivateInGroup()
    {
        var id = ResolveRoomId();
        if (id != null && !string.IsNullOrEmpty(groupId) && MapExploration.Instance != null)
            MapExploration.Instance.SetGroupActive(groupId, id, markerId);
    }

#if UNITY_EDITOR
    void Reset() => markerId = System.Guid.NewGuid().ToString("N").Substring(0, 8);

    void OnDrawGizmos()
    {
        Gizmos.color = type != null ? type.tint : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f,
            type != null ? $"{type.displayName} ({markerId})" : "未指定種類");
    }
#endif
}
