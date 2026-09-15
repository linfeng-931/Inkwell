using UnityEngine;

public class EnergyUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image energyBar;

    [Header("Color Setting")]
    [SerializeField] private Color midEnergyColor;
    [SerializeField] private Color lowEnergyColor;
    [SerializeField] private float midEnergy = 50f;
    [SerializeField] private float lowEnergy = 20f;

    [Header("Animation Setting")]
    [SerializeField] private float slideSpeed = 5f;

    private Color originalColor;
    private float targetFillAmount = 1f; // for length
    private float maxEnergyValue = 100f; // for color

    void Awake()
    {
        originalColor = energyBar.color;
        targetFillAmount = energyBar.fillAmount;
    }

    void Update()
    {
        // slide animation
        if (Mathf.Abs(energyBar.fillAmount - targetFillAmount) > 0.001f)
        {
            energyBar.fillAmount = Mathf.Lerp(energyBar.fillAmount, targetFillAmount, Time.deltaTime * slideSpeed);
            UpdateColor(energyBar.fillAmount * maxEnergyValue);
        }
    }

    /// <summary>
    /// update value about energy
    /// </summary>
    /// <param name="current"></param>
    /// <param name="max"></param>
    public void UpdateVisuals(float current, float max)
    {
        maxEnergyValue = max;
        targetFillAmount = current / max;
    }

    /// <summary>
    /// change energy bar color
    /// </summary>
    /// <param name="currentVisualValue"></param>
    private void UpdateColor(float currentVisualValue)
    {
        if (currentVisualValue <= lowEnergy)
        {
            energyBar.color = lowEnergyColor;
        }
        else if (currentVisualValue <= midEnergy)
        {
            energyBar.color = midEnergyColor;
        }
        else
        {
            energyBar.color = originalColor;
        }
    }
}
