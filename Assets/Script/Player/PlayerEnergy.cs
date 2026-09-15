using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100;
    public float currentEnergy;

    [Header("Recovery Setting")]
    public float energyRecoveryRate = 15f; // pre second
    public float recoveryDelay = 2f;

    public bool canRecoverEnergy {get; set;} = false;
    private float lastConsumeTime = -99f;

    void Start()
    {
        currentEnergy = maxEnergy;

        //init player Energy
        GameEvent.OnEnergyChanged.Invoke(currentEnergy, maxEnergy);
    }

    void Update()
    {
        if(canRecoverEnergy && currentEnergy < maxEnergy && Time.time >= lastConsumeTime+recoveryDelay)
        {
            currentEnergy += energyRecoveryRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            
            GameEvent.OnEnergyChanged.Invoke(currentEnergy, maxEnergy);
        }
    }

    public bool ConsumeEnergy(float consumeEnergy)
    {
        if (currentEnergy < consumeEnergy) return false;

        currentEnergy -= consumeEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        lastConsumeTime = Time.time;
        
        GameEvent.OnEnergyChanged.Invoke(currentEnergy, maxEnergy);
        return true;
    }
}
