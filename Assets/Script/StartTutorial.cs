using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using DG.Tweening;
using Unity.Cinemachine;

public class StartTutorial : MonoBehaviour
{
    [Header("Player Setting")]
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform hookSpawnPos;
    [SerializeField] private Transform dialouguePos;
    [SerializeField] private TextEffect hintCanvas;

    // Movement Tutorial
    private UnityEvent onAllMovementFinished;
    private UnityEvent onAllShiftFinished;
    private UnityEvent onEnergyFilled;

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
    [SerializeField] private GameObject[] vines;
    [SerializeField] private GameObject lightParticle;

    [SerializeField] private CinemachineCamera focusCamera;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private int focusPriority = 30;
    [SerializeField] private int idlePriority = 0;

    [SerializeField] private Animator branchAni;

    // End Tutorail
    [Header("Tree Dialogue")]
    [SerializeField] private ItemTrigger itemTrigger;
    [SerializeField] private Transform playerPos;
    [SerializeField] private DialogueCameraFocus dialogueCameraFocus;
    [SerializeField] private DialogueCameraFocus cameraFocus;
    [SerializeField] private DialogueTrigger dialogueTrigger;
    // [SerializeField] private TimelineSequenceController timeline;

    [Header("DoTween Setting")]
    [SerializeField] private float tweenDuration = 0.4f;

    [Header("SFX")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip branchOpenClip;

    private Coroutine currentCanvasCoroutine;

    private readonly HashSet<Silverfish> aliveEnemies = new HashSet<Silverfish>();
    private bool hasReachedDestination = false;

    public void Awake()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }

    public void Tutorial()
    {
        StartCoroutine(TutorialFlowRoutine());
    }

    private IEnumerator TutorialFlowRoutine()
    {
        PlayerLoadout loadout = MapManager.Instance.player.GetComponent<PlayerLoadout>();
        loadout.EnterArmedMode();

        yield return StartCoroutine(TransitionSequence(playerSpawn.position));
        DisplayHint("A D鍵進行移動,空白鍵進行跳躍", 4f);

        bool pressedA = false;
        bool pressedD = false;
        bool pressedSpace = false;

        while (!pressedA || !pressedD || !pressedSpace)
        {
            if (GameInputManager.Instance.moveAction.action.WasPressedThisFrame())
            {
                pressedA = true;
                pressedD = true;
            }
            if (GameInputManager.Instance.jumpAction.action.WasPressedThisFrame()) pressedSpace = true;
            yield return null;
        }
        onAllMovementFinished?.Invoke();

        yield return new WaitForSeconds(1f);


        DisplayHint("按下 Shift 來化作墨水穿梭地形,此技能會消耗墨水", 4f);

        bool pressedShift = false;

        while (!pressedShift)
        {
            if (GameInputManager.Instance.dashAction.action.WasPressedThisFrame()) pressedShift = true;
            yield return null;
        }
        onAllShiftFinished?.Invoke();

        yield return new WaitForSeconds(1f);

        DisplayHint("消耗的墨水可藉由移動來回復,試著將墨水補滿吧", 4f);
        PlayerController playerController = MapManager.Instance.player.GetComponent<PlayerController>();
        float maxEnergy = playerController.playerEnergy.maxEnergy;

        while (playerController.playerEnergy.GetCurrentEnergy() < maxEnergy)
        {
            yield return null;
        }

        onEnergyFilled?.Invoke();

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(TransitionSequence(playerSpawn.position));
        DisplayHint("按下左鍵進行攻擊", 4);

        yield return new WaitForSeconds(1f);
        SpawnEnemies();

        yield return new WaitUntil(() => aliveEnemies.Count == 0);
        onAllEnemiesDefeated?.Invoke();
        yield return new WaitForSeconds(1.5f);

        if (playerController.playerHealth != null)
        {
            playerController.playerHealth.ResetPlayerHealth();
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(SpawnHookTutorial());

        yield return new WaitUntil(() => hasReachedDestination);

        // branchAni.SetBool("open", true);
    }

    public void TransformToDialouguePos()
    {
        StartCoroutine(DialogueTransitionRoutine());
    }

    private IEnumerator DialogueTransitionRoutine()
    {
        yield return StartCoroutine(TransitionSequence(dialouguePos.position, keepLocked: true));

        HideAllHookObject();
        cameraFocus.Activate();
        dialogueTrigger.StartDialogue();

        PlayerLock.Unlock(this);
        PlayerLock.Unlock(this, PlayerLockType.Combat);
    }

    private IEnumerator TransitionSequence(Vector3 targetPosition, bool keepLocked = false)
    {
        PlayerController player = MapManager.Instance.player.GetComponent<PlayerController>();

        PlayerLock.Lock(this);
        PlayerLock.Lock(this, PlayerLockType.Combat);

        yield return WaitUntilPlayerSettled(player);

        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1f);

        if (player != null) player.TeleportTo(targetPosition);

        yield return WaitUntilPlayerSettled(player);

        GameEvent.OnToggleFade(true);
        yield return new WaitForSeconds(0.5f);

        if (!keepLocked)
        {
            PlayerLock.Unlock(this);
            PlayerLock.Unlock(this, PlayerLockType.Combat);
        }
    }

