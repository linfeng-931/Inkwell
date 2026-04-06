using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public int blood;
    public int maxBlood;
    public int energy;
    public float speedOfEnergyRaise;
    public bool isUsingEnergy;
    public float energyRangeX_max;
    public float energyRangeX_min;
    public Vector3 currentPlayerPos;
    public float damage;

    private GameObject blood_full;
    private GameObject inkbar_full;
    private GameObject inkbar_empty;
    private RectTransform inkbar_emptyTrans;
    private Transform playerTrans;
    private float energyRaiseTimer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blood_full = GameObject.FindWithTag("Blood").transform.GetChild(1).gameObject;
        inkbar_full = GameObject.FindWithTag("Inkbar").transform.GetChild(1).gameObject;
        inkbar_empty = GameObject.FindWithTag("Inkbar").transform.GetChild(0).gameObject;
        inkbar_emptyTrans = inkbar_full.GetComponent<RectTransform>();
        playerTrans = GameObject.FindWithTag("Player").transform;
        energyRaiseTimer = 0f;

        //test
        blood = 5;
        energy = 100;
    }

    // Update is called once per frame
    void Update()
    {
        inkbar_emptyTrans.position = new Vector3(energyRangeX_max-(energyRangeX_max-energyRangeX_min)*(1f-(energy/100f)), inkbar_emptyTrans.position.y, inkbar_emptyTrans.position.z);
        if (maxBlood != blood)
        {
            for(int i = maxBlood-1; i>=blood; i--)
            {
                blood_full.transform.GetChild(i).gameObject.SetActive(false);
            }
            for(int i = 0; i<blood; i++)
            {
                blood_full.transform.GetChild(i).gameObject.SetActive(true);
            }
        }  
        if(energy< 100) energyRaiseTimer+=Time.deltaTime;
    }
    public void RaiseEnegry(float lx)
    {
        if(energyRaiseTimer<0.1f) return;
        energyRaiseTimer = 0f;
        if(!isUsingEnergy && energy< 100)
        {
            energy += (int)(speedOfEnergyRaise*lx);
            if(energy>=100) energy = 100;
        }
    }
}
