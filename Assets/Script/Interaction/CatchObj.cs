using UnityEngine;

public class CatchObj : MonoBehaviour
{
    public Interaction interaction;
    public GameObject interactionObj;
    public GameObject appearGameOject;
    public Light pointlight;
    public ParticleSystem catchEffect;
    public Material actMaterial;
    public float effectSpeed;
    public float effectSpeedOut;

    private float timer;
    private float maxLight;
    private float minLight;
    private int inEffect;
    private CameraController cameraController;
    private Material oriMaterial;
    private Color emissionColor;
    private Color emissionColorAdd;

    void Start()
    {
        maxLight = 180f;
        minLight = 0f;
        timer = 0f;
        inEffect = 0;
        cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        emissionColor = actMaterial.GetColor("_EmissionColor");
        emissionColorAdd = emissionColor;
    }

    void Update()
    {
        if (interaction.canInteract && inEffect == 0)
        {
            inEffect = 1;
            GetComponent<SpriteRenderer>().material = actMaterial;
            Time.timeScale = 0.5f;
            cameraController.cameraStatus = 1;
        }
        if (inEffect == 5)
        {
            appearGameOject.GetComponent<SpriteRenderer>().material = oriMaterial;
            interactionObj.SetActive(false);
            actMaterial.SetColor("_EmissionColor", emissionColor);
        }

        if(inEffect == 1)
        {
            timer += Time.deltaTime;
            emissionColorAdd *= 1.05f;
            actMaterial.SetColor("_EmissionColor", emissionColorAdd);
            if(timer > 0.2f)
            {
                inEffect = 2;
                timer = 0f;
                Time.timeScale = 1f;
            }
        }
        else if(inEffect == 2)
        {
            timer+=Time.deltaTime;
            pointlight.intensity += Time.deltaTime*effectSpeed;
            if(pointlight.intensity >= maxLight){
                pointlight.intensity = maxLight;
                inEffect = 3;
                timer = 0f;
                appearGameOject.SetActive(true);
                oriMaterial = appearGameOject.GetComponent<SpriteRenderer>().material;
                appearGameOject.GetComponent<SpriteRenderer>().material = actMaterial;
            }
        }
        else if(inEffect == 3)
        {
            timer += Time.deltaTime;
            if(timer > 0.3f)
            {
                inEffect = 4;
                timer = 0f;
            }
        }
        else if(inEffect == 4)
        {
            timer += Time.deltaTime;
            pointlight.intensity -= Time.deltaTime*effectSpeedOut;

            
            emissionColorAdd *= 0.95f;
            actMaterial.SetColor("_EmissionColor", emissionColorAdd);

            if(pointlight.intensity <= minLight){
                pointlight.intensity = minLight;
                inEffect = 5;
                timer = 0f;
                catchEffect.Play();
            }
        }
    }
}
