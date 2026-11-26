using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(10000)]
[RequireComponent(typeof(Canvas))]
public class CameraFitWorldCanvas : MonoBehaviour
{
    public Camera targetCamera;
    public float distance = 10f; // to camera
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    private Canvas canvas;
    private RectTransform rect;

    void OnEnable()
    {
        canvas = GetComponent<Canvas>();
        rect = GetComponent<RectTransform>();
        canvas.renderMode = RenderMode.WorldSpace;
    }

    void LateUpdate()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null || cam.aspect < 0.0001f) return;

        if (canvas.worldCamera != cam) canvas.worldCamera = cam;

        // rotation
        rect.SetPositionAndRotation(cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distance)), cam.transform.rotation);

        // heigh
        float frustumH = cam.orthographic
            ? cam.orthographicSize * 2f
            : 2f * distance / cam.projectionMatrix[1, 1];

        // Canvas size (static height, auto width)
        Vector2 size = new Vector2(referenceResolution.y * cam.aspect, referenceResolution.y);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        // size
        float s = frustumH / size.y;
        Vector3 p = rect.parent != null ? rect.parent.lossyScale : Vector3.one;
        rect.localScale = new Vector3(s / p.x, s / p.y, s / p.z);
    }
}