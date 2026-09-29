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
        GameEvent.OnCutsceneStateChanged += HandleCutsceneState;
    }

    /// <summary>
    /// destory obj
    /// </summary>
    private void OnDisable()
    {
        GameEvent.OnHealthChanged -= healthUI.UpdateVisuals;
        GameEvent.OnEnergyChanged -= energyUI.UpdateVisuals;
        GameEvent.OnCutsceneStateChanged -= HandleCutsceneState;
    }

    private void HandleCutsceneState(bool isEnterCutscene)
    {
        if (isEnterCutscene)
        {
            OnExit();
        }
        else
        {
            OnEnter();
        }
    }

    public override void OnEnter()
    {
        base.OnEnter();
        canvasGroup.blocksRaycasts = false;
    }

    public override void OnPause()
    {
        base.OnPause();
        canvasGroup.alpha = 0f;
    }
    
    public override void OnExit()
    {
        base.OnPause();
        canvasGroup.alpha = 1f;
    }
}   
