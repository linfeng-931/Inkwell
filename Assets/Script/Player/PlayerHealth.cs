using System;
using UnityEngine;
using System.Collections;

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

    private PlayerController playerController;
    public event Action OnDeath;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        currentHealth = maxHealth;

        //init player health
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);
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
        if(isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);

        CombatFeedbackManager.Instance.TriggerHitFeedback(0.06f, 1.5f);

        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            playerController.FaceTowards(sourcePosition);
            playerController.TransitionToState<HurtState>();
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    private void Die()
    {
        playerController.TransitionToState<DeathState>();

        StartCoroutine(ResetHealth(1.5f));
    }

    /// <summary>
    /// reset player health
    /// </summary>
    /// <param name="waitTime">suit to fade ani or other need</param>
    /// <returns></returns>
    private IEnumerator ResetHealth(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        currentHealth = maxHealth;
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Hurt and respawn at specific point, handle health and enter hurt state
    /// </summary>
    /// <param name="hazardRespawnPos"></param>
    /// <param name="hazardDamage"></param>
    public void RespawnFromHazard(Vector3 hazardRespawnPos, int hazardDamage = 1) {
        // Update player info
        currentHealth = Mathf.Clamp(currentHealth - hazardDamage, 0, maxHealth);
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);

        // Is player die or not
        if (currentHealth <= 0) {
            Die();
            return;
        }

        // Start invincibility frames
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Respawn
        playerController.hazardRespawnPosition = hazardRespawnPos;
        playerController.isHazardHurt = true;

        playerController.TransitionToState<HurtState>();
    }
}
