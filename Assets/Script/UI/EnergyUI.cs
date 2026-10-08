using UnityEngine;

public class EnergyUI : MonoBehaviour
{
    [Header("Energy Image Setting")]
    [SerializeField] private UnityEngine.UI.Image energyBar;
    [SerializeField] private Sprite energyBarNormal;
    [SerializeField] private Sprite energyBarY;
    [SerializeField] private Material emissionY;

    [Header("Color Setting")]
    [SerializeField] private Color midEnergyColor;
    [SerializeField] private Color lowEnergyColor;
    [SerializeField] private float midEnergy = 50f;
    [SerializeField] private float lowEnergy = 20f;
    [SerializeField] private float fullEnergyDuration = 0.5f;

    [Header("Animation Setting")]
    [SerializeField] private float slideSpeed = 5f;

    private Color originalColor;
    private float targetFillAmount = 1f; // for length
    private float maxEnergyValue = 100f; // for color
    private float fullEnergyTimer = 0f;
    private bool wasNotFull = false;

    void Awake()
    {
        originalColor = energyBar.color;
        targetFillAmount = energyBar.fillAmount;
        energyBar.sprite = energyBarNormal;
        energyBar.material = null;
    }

    void Update()
    {
        if (fullEnergyTimer > 0f)
        {
            fullEnergyTimer -= Time.deltaTime;
        }

        // slide animation
        if (Mathf.Abs(energyBar.fillAmount - targetFillAmount) > 0.001f)
        {
            energyBar.fillAmount = Mathf.Lerp(energyBar.fillAmount, targetFillAmount, Time.deltaTime * slideSpeed);
        }
        else
        {
            energyBar.fillAmount = targetFillAmount;
        }

        UpdateColor(energyBar.fillAmount * maxEnergyValue);
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

        if (current < max)
        {
            fullEnergyTimer = 0f;
            wasNotFull = true;
        }
        else if (current >= max)
        {
            if (wasNotFull)
            {
                fullEnergyTimer = fullEnergyDuration;
                wasNotFull = false;
            }
        }
    }

    /// <summary>
    /// change energy bar color
    /// </summary>
    /// <param name="currentVisualValue"></param>
    private void UpdateColor(float currentVisualValue)
    {
        // Show yellow energy bar when recover 100 percent energy
        if (fullEnergyTimer > 0f)
        {
            energyBar.sprite = energyBarY;
            energyBar.color = originalColor;
            energyBar.material = emissionY;
            return;
        }

        energyBar.sprite = energyBarNormal;
        energyBar.material = null;

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
