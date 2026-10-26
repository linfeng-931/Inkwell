using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class StartTutorial : MonoBehaviour
{
    [Header("Player Setting")]
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform hookSpawnPos;
    [SerializeField] private TextEffect hintCanvas;

    // Movement Tutorial
    private UnityEvent onAllMovementFinished;
    private UnityEvent onAllShiftFinished;

    // Enemy Tutorial
    [Header("Enemy Setting")]
    [SerializeField] private GameObject[] silverFishs;
    public PlayableDirector enemySpawnDirector;
    private UnityEvent onAllEnemiesDefeated;

    // Hook Tutorial
    [Header("Hook Setting")]
    [SerializeField] private GameObject[] platforms;
    [SerializeField] private Collider endPlatform;
    [SerializeField] private GameObject[] hookTarget;
    [SerializeField] private GameObject lightParticle;

    private readonly HashSet<Silverfish> aliveEnemies = new HashSet<Silverfish>();
    private bool hasReachedDestination = false;


    public void Tutorial()
    {
        StartCoroutine(TutorialFlowRoutine());
    }

    private IEnumerator TutorialFlowRoutine()
    {
        // Movement and Jump
        yield return StartCoroutine(TransitionSequence(playerSpawn.position, "A D鍵進行移動, 空白鍵進行跳躍"));

        bool pressedA = false;
        bool pressedD = false;
        bool pressedSpace = false;

        while (!pressedA || !pressedD || !pressedSpace)
        {
            if (Input.GetKeyDown(KeyCode.A)) pressedA = true;
            if (Input.GetKeyDown(KeyCode.D)) pressedD = true;
            if (Input.GetKeyDown(KeyCode.Space)) pressedSpace = true;
            yield return null;
        }
        onAllMovementFinished?.Invoke();

        yield return new WaitForSeconds(1.5f);

        // Shift
        StartCoroutine(ShowCanvas("按下 Shift 來化作墨水穿梭地形"));

        bool pressedShift = false;

        while (!pressedShift)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) pressedShift = true;
            yield return null;
        }

        hintCanvas.StopBlinking();
        onAllShiftFinished?.Invoke();

         yield return new WaitForSeconds(1.5f);

        // Attack
        yield return StartCoroutine(TransitionSequence(playerSpawn.position, "按下左鍵進行攻擊"));
        SpawnEnemies();
        yield return new WaitUntil(() => aliveEnemies.Count == 0);
        onAllEnemiesDefeated?.Invoke();
        yield return new WaitForSeconds(1f);

        // Hook
        yield return StartCoroutine(TransitionSequence(hookSpawnPos.position, "按下右鍵發射勾鎖"));
        SpawnHookTutorial();
        yield return new WaitUntil(() => hasReachedDestination);

        Debug.Log("教學全數完成！");
    }

    private IEnumerator TransitionSequence(Vector3 targetPosition, string hintText)
    {
        GameEvent.OnToggleFade(false);
        PlayerLock.Lock(this);
        PlayerLock.Lock(this, PlayerLockType.Combat);
        yield return new WaitForSeconds(1f);

        GameEvent.OnToggleFade(true);

        Transform playerTransform = MapManager.Instance.player.transform;
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TeleportTo(targetPosition);
        }

        PlayerLock.Unlock(this);
        PlayerLock.Unlock(this, PlayerLockType.Combat);

        StartCoroutine(ShowCanvas(hintText));

        yield return new WaitForSeconds(2f);
        hintCanvas.StopBlinking();
    }

    private IEnumerator ShowCanvas(string text)
    {
        hintCanvas.StartBlinking();
        hintCanvas.SetText(text);
        yield return new WaitForSeconds(4f);
    }

    private void SpawnEnemies()
    {
        foreach (GameObject enemy in silverFishs)
        {
            if (enemy == null) continue;
            enemy.SetActive(true);

            Silverfish fish = enemy.GetComponent<Silverfish>();
            aliveEnemies.Add(fish);
            fish.OnDied += HandleEnemyDied;
        }

        if (enemySpawnDirector != null) enemySpawnDirector.Play();
    }

    private void HandleEnemyDied(Silverfish fish)
    {
        fish.OnDied -= HandleEnemyDied;
        aliveEnemies.Remove(fish);
    }

    private void SpawnHookTutorial()
    {
        foreach (var platform in platforms)
        {
            platform.SetActive(true);
        }

        foreach (var target in hookTarget)
        {
            target.SetActive(true);
        }

        lightParticle.SetActive(true);
    }

    public void ReachDestination()
    {
        hasReachedDestination = true;
        lightParticle.SetActive(false);
    }
}