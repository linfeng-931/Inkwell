using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class DashColorAdvanced : MonoBehaviour
{
    public ParticleSystem[] particleSystems;
    public Light mainDirectionalLight;

    [Header("Color Setting")]
    public Color lightAreaColor = Color.white;
    public Color darkAreaColor = Color.black;

    [Header("Judgment Parameters")]
    [Range(0f, 1f)] public float brightnessThreshold = 0.4f;
    public bool useFallbackToGlobal = true;

    void Update()
    {
        if (particleSystems == null || particleSystems.Length == 0) return;

        Color targetRGB = GetProcessedColor();

        foreach (var ps in particleSystems)
        {
            if (ps == null) continue;

            // Color over Lifetime (Gradient)
            var col = ps.colorOverLifetime;
            if (col.enabled)
            {
                if (col.color.mode == ParticleSystemGradientMode.Gradient)
                {
                    Gradient currentGradient = col.color.gradient;
                    GradientColorKey[] colorKeys = currentGradient.colorKeys;
                    GradientAlphaKey[] alphaKeys = currentGradient.alphaKeys;

                    for (int i = 0; i < colorKeys.Length; i++)
                    {
                        colorKeys[i].color = new Color(targetRGB.r, targetRGB.g, targetRGB.b);
                    }

                    Gradient newGradient = new Gradient();
                    newGradient.SetKeys(colorKeys, alphaKeys);
                    col.color = new ParticleSystem.MinMaxGradient(newGradient);
                }
            }

            //Start Color (maintain original Alpha)
            var main = ps.main;
            
            if (main.startColor.mode == ParticleSystemGradientMode.Color)
            {
                float originalAlpha = main.startColor.color.a;
                main.startColor = new Color(targetRGB.r, targetRGB.g, targetRGB.b, originalAlpha);
            }
            else if (main.startColor.mode == ParticleSystemGradientMode.TwoColors)
            {
                float alphaMin = main.startColor.colorMin.a;
                float alphaMax = main.startColor.colorMax.a;
                
                Color cMin = new Color(targetRGB.r, targetRGB.g, targetRGB.b, alphaMin);
                Color cMax = new Color(targetRGB.r, targetRGB.g, targetRGB.b, alphaMax);
                
                main.startColor = new ParticleSystem.MinMaxGradient(cMin, cMax);
            }
        }
    }

    Color GetProcessedColor()
    {
        float brightness = 0;

        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe(transform.position, null, out sh);
        
        Color[] results = new Color[1];
        Vector3[] dirs = { Vector3.up };
        sh.Evaluate(dirs, results);
        float localBrightness = results[0].grayscale;

        if (localBrightness < 0.01f && useFallbackToGlobal)
        {
            float ambient = RenderSettings.ambientLight.grayscale;
            float direct = (mainDirectionalLight != null && mainDirectionalLight.enabled) 
                           ? Mathf.Clamp01(mainDirectionalLight.intensity / 2f) : 0f;
            brightness = (ambient * 0.4f) + (direct * 0.6f);
        }
        else
        {
            brightness = localBrightness;
        }

        return (brightness >= brightnessThreshold) ? lightAreaColor : darkAreaColor;
    }
}