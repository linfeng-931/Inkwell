using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(MapRoomAnchor))]
public class MapRoomAnchorEditor : Editor
{
    static readonly BoxBoundsHandle handle = new BoxBoundsHandle
    {
        axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
        handleColor = new Color(0.2f, 0.9f, 1f),
        wireframeColor = new Color(0.2f, 0.9f, 1f, 0.6f)
    };

    public override void OnInspectorGUI()
    {
        var a = (MapRoomAnchor)target;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck() && a.autoBake) a.Bake();

        EditorGUILayout.Space();
        if (a.data == null)
        {
            EditorGUILayout.HelpBox("尚未指定 MapRoomData", MessageType.Warning);
        }
        else if (!a.IsBakeUpToDate())
        {
            EditorGUILayout.HelpBox("Zone／地標資料尚未烘焙或已過期，迷霧與地標位置會對不上。（存檔場景時會自動烘焙）", MessageType.Warning);
            if (GUILayout.Button("烘焙 Zone 與地標")) a.Bake();
        }
        else
        {
            EditorGUILayout.HelpBox($"已烘焙：{a.data.zoneRects.Length} 個 zone、{(a.data.markers != null ? a.data.markers.Length : 0)} 個地標，與目前場景一致。", MessageType.Info);
        }
    }

    void OnSceneGUI()
    {
        var a = (MapRoomAnchor)target;
        if (a.zones == null) return;

        var s = a.transform.lossyScale;
        if (Mathf.Approximately(s.x, 0) || Mathf.Approximately(s.y, 0)) return;

        for (int i = 0; i < a.zones.Length; i++)
        {
            var r = a.ZoneToWorld(a.zones[i]);
            handle.center = r.center;
            handle.size = r.size;

            EditorGUI.BeginChangeCheck();
            handle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(a, "Edit Map Room Zone");
                var localCenter = (Vector2)handle.center - (Vector2)a.transform.position;
                a.zones[i].center = new Vector2(localCenter.x / s.x, localCenter.y / s.y);
                a.zones[i].size = new Vector2(handle.size.x / Mathf.Abs(s.x), handle.size.y / Mathf.Abs(s.y));
                if (a.autoBake) a.Bake();
            }

            Handles.Label(r.center, string.IsNullOrEmpty(a.zones[i].zoneId) ? "⚠ 無 zoneId" : a.zones[i].zoneId);
        }
    }
}
