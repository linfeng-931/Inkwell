using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地圖進度狀態（唯一的存檔對象）：已探索 zone、已解鎖／已完成地標、地標群組目前位置。
/// 放在 BaseObj。
/// </summary>
[DefaultExecutionOrder(-100)]
public class MapExploration : MonoBehaviour
{
    public static MapExploration Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    readonly HashSet<string> revealed = new();
    readonly HashSet<string> unlockedMarkers = new();
    readonly HashSet<string> completedMarkers = new();
    readonly Dictionary<string, string> groupActive = new();   // groupId -> "roomId/markerId"

    public event Action<string, string> OnZoneRevealed;    // roomId, zoneId
    public event Action<string, string> OnMarkerChanged;   // roomId, markerId
    public event Action<string> OnGroupChanged;            // groupId
    public event Action OnLoaded;                          // 讀檔／重設後，UI 需整體刷新

    /// <summary>每次讀檔或清除就 +1，讓其他系統知道要重置快取</summary>
    public int Version { get; private set; }
    public int RevealedCount => revealed.Count;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public static string Key(string roomId, string id) => $"{roomId}/{id}";

    // ---------- Zone ----------
    public bool IsRevealed(string roomId, string zoneId) => revealed.Contains(Key(roomId, zoneId));
    public bool IsRevealedKey(string key) => revealed.Contains(key);

    public bool Reveal(string roomId, string zoneId)
    {
        if (!revealed.Add(Key(roomId, zoneId))) return false;
        OnZoneRevealed?.Invoke(roomId, zoneId);
        return true;
    }

    // ---------- Marker ----------
    public bool IsMarkerUnlocked(string roomId, string markerId) => unlockedMarkers.Contains(Key(roomId, markerId));
    public bool IsMarkerCompleted(string roomId, string markerId) => completedMarkers.Contains(Key(roomId, markerId));

    public bool UnlockMarker(string roomId, string markerId)
    {
        if (!unlockedMarkers.Add(Key(roomId, markerId))) return false;
        OnMarkerChanged?.Invoke(roomId, markerId);
        return true;
    }

    /// <summary>完成（同時視為已解鎖）</summary>
    public bool CompleteMarker(string roomId, string markerId)
    {
        string k = Key(roomId, markerId);
        bool changed = completedMarkers.Add(k);
        changed |= unlockedMarkers.Add(k);
        if (changed) OnMarkerChanged?.Invoke(roomId, markerId);
        return changed;
    }

    // ---------- Marker Group（會階段性換位置的對象，如商人）----------
    public bool IsGroupActive(string groupId, string roomId, string markerId) =>
        groupActive.TryGetValue(groupId, out var k) && k == Key(roomId, markerId);

    /// <summary>指定群組目前所在的地標。劇情／商人邏輯在對象換位置時呼叫（房間不需要已載入）。</summary>
    public void SetGroupActive(string groupId, string roomId, string markerId)
    {
        string k = Key(roomId, markerId);
        if (groupActive.TryGetValue(groupId, out var cur) && cur == k) return;
        groupActive[groupId] = k;
        OnGroupChanged?.Invoke(groupId);
    }

    /// <summary>讓群組不顯示在任何位置（例如商人離開了地區）</summary>
    public void ClearGroup(string groupId)
    {
        if (groupActive.Remove(groupId)) OnGroupChanged?.Invoke(groupId);
    }

    public void Clear()
    {
        revealed.Clear();
        unlockedMarkers.Clear();
        completedMarkers.Clear();
        groupActive.Clear();
        Version++;
        OnLoaded?.Invoke();
    }

    // ---------- 存檔 ----------
    [Serializable] struct GroupEntry { public string groupId; public string key; }

    [Serializable]
    class SaveData
    {
        public List<string> keys = new();        // 已探索 zone
        public List<string> unlocked = new();    // 已解鎖地標
        public List<string> completed = new();   // 已完成地標
        public List<GroupEntry> groups = new();  // 地標群組目前位置
    }

    public string Serialize()
    {
        var d = new SaveData
        {
            keys = new List<string>(revealed),
            unlocked = new List<string>(unlockedMarkers),
            completed = new List<string>(completedMarkers)
        };
        foreach (var kv in groupActive) d.groups.Add(new GroupEntry { groupId = kv.Key, key = kv.Value });
        return JsonUtility.ToJson(d);
    }

    public void Deserialize(string json)
    {
        revealed.Clear();
        unlockedMarkers.Clear();
        completedMarkers.Clear();
        groupActive.Clear();

        if (!string.IsNullOrEmpty(json))
        {
            var d = JsonUtility.FromJson<SaveData>(json);
            if (d != null)
            {
                if (d.keys != null) foreach (var k in d.keys) revealed.Add(k);
                if (d.unlocked != null) foreach (var k in d.unlocked) unlockedMarkers.Add(k);
                if (d.completed != null) foreach (var k in d.completed) completedMarkers.Add(k);
                if (d.groups != null) foreach (var g in d.groups) groupActive[g.groupId] = g.key;
            }
        }
        Version++;
        OnLoaded?.Invoke();
    }
}
