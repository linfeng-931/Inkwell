using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 存檔場景、進入 Play Mode 前，自動烘焙場景內的 MapRoomAnchor（zone 與地標）。
/// 企劃移動地標物件後，只要存檔場景，地圖資料就會同步，不需要手動操作。
/// </summary>
[InitializeOnLoad]
public static class MapAutoBake
{
    static MapAutoBake()
    {
        EditorSceneManager.sceneSaving += OnSceneSaving;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnSceneSaving(Scene scene, string path)
    {
        if (BakeScene(scene)) AssetDatabase.SaveAssets();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;
        for (int i = 0; i < SceneManager.sceneCount; i++) BakeScene(SceneManager.GetSceneAt(i));
    }

    static bool BakeScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return false;

        bool baked = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var a in root.GetComponentsInChildren<MapRoomAnchor>(true))
            {
                if (!a.autoBake || a.data == null || a.IsBakeUpToDate()) continue;
                a.Bake();
                baked = true;
            }
        }
        return baked;
    }
}
