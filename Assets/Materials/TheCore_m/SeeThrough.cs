
using System.Collections.Generic;
using UnityEngine;

public class SeeThrough : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform targetObject;

    [Header("Detection")]
    [SerializeField]
    private LayerMask wallMask;

    [Header("Opacity")]
    [SerializeField]
    private float blockedOpacity = -1f;

    [SerializeField]
    private float normalOpacity = 1f;

    [SerializeField]
    private float fadeSpeed = 3f;

    private Camera mainCamera;

    private readonly Dictionary<Material, float> targetOpacity = new();

    private readonly HashSet<Material> currentMaterials = new();

    private readonly List<Material> removeMaterials = new();

    private static readonly int OpacityID =
        Shader.PropertyToID("_Opacity");

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null || targetObject == null)
            return;

        currentMaterials.Clear();

        Vector3 direction =
            targetObject.position - mainCamera.transform.position;

        float distance = direction.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(
            mainCamera.transform.position,
            direction.normalized,
            distance,
            wallMask
        );

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.transform.GetComponent<Renderer>();

            if (renderer == null)
                renderer = hit.transform.GetComponentInChildren<Renderer>();

            if (renderer == null)
                continue;

            foreach (Material mat in renderer.materials)
            {
                if (mat == null || !mat.HasProperty(OpacityID))
                    continue;

                currentMaterials.Add(mat);
                targetOpacity[mat] = blockedOpacity;
            }
        }

        // 沒有再遮擋的材質，目標恢復為 1
        List<Material> materials = new List<Material>(targetOpacity.Keys);

        foreach (var mat in materials)
        {
            if (mat != null && !currentMaterials.Contains(mat))
            {
                targetOpacity[mat] = normalOpacity;
            }
        }

        // 平滑改變 Opacity
        removeMaterials.Clear();

        foreach (var item in targetOpacity)
        {
            Material mat = item.Key;

            if (mat == null)
            {
                removeMaterials.Add(mat);
                continue;
            }

            if (!mat.HasProperty(OpacityID))
            {
                removeMaterials.Add(mat);
                continue;
            }

            float currentOpacity = mat.GetFloat(OpacityID);

            float newOpacity = Mathf.MoveTowards(
                currentOpacity,
                item.Value,
                fadeSpeed * Time.deltaTime
            );

            mat.SetFloat(OpacityID, newOpacity);

            // 已恢復正常，從管理清單移除
            if (item.Value == normalOpacity &&
                Mathf.Approximately(newOpacity, normalOpacity))
            {
                removeMaterials.Add(mat);
            }
        }

        foreach (Material mat in removeMaterials)
        {
            targetOpacity.Remove(mat);
        }
    }

    private void OnDisable()
    {
        foreach (var item in targetOpacity)
        {
            if (item.Key != null && item.Key.HasProperty(OpacityID))
                item.Key.SetFloat(OpacityID, normalOpacity);
        }

        targetOpacity.Clear();
    }
}