    private IEnumerator WaitUntilPlayerSettled(PlayerController player, float timeout = 3f)
    {
        if (player == null) yield break;

        float waited = 0f;
        while (waited < timeout)
        {
            bool settled = player.isGrounded
                           && player.currentState is IdleState
                           && Mathf.Abs(player.rig.linearVelocity.y) < 0.1f;
            if (settled) yield break;

            waited += Time.deltaTime;
            yield return null;
        }

        player.StopMovement();
    }

    private IEnumerator ShowCanvas(string text, float duration)
    {
        hintCanvas.StartBlinking();
        hintCanvas.SetText(text);

        yield return new WaitForSeconds(duration);
        hintCanvas.StopBlinking();
    }

    private void SpawnEnemies()
    {
        foreach (GameObject enemy in silverFishs)
        {
            if (enemy == null) continue;
            enemy.SetActive(true);

            Silverfish fish = enemy.GetComponent<Silverfish>();
            if (fish != null)
            {
                aliveEnemies.Add(fish);
                fish.OnDied += HandleEnemyDied;
            }
        }

        if (enemySpawnDirector != null) enemySpawnDirector.Play();
    }

    private void HandleEnemyDied(Silverfish fish)
    {
        fish.OnDied -= HandleEnemyDied;
        aliveEnemies.Remove(fish);
    }

    private IEnumerator SpawnHookTutorial()
    {
        PlayerLock.Lock(this);
        PlayerLock.Lock(this, PlayerLockType.Combat);

        // Show vines and focus camera for end point
        ShowObject(vines);
        ShowObject(platforms);

        CameraActivate();
        yield return new WaitForSeconds(0.7f);

        playableDirector.Play();
        ShowObjectsWithTween(hookTarget);
        if (lightParticle != null) lightParticle.SetActive(true);



        //yield return new WaitForSeconds(1.0f);

    }

    public void HookTutorial()
    {
        StartCoroutine(StartHookTutorial());
    }

    private IEnumerator StartHookTutorial()
    {
        yield return StartCoroutine(TransitionSequence(hookSpawnPos.position));
        DisplayHint("按下右鍵發射勾索, 請注意此技能會消耗墨水", 4f);

        PlayerLock.Unlock(this);
        PlayerLock.Unlock(this, PlayerLockType.Combat);
    }

    public void CameraActivate()
    {
        if (focusCamera == null) return;

        if (focusCamera.Follow == null)
        {
            focusCamera.Follow = MapManager.Instance.player.transform;
        }

        focusCamera.gameObject.SetActive(true);
        focusCamera.Priority = focusPriority;
    }

    public void CameraDeactivate()
    {
        if (focusCamera == null) return;

        focusCamera.Priority = idlePriority;
        focusCamera.gameObject.SetActive(false);
    }

    public void HideAllHookObject()
    {
        HideObject(platforms);
        HideObjectsWithTween(hookTarget);
        HideObject(vines);
        // HideObjectsWithTween(vines);
    }

    private void ShowObject(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void HideObject(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            Animator animator = obj.GetComponent<Animator>();
            animator.SetTrigger("back");
        }
    }

    private void ShowObjectsWithTween(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(true);

                Vector3 targetScale = obj.transform.localScale;
                if (targetScale == Vector3.zero) targetScale = Vector3.one;

                obj.transform.localScale = Vector3.zero;

                obj.transform.DOScale(targetScale, 2f).SetEase(Ease.OutBack);
            }
        }
    }

    private void HideObjectsWithTween(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf)
            {
                Vector3 originalScale = obj.transform.localScale;

                obj.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack);

                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.DOFade(0f, tweenDuration).OnComplete(() =>
                    {
                        obj.SetActive(false);

                        obj.transform.localScale = originalScale;
                        Color resetColor = sr.color;
                        resetColor.a = 1f;
                        sr.color = resetColor;
                    });
                }
                else
                {
                    obj.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        obj.SetActive(false);
                        obj.transform.localScale = originalScale;
                    });
                }
            }
        }
    }

    public void ReachDestination()
    {
        hasReachedDestination = true;
        if (lightParticle != null) lightParticle.SetActive(false);
        Transform playerTransform = MapManager.Instance.player.transform;
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.StopMovement();
        }

        itemTrigger.StartItem();
    }

    private void DisplayHint(string text, float duration)
    {
        if (currentCanvasCoroutine != null)
        {
            StopCoroutine(currentCanvasCoroutine);
        }
        currentCanvasCoroutine = StartCoroutine(ShowCanvas(text, duration));
    }

    public void PlayBranchSFX()
    {
        audioManager.PlaySFX(branchOpenClip);
    }
}