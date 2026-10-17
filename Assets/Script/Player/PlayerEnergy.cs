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
        GameEvent.OnGameLoaded += Load;
        GameEvent.OnGameSaved += Save;
    }

    void OnDestroy()
    {
        GameEvent.OnGameLoaded -= Load;
        GameEvent.OnGameSaved -= Save;
    }

    void Update()
    {
        if(canRecoverEnergy && currentEnergy < maxEnergy && Time.time >= lastConsumeTime+recoveryDelay)
        {
            currentEnergy += energyRecoveryRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

            if (currentEnergy == maxEnergy) {
                // Play Sound Effect
                PlayerController manager = GetComponent<PlayerController>();
                manager.audioManager.PlaySFX(manager.fullOffEnergy);

                // Change Player Energy UI color
                EnergyUI energyUI = GetComponent<EnergyUI>();
            }
            
            GameEvent.OnEnergyChanged.Invoke(currentEnergy, maxEnergy);
        }
    }

    public void ConsumeEnergy(float consumeEnergy)
    {
        if (currentEnergy < consumeEnergy) return;

        currentEnergy -= consumeEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        lastConsumeTime = Time.time;
        
        GameEvent.OnEnergyChanged.Invoke(currentEnergy, maxEnergy);
    }

    private void Load()
    {
        var data = SaveManager.Instance.currentSave.player;
        this.currentEnergy = data.energy;
        this.maxEnergy = data.maxEnergy;
    }

    public void Save()
    {
        var data = SaveManager.Instance.currentSave.player;
        data.energy = this.currentEnergy;
        data.maxEnergy = this.maxEnergy;
    }
}
