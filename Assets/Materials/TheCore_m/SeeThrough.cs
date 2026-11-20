
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

    // 儲存 Renderer 原本的材質
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();

    // 記錄目前哪些 Renderer 已經隱藏其他材質
    private readonly HashSet<Renderer> reducedRenderers = new();

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
            Renderer renderer =
                hit.transform.GetComponent<Renderer>();

            if (renderer == null)
                renderer = hit.transform.GetComponentInChildren<Renderer>();

            if (renderer == null)
                continue;

            // 儲存原始材質
            if (!originalMaterials.ContainsKey(renderer))
            {
                originalMaterials[renderer] = renderer.sharedMaterials;
            }

            // 只使用 Element 0
            if (!reducedRenderers.Contains(renderer))
            {
                Material[] mats = renderer.sharedMaterials;

                if (mats.Length > 1)
                {
                    renderer.sharedMaterials = new Material[]
                    {
                        mats[0]
                    };

                    reducedRenderers.Add(renderer);
                }
            }

            foreach (Material mat in originalMaterials[renderer])
            {
                if (mat == null || !mat.HasProperty(OpacityID))
                    continue;

                currentMaterials.Add(mat);
                targetOpacity[mat] = blockedOpacity;
            }
        }

        // 恢復沒有遮擋的材質
        List<Material> materials =
            new List<Material>(targetOpacity.Keys);

        foreach (var mat in materials)
        {
            if (mat != null && !currentMaterials.Contains(mat))
            {
                targetOpacity[mat] = normalOpacity;
            }
        }

        removeMaterials.Clear();

        foreach (var item in targetOpacity)
        {
            Material mat = item.Key;

            if (mat == null || !mat.HasProperty(OpacityID))
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

        // 找出已經不再被遮擋的 Renderer
        List<Renderer> restoreList = new();

        foreach (Renderer renderer in reducedRenderers)
        {
            if (renderer == null)
            {
                restoreList.Add(renderer);
                continue;
            }

            bool stillBlocked = false;

            foreach (Material mat in originalMaterials[renderer])
            {
                if (mat != null &&
                    targetOpacity.TryGetValue(mat, out float opacity) &&
                    opacity == blockedOpacity)
                {
                    stillBlocked = true;
                    break;
                }
            }

            if (!stillBlocked)
                restoreList.Add(renderer);
        }

        // 恢復原本所有 Material
        foreach (Renderer renderer in restoreList)
        {
            if (renderer != null &&
                originalMaterials.TryGetValue(renderer, out Material[] mats))
            {
                renderer.sharedMaterials = mats;
            }

            reducedRenderers.Remove(renderer);
        }
    }

    private void OnDisable()
    {
        foreach (var item in targetOpacity)
        {
            if (item.Key != null &&
                item.Key.HasProperty(OpacityID))
            {
                item.Key.SetFloat(OpacityID, normalOpacity);
            }
        }

        // 恢復所有 Renderer 的材質
        foreach (var item in originalMaterials)
        {
            if (item.Key != null)
            {
                item.Key.sharedMaterials = item.Value;
            }
        }

        targetOpacity.Clear();
        originalMaterials.Clear();
        reducedRenderers.Clear();
    }
}
