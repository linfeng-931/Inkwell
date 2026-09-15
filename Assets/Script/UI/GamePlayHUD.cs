using UnityEngine;

public class GamePlayHUD : BasePanel
{
    [Header("Sub UI References")]
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private EnergyUI energyUI;

    private void OnEnable()
    {
        // set ui event
        GameEvent.OnHealthChanged += healthUI.UpdateVisuals;
        GameEvent.OnEnergyChanged += energyUI.UpdateVisuals;
    }

    /// <summary>
    /// destory obj
    /// </summary>
    private void OnDisable()
    {
        GameEvent.OnHealthChanged -= healthUI.UpdateVisuals;
        GameEvent.OnEnergyChanged -= energyUI.UpdateVisuals;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        canvasGroup.blocksRaycasts = false;
    }
}
