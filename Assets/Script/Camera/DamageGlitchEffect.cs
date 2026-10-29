using UnityEngine;

public class DamageGlitchEffect : MonoBehaviour
{
    public Material glitchMaterial;
    
    [Range(0f, 1f)]
    public float maxIntensity = 0.02f;
    public float recoverSpeed = 5f;
    
    private float currentIntensity = 0f;

    private readonly int redOffsetID = Shader.PropertyToID("_redOffset");
    private readonly int greenOffsetID = Shader.PropertyToID("_greenOffset");

    void Update()
    {
        if (currentIntensity > 0.00001f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, 0, Time.deltaTime * recoverSpeed);
            
            Vector3 rOffset = new Vector3(currentIntensity, -currentIntensity, 0);
            Vector3 gOffset = new Vector3(-currentIntensity, currentIntensity, 0);

            glitchMaterial.SetVector(redOffsetID, rOffset);
            glitchMaterial.SetVector(greenOffsetID, gOffset);
        }
        else if (currentIntensity != 0)
        {
            currentIntensity = 0;
            glitchMaterial.SetVector(redOffsetID, Vector3.zero);
            glitchMaterial.SetVector(greenOffsetID, Vector3.zero);
        }
    }

    public void TakeDamage()
    {
        currentIntensity = maxIntensity;
    }
}