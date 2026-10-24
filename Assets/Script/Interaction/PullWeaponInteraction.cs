using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
public class PullWeaponInteraction : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 2f;

    [Header("Pull Sequences")]
    [SerializeField] private TimelineSequenceController[] pullSequences;

    [Header("Feedback")]
    [SerializeField] private WeaponPullFeedback feedback;
    [SerializeField] private WeaponSwapFade swapFade;

    private Transform playerTrans;
    private PlayerController player;

    private bool isPhaseActive;
    private bool isPlayerInRange;
    private bool isBusy;
    private bool isCompleted;
    private int pullIndex;
    private bool unlockMovement = false;

    private void Start()
    {
        GameObject playerObj = MapManager.Instance.player.gameObject;
        playerTrans = playerObj.transform;
        player = playerObj.GetComponent<PlayerController>();
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
        if (unlockMovement) return;

        if (swapFade.HasFadePlayed())
        {
            PlayerLock.Unlock(this);
            unlockMovement = true;
        }

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
        }
    }
}