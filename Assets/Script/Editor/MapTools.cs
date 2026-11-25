using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MapTools
{
    [MenuItem("Tools/Map/Bake All Anchors In Open Scenes")]
    static void BakeAll()
    {
        var anchors = Object.FindObjectsByType<MapRoomAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var a in anchors) a.Bake();
        AssetDatabase.SaveAssets();
        Debug.Log($"[Map] 已烘焙 {anchors.Length} 個 Anchor");
    }

    [MenuItem("Tools/Map/Validate Databases")]
    static void ValidateDatabases()
    {
        int issues = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:MapDatabase"))
        {
            var db = AssetDatabase.LoadAssetAtPath<MapDatabase>(AssetDatabase.GUIDToAssetPath(guid));
            var ids = new HashSet<string>();

            foreach (var r in db.rooms)
            {
                if (r == null) { Warn(db, "有空的房間項目"); issues++; continue; }
                if (string.IsNullOrEmpty(r.roomId)) { Warn(r, "未填 roomId"); issues++; }
                else if (!ids.Add(r.roomId)) { Warn(r, $"roomId 重複：{r.roomId}"); issues++; }

                if (r.mapSprite == null) { Warn(r, "未指定 mapSprite"); issues++; }
                else if (r.mapRect.height > 0f)
                {
                    float sp = r.mapSprite.rect.width / r.mapSprite.rect.height;
                    float mp = r.mapRect.width / r.mapRect.height;
                    if (Mathf.Abs(sp - mp) > 0.01f)
                    { Warn(r, $"圖片比例 {sp:F3} 與 mapRect 比例 {mp:F3} 不一致，會變形"); issues++; }
                }

                if (r.mapRect.width <= 0f || r.mapRect.height <= 0f) { Warn(r, "mapRect 尺寸為 0"); issues++; }
                if (r.zoneRects == null || r.zoneRects.Length == 0) { Warn(r, "尚未烘焙 zoneRects（迷霧不會出現）"); issues++; }

                if (r.markers != null)
                {
                    foreach (var mk in r.markers)
                    {
                        if (mk.type == null) { Warn(r, $"地標 {mk.markerId} 未指定種類"); issues++; continue; }
                        if (mk.type.icon == null) { Warn(mk.type, "地標種類未指定 icon"); issues++; }
                        if (mk.reveal == MarkerReveal.WhenZoneRevealed && string.IsNullOrEmpty(mk.zoneId))
                        { Warn(r, $"地標 {mk.markerId} 不在任何 zone 內，不會因探索而顯示"); issues++; }
                    }
                }
            }
        }
        Debug.Log(issues == 0 ? "[Map] 驗證完成，沒有問題" : $"[Map] 驗證完成，共 {issues} 個問題");
    }

    static void Warn(Object ctx, string msg) => Debug.LogWarning($"[Map] {ctx.name}: {msg}", ctx);
}
