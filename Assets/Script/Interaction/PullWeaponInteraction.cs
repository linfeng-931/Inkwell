using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
public class PullWeaponInteraction : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 2f;

    [SerializeField] private TimelineSequenceController[] pullSequences;

    [Header("Feedback")]
    [SerializeField] private WeaponPullFeedback feedback;
    [SerializeField] private WeaponSwapFade swapFade;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem sparkParticle;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip flameClip;
    [SerializeField] private float pressFeedbackCooldown = 0.12f;

    private Transform playerTrans;
    private PlayerController player;

    private bool isPhaseActive;
    private bool isPlayerInRange;
    private bool isBusy;
    private bool isCompleted;
    private int pullIndex;
    private bool unlockMovement = false;

    private int busyStartFrame = -1;
    private float lastFeedbackTime = -100f;

    private void Awake()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Start()
    {
        GameObject playerObj = MapManager.Instance.player.gameObject;
        playerTrans = playerObj.transform;
        player = playerObj.GetComponent<PlayerController>();
    }

    private void OnDisable()
    {
        GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
        PlayerLock.UnlockAll(this);
    }

    // Player start pick up weapon
    public void BeginPullPhase()
    {
        if (isPhaseActive || isCompleted) return;
        isPhaseActive = true;

        // Lock Player Movement
        PlayerLock.Lock(this);
        if (player != null) player.currentMoveX = 0f;
    }

    private void Update()
    {
        TryReleaseMovementLock();
        HandleBusyPressFeedback();
        UpdateRangeDetection();
    }

    private void TryReleaseMovementLock()
    {
        if (unlockMovement || swapFade == null) return;
        if (swapFade.HasFadePlayed()) ReleaseMovementLock();
    }

    private void ReleaseMovementLock()
    {
        if (unlockMovement) return;
        unlockMovement = true;
        PlayerLock.Unlock(this);
    }

    private void HandleBusyPressFeedback()
    {
        if (!isBusy || pullIndex == 2) return;
        if (Time.frameCount <= busyStartFrame) return;
        if (!Input.GetKeyDown(KeyCode.F)) return;
        if (Time.time - lastFeedbackTime < pressFeedbackCooldown) return;

        lastFeedbackTime = Time.time;

        if (sparkParticle != null)
        {
            sparkParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            sparkParticle.Play(true);
        }

        if (audioManager != null && flameClip != null)
        {
            audioManager.PlayUISFX(flameClip);
        }
    }

    private void UpdateRangeDetection()
    {
        if (!isPhaseActive || isCompleted || isBusy) return;

        bool inRange = Vector3.Distance(playerTrans.position, transform.position) < detectionRange;
        if (inRange == isPlayerInRange) return;

        SetInRange(inRange);
    }

    // Player in pick up weapon range
    private void SetInRange(bool inRange)
    {
        isPlayerInRange = inRange;

        GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
        if (inRange) GameEvent.OnWorldInteractPressed += HandleInteractPressed;
    }

    private void OnDestroy()
    {
        GameEvent.OnWorldInteractPressed -= HandleInteractPressed;
    }

    private void HandleInteractPressed()
    {
        if (!isPhaseActive || !isPlayerInRange || isBusy || isCompleted) return;
        if (pullIndex >= pullSequences.Length) return;

        isBusy = true;
        busyStartFrame = Time.frameCount;
        SetInRange(false);

        TimelineSequenceController sequence = pullSequences[pullIndex];

        player.FaceTowards(transform.position);
        if (feedback != null) feedback.SetStage(pullIndex);

        sequence.director.stopped += OnPullFinished;
        sequence.TriggerTimeline();
    }

    private void OnPullFinished(PlayableDirector director)
    {
        director.stopped -= OnPullFinished;
        StartCoroutine(FinishPullRoutine());
    }

    private IEnumerator FinishPullRoutine()
    {
        yield return null;

        pullIndex++;
        isBusy = false;

        if (pullIndex >= pullSequences.Length)
        {
            isCompleted = true;
            isPhaseActive = false;

            if (swapFade == null || !swapFade.HasFadePlayed())
            {
                ReleaseMovementLock();
            }
        }
    }
}