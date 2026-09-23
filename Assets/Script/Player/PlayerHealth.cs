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

    [Header("Respawn")]
    public Vector3 lastCheckpointPosition;

    [Header("Animation")]
    public Animator fadeAni;
    public float fadeOutWaitTime = 0.5f;

    private PlayerController playerController;
    public event Action OnDeath;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        currentHealth = maxHealth;
        lastCheckpointPosition = transform.position;
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

    public void UpdateCheckpoint(Vector3 newPoint)
    {
        lastCheckpointPosition = newPoint;
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
        OnDeath?.Invoke();
        playerController.animator.Play(PlayerAnimateHash.Dead, 0, 0f);
        StartCoroutine(RespawnRoutine());
    }

    // Die and respawn at save point
    private IEnumerator RespawnRoutine() {
        // Disabled player input
        playerController.isPlayerInputEnabled = false;
        playerController.rig.linearVelocity = Vector3.zero;



        // Play Animation
        if (fadeAni != null)
        {
            fadeAni.SetTrigger("changeScene");
        }

        if (fadeOutWaitTime > 0f)
        {
            yield return new WaitForSeconds(fadeOutWaitTime);
        }

        currentHealth = maxHealth;
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);

        if (fadeAni != null)
        {
            fadeAni.SetTrigger("returnStart");
        }

        yield return new WaitForSeconds(0.2f);

        // Reset Player Data
        transform.position = lastCheckpointPosition;
        playerController.rig.position = lastCheckpointPosition;
        playerController.rig.linearVelocity = Vector3.zero;

        // Recover player input
        playerController.isPlayerInputEnabled = true;
        playerController.TransitionToState<IdleState>();
    }

    // Hurt and respawn at specific point
    public IEnumerator RespawnFromHazard(Vector3 hazardRespawnPos, int hazardDamage = 1) {
        // Update player info
        currentHealth -= hazardDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        GameEvent.OnHealthChanged.Invoke(currentHealth, maxHealth);

        // Is player die or not
        if (currentHealth <= 0) {
            Die();
            yield break;
        }

        // Respawn
        playerController.rig.linearVelocity = Vector3.zero;
        playerController.rig.position = hazardRespawnPos;
        transform.position = hazardRespawnPos;

        // Start invincibility frames
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        playerController.TransitionToState<IdleState>();
    }
}
