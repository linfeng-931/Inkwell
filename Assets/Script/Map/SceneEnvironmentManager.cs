using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class SceneEnvironmentManager : MonoBehaviour
{
    public Material sceneSkybox;

    public bool enableFog = true;
    public Color fogColor = Color.gray;
    public float fogStart = 15f; 
    public float fogEnd = 40f;   

    private void OnEnable()
    {
        if (!Application.isPlaying) ApplySettings();
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        if (next == gameObject.scene) 
        {
            ApplySettings();
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) ApplySettings();
        };
    }
    #endif

    private void ApplySettings()
    {
        RenderSettings.skybox = sceneSkybox;

        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;
    }
}