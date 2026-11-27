using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 0.5f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    public PlayerController playerController;
    // public event Action OnDeath;

    [Header("Visual Effects")]
    public DamageGlitchEffect damageGlitchEffect;
    public Animator hurtEffectAnimator;
    public ParticleSystem hurtParticleEffect;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        //init player health
        GameEvent.OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void OnEnable()
    {
        GameEvent.OnGameLoaded += Load;
        GameEvent.OnGameSaved += Save;
        GameEvent.OnPlayerSave += HandlePlayerSave;
    }

    void OnDestroy()
    {
        GameEvent.OnGameLoaded -= Load;
        GameEvent.OnGameSaved -= Save;
        GameEvent.OnPlayerSave -= HandlePlayerSave;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if(invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }

    /// <summary>
    /// handle player hurt logic (such as blood and force)
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="sourcePosition">enemy position</param>
    public void TakeDamage(int damage, Vector3 sourcePosition)
    {
        if (playerController.currentState is DeathState) return;
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);

        PlayHurtFeedback(includeGlitch: false);

        //CombatFeedbackManager.Instance.TriggerHitFeedback(0.06f, 1.5f);

        //hurtEffectAnimator.Play("PlayerHurtEffect", -1, 0f);
        //hurtParticleEffect.Play();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            playerController.FaceTowards(sourcePosition);
            playerController.TransitionToState<HurtState>();
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            //damageGlitchEffect.TakeDamage();

            if (damageGlitchEffect != null) damageGlitchEffect.TakeDamage();
        }
    }

    private void Die()
    {
        playerController.TransitionToState<DeathState>();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        playerController.playerEnergy.RefillEnergy();
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);
    }

    private void PlayHurtFeedback(bool includeGlitch)
    {
        if (CombatFeedbackManager.Instance != null)
        {
            CombatFeedbackManager.Instance.TriggerHitFeedback(0.06f, 1.5f);
        }

        if (hurtEffectAnimator != null)
        {
            hurtEffectAnimator.Play("PlayerHurtEffect", -1, 0f);
        }

        if (hurtParticleEffect != null)
        {
            hurtParticleEffect.Play();
        }

        if (includeGlitch && damageGlitchEffect != null)
        {
            damageGlitchEffect.TakeDamage();
        }
    }

    /// <summary>
    /// reset player health
    /// </summary>
    /// <param name="waitTime">suit to fade ani or other need</param>
    /// <returns></returns>
    //private IEnumerator ResetHealth(float waitTime) {
    //    yield return new WaitForSeconds(waitTime);
    //    currentHealth = maxHealth;
    //    playerController.playerEnergy.RefillEnergy();
    //    GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);
    //}

    /// <summary>
    /// Hurt and respawn at specific point, handle health and enter hurt state
    /// </summary>
    /// <param name="hazardRespawnPos"></param>
    /// <param name="hazardDamage"></param>
    public bool RespawnFromHazard(Vector3 hazardRespawnPos, int hazardDamage = 1)
    {
        if (playerController.currentState is DeathState) return false;


        currentHealth = Mathf.Clamp(currentHealth - hazardDamage, 0, maxHealth);
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);

        PlayHurtFeedback(includeGlitch: true);

        // CombatFeedbackManager.Instance.TriggerHitFeedback(0.06f, 1.5f);

        //if (hurtEffectAnimator != null)
        //{
        //    hurtEffectAnimator.Play("PlayerHurtEffect", -1, 0f);
        //}

        //hurtParticleEffect.Play();
        //damageGlitchEffect.TakeDamage();

        if (currentHealth <= 0)
        {
            Die();
            return false;
        }

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        playerController.hazardRespawnPosition = hazardRespawnPos;
        playerController.isHazardHurt = true;
        playerController.TransitionToState<HurtState>();
        return true;
    }

    private void Load()
    {
        var data = SaveManager.Instance.CurrentSave.player;
        this.currentHealth = data.health;
        this.maxHealth = data.maxHealth;
    }

    public void Save()
    {
        var data = SaveManager.Instance.CurrentSave;

        if(data.player == null)
        {
            data.player = new PlayerStateData();
        }

        data.player.health = this.currentHealth;
        data.player.maxHealth = this.maxHealth;
    }

    private void HandlePlayerSave(Transform trans)
    {
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);
    }
}
