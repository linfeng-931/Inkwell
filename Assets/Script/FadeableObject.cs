using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeableObject : MonoBehaviour
{
    public enum FadeMode
    {
        Alpha,
        Scale,
        AlphaAndScale
    }

    [SerializeField] private FadeMode mode = FadeMode.AlphaAndScale;
    [SerializeField] private Transform scaleTarget;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");
    private static readonly int AlphaClipId = Shader.PropertyToID("_AlphaClip");

    private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    private readonly List<Color> spriteColors = new List<Color>();

    private readonly List<Material> materials = new List<Material>();
    private readonly List<int> materialColorIds = new List<int>();
    private readonly List<Color> materialColors = new List<Color>();
    private readonly List<float> materialCutoffs = new List<float>();

    private Vector3 baseScale = Vector3.one;
    private bool cached;
    private float currentValue = 1f; // 0 = Hidden 1 = Show
    private Coroutine running;

    private void Cache()
    {
        if (cached) return;
        cached = true;

        if (scaleTarget == null) scaleTarget = transform;
        baseScale = scaleTarget.localScale;

        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer) continue;

            if (r is SpriteRenderer sr)
            {
                sprites.Add(sr);
                spriteColors.Add(sr.color);
                continue;
            }

            foreach (Material m in r.materials)
            {
                int id = m.HasProperty(BaseColorId) ? BaseColorId : (m.HasProperty(ColorId) ? ColorId : -1);
                if (id == -1) continue;

                bool alphaClip = m.HasProperty(AlphaClipId) && m.GetFloat(AlphaClipId) > 0.5f && m.HasProperty(CutoffId);

                materials.Add(m);
                materialColorIds.Add(id);
                materialColors.Add(m.GetColor(id));
                materialCutoffs.Add(alphaClip ? m.GetFloat(CutoffId) : -1f);
            }
        }
    }

    private void Apply(float t)
    {
        currentValue = t;

        if (mode != FadeMode.Alpha)
        {
            scaleTarget.localScale = baseScale * Mathf.Max(t, 0.0001f);
        }

        if (mode == FadeMode.Scale) return;

        for (int i = 0; i < sprites.Count; i++)
        {
            Color c = spriteColors[i];
            c.a *= t;
            sprites[i].color = c;
        }

        for (int i = 0; i < materials.Count; i++)
        {
            Color c = materialColors[i];
            c.a *= t;
            materials[i].SetColor(materialColorIds[i], c);
            if (materialCutoffs[i] >= 0f)
            {
                materials[i].SetFloat(CutoffId, materialCutoffs[i] * t);
            }
        }
    }

    public void FadeIn(float duration)
    {
        Cache();
        Apply(0f);
        gameObject.SetActive(true);
        FadeTo(1f, duration, false);
    }

    public void FadeOut(float duration)
    {
        FadeTo(0f, duration, true);
    }

    public void FadeTo(float target, float duration, bool deactivateWhenDone)
    {
        Cache();
        if (running != null) StopCoroutine(running);
        if (duration <= 0f || !gameObject.activeInHierarchy)
        {
            Apply(target);
            if (deactivateWhenDone && target <= 0f) gameObject.SetActive(false);
            return;
        }

        running = StartCoroutine(FadeRoutine(target, duration, deactivateWhenDone));
    }

    private IEnumerator FadeRoutine(float target, float duration, bool deactivateWhenDone)
    {
        float start = currentValue;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            Apply(Mathf.Lerp(start, target, k));
            yield return null;
        }

        Apply(target);
        running = null;

        if (deactivateWhenDone && target <= 0f) gameObject.SetActive(false);
    }
}