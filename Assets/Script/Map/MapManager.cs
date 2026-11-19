using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Core References")]
    public GameObject player;
    public AudioManager audioManager;
    public Animator fadeAni;
    //public CinemachineCamera activeVCam;
    public event Action OnMapSetupComplete;

    [Header("Start Setting")]
    public string startingMap = "Story_TheCore";
    public string startingSpawnID = "TheCore_1";

    public string currentActiveMap {get; private set;}

    [Header("Other Setting")]
    public float holdBeforeCameraMove = 0.4f;
    public float cameraBlendDuration = 0.6f;

    private bool isTeleporting;
    private readonly HashSet<string> loadedMaps = new HashSet<string>();
    private readonly Dictionary<string, AsyncOperation> preloadOps = new Dictionary<string, AsyncOperation>();
    public event Action OnCameraTransitionStart;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        //map test (will delete)
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != "BaseScene")
                {
                    currentActiveMap = scene.name;
                    loadedMaps.Add(scene.name);
                    SceneManager.SetActiveScene(scene);
                    break;
                }
            }

            SpawnPointInfo testSpawn = FindFirstObjectByType<SpawnPointInfo>();
            if (testSpawn != null && player != null)
            {
                player.SetActive(false);
                player.transform.position = testSpawn.transform.position;
                player.SetActive(true);
            }

            OnMapSetupComplete?.Invoke();
        }
        else
        {
            StartCoroutine(FirstLoad(startingMap));
        }
    }

    public void ChangeMap(string newMapName, string targetSpawnID, int type = 0)
    {
        if (string.IsNullOrEmpty(newMapName)) return;

        switch (type)
        {
            case 0:
                if (isTeleporting) return;
                StartCoroutine(TransitionToMap(newMapName, targetSpawnID));
                break;

            case 1:
                TransitionByTeleport(newMapName, targetSpawnID);
                break;

            case 2:
                EnsureAdjacentMapLoaded(newMapName);
                break;

            default:
                TransitionByTeleport(newMapName, targetSpawnID);
                break;
        }
    }

    #region Preload Teleport (no fade ani)
    /// <summary>
    /// preloadTeleport no fade ani
    /// </summary>
    /// <param name="mapName"></param>
    public void PreloadMap(string mapName)
    {
        if (string.IsNullOrEmpty(mapName) || mapName == currentActiveMap) return;
        if (loadedMaps.Contains(mapName) || preloadOps.ContainsKey(mapName)) return;

        Scene existing = SceneManager.GetSceneByName(mapName);
        if (existing.IsValid() && existing.isLoaded)
        {
            loadedMaps.Add(mapName);
            return;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Additive);
        if (op == null) return;

        op.allowSceneActivation = false;
        preloadOps[mapName] = op;
    }

    public void TransitionByTeleport(string targetMapName, string targetSpawnID)
    {
        if (isTeleporting || string.IsNullOrEmpty(targetMapName)) return;
        StartCoroutine(TeleportRoutine(targetMapName, targetSpawnID));
    }

    private IEnumerator TeleportRoutine(string targetMapName, string targetSpawnID)
    {
        isTeleporting = true;
        List<string> mapsToUnload = new List<string>(loadedMaps);
        mapsToUnload.Remove(targetMapName);

        AsyncOperation loadOp;
        if (!preloadOps.TryGetValue(targetMapName, out loadOp) || loadOp == null)
        {
            loadOp = SceneManager.LoadSceneAsync(targetMapName, LoadSceneMode.Additive);
            if (loadOp != null) loadOp.allowSceneActivation = false;
        }
        else
        {
            preloadOps.Remove(targetMapName);
        }

        while (loadOp != null && loadOp.progress < 0.9f)
        {
            yield return null;
        }

        if (loadOp != null)
        {
            loadOp.allowSceneActivation = true;
            while (!loadOp.isDone)
            {
                yield return null;
            }
        }

        Scene targetScene = SceneManager.GetSceneByName(targetMapName);
        PlayerController playerController = null;
    
        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            loadedMaps.Add(targetMapName);
            SceneManager.SetActiveScene(targetScene);
            currentActiveMap = targetMapName;

            Transform spawnTransform = FindSpawnPointInScene(targetScene, targetSpawnID);
            if (spawnTransform != null && player != null)
            {
                player.SetActive(false);
                player.transform.position = spawnTransform.position;
                player.SetActive(true);

                playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    if (spawnTransform.localScale.x < 0)
                        playerController.HandleTurning(false);
                    else if (spawnTransform.localScale.x > 0)
                        playerController.HandleTurning(true);
                }
            }
            if (playerController != null && spawnTransform != null)
            {
                playerController.UpdateCheckpoint(spawnTransform.position);
            }
        }

        OnMapSetupComplete?.Invoke();
        yield return null;

        
        if (holdBeforeCameraMove > 0f)
        {
            yield return new WaitForSeconds(holdBeforeCameraMove);
        }

        OnCameraTransitionStart?.Invoke();

        if (cameraBlendDuration > 0f && mapsToUnload.Count > 0)
        {
            yield return new WaitForSeconds(cameraBlendDuration);
        }

        foreach (string oldMap in mapsToUnload)
        {
            yield return UnloadMapRoutine(oldMap);
        }

        isTeleporting = false;
    }
    #endregion

    #region AdjacentStream (no fade ani)
    public void EnsureAdjacentMapLoaded(string mapName)
    {
        if (string.IsNullOrEmpty(mapName) || loadedMaps.Contains(mapName)) return;

        if (preloadOps.TryGetValue(mapName, out AsyncOperation preOp) && preOp != null)
        {
            preloadOps.Remove(mapName);
            StartCoroutine(ActivatePreloadedAdjacentRoutine(mapName, preOp));
            return;
        }

        StartCoroutine(LoadAdjacentRoutine(mapName));
    }

    public void UnloadAdjacentMapIfFar(string mapName)
    {
        if (string.IsNullOrEmpty(mapName) || mapName == currentActiveMap) return;
        if (!loadedMaps.Contains(mapName)) return;

        StartCoroutine(UnloadMapRoutine(mapName));
    }

    private IEnumerator LoadAdjacentRoutine(string mapName)
    {
        Scene existing = SceneManager.GetSceneByName(mapName);
        if (existing.IsValid() && existing.isLoaded)
        {
            loadedMaps.Add(mapName);
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Additive);
        if (op == null) yield break;

        while (!op.isDone)
        {
            yield return null;
        }

        loadedMaps.Add(mapName);
        OnMapSetupComplete?.Invoke();
    }

    public void SetActiveAdjacentRegion(string mapName)
    {
        if (string.IsNullOrEmpty(mapName)) return;
        Scene scene = SceneManager.GetSceneByName(mapName);
        if (scene.IsValid() && scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
            currentActiveMap = mapName;
        }
    }

    private IEnumerator ActivatePreloadedAdjacentRoutine(string mapName, AsyncOperation op)
    {
        while (op.progress < 0.9f)
        {
            yield return null;
        }
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
        loadedMaps.Add(mapName);
        OnMapSetupComplete?.Invoke();
    }

    private IEnumerator UnloadMapRoutine(string mapName)
    {
        if (!loadedMaps.Contains(mapName)) yield break;

        Scene scene = SceneManager.GetSceneByName(mapName);
        loadedMaps.Remove(mapName);

        if (scene.IsValid() && scene.isLoaded)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
            while (unloadOp != null && !unloadOp.isDone)
            {
                yield return null;
            }
        }
    }
    #endregion

    #region Normal Change (has fade ani)
    public void ChangeMapWithFade(string newMapName, string targetSpawnID)
    {
        if (isTeleporting || string.IsNullOrEmpty(newMapName)) return;
        StartCoroutine(TransitionToMap(newMapName, targetSpawnID));
    }

    /// <summary>
    /// map to map
    /// </summary>
    /// <param name="newMapName"></param>
    /// <param name="targetSpawnID"></param>
    /// <returns></returns>
    private IEnumerator TransitionToMap(string newMapName, string targetSpawnID)
    {
        isTeleporting = true;

        //fading ani
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);;

        player.SetActive(false); // diactivate all player action

        // if preLoadList has this map
        if (preloadOps.TryGetValue(newMapName, out AsyncOperation pendingPreload))
        {
            preloadOps.Remove(newMapName);
            if (pendingPreload != null && !pendingPreload.isDone)
            {
                pendingPreload.allowSceneActivation = true;
                while (!pendingPreload.isDone)
                {
                    yield return null;
                }
            }
        }

        //unload map, don't unload if is startingMap
        if (!string.IsNullOrEmpty(currentActiveMap) && currentActiveMap != newMapName)
        {
            Scene oldScene = SceneManager.GetSceneByName(currentActiveMap);
            loadedMaps.Remove(currentActiveMap);

            if (oldScene.IsValid() && oldScene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);
                while (unloadOp != null && !unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }

        //load new map
        Scene newLoadedScene = SceneManager.GetSceneByName(newMapName);
        if (!newLoadedScene.IsValid() || !newLoadedScene.isLoaded)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(newMapName, LoadSceneMode.Additive);
            while (loadOp != null && !loadOp.isDone)
            {
                yield return null;
            }
            newLoadedScene = SceneManager.GetSceneByName(newMapName);
        }

        if (newLoadedScene.IsValid() && newLoadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(newLoadedScene);
            currentActiveMap = newMapName;
            loadedMaps.Add(newMapName);
        }

        //handle player position and face
        Transform finalSpawnPos = null;
        SceneController sceneController = FindAnyObjectByType<SceneController>();
        SpawnPointInfo[] allSpawnPoints = sceneController.spawnPointInfos;

        foreach(var spawnPoint in allSpawnPoints)
        {
            if(spawnPoint.spawnPointID == targetSpawnID)
            {
                finalSpawnPos = spawnPoint.transform;
                break;
            }
        }

        Vector3 delta = finalSpawnPos.position - player.transform.position;
        player.transform.position = finalSpawnPos.position;
        //activeVCam.OnTargetObjectWarped(player.transform, delta);

        PlayerController playerController = player.transform.GetComponent<PlayerController>();

        if(finalSpawnPos.localScale.x < 0)
        {
            playerController.HandleTurning(false);
        }
        else if(finalSpawnPos.localScale.x > 0)
        {
            playerController.HandleTurning(true);
        }

        OnMapSetupComplete?.Invoke();
        player.SetActive(true);

        GameEvent.OnToggleFade(true);
        playerController.UpdateCheckpoint(finalSpawnPos.position);

        isTeleporting = false;
    }

    /// <summary>
    /// main menu to map
    /// </summary>
    /// <param name="defaultMapName"></param>
    /// <returns></returns>
    private IEnumerator FirstLoad(string defaultMapName)
    {
        fadeAni.speed = 0f;
        player.SetActive(false); // diactivate all player action

        List<string> leftoverMaps = new List<string>(loadedMaps);
        foreach(string oldMap in leftoverMaps)
        {
            yield return UnloadMapRoutine(oldMap);
        }

        //load new map
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(defaultMapName, LoadSceneMode.Additive);
        while (loadOp != null && !loadOp.isDone)
        {
            yield return null;
        }

        Scene newLoadedScene = SceneManager.GetSceneByName(defaultMapName);
        if (newLoadedScene.IsValid())
        {
            SceneManager.SetActiveScene(newLoadedScene);
        }

        currentActiveMap = defaultMapName;
        loadedMaps.Add(defaultMapName);

        //handle player position and face
        Transform finalSpawnPos = null;
        SceneController sceneController = FindAnyObjectByType<SceneController>();
        SpawnPointInfo[] allSpawnPoints = sceneController.spawnPointInfos;
        string targetSpawnID = SaveManager.Instance.currentSave.player?.spawnPoint;
        if(targetSpawnID == null) targetSpawnID = startingSpawnID;

        foreach(var spawnPoint in allSpawnPoints)
        {
            if(spawnPoint.spawnPointID == targetSpawnID)
            {
                finalSpawnPos = spawnPoint.transform;
                break;
            }
        }

        player.transform.position = finalSpawnPos.position;
        //activeVCam.OnTargetObjectWarped(player.transform, Vector3.zero);

        PlayerController playerController = player.transform.GetComponent<PlayerController>();

        if(finalSpawnPos.localScale.x < 0)
        {
            playerController.HandleTurning(false);
        }
        else if(finalSpawnPos.localScale.x > 0)
        {
            playerController.HandleTurning(true);
        }

        OnMapSetupComplete?.Invoke();
        player.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        //fading ani
        fadeAni.speed = 1f;
        yield return new WaitForSeconds(1.2f);
        
        playerController.UpdateCheckpoint(finalSpawnPos.position);
        GameEvent.OnInteractStateChanged(false);
    }

    public void ChangeMapToMainMenu()
    {
        StartCoroutine(TransitionToMainMenu());
    }

    private IEnumerator TransitionToMainMenu()
    {
        GameEvent.OnToggleFade(false);
        player.SetActive(false);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }

    public void QuitGameWithFade()
    {
        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        GameEvent.OnToggleFade(false);
        player.SetActive(false);
        yield return new WaitForSeconds(1.2f);
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    #endregion

    private Transform FindSpawnPointInScene(Scene scene, string spawnID)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            SpawnPointInfo[] points = root.GetComponentsInChildren<SpawnPointInfo>(true);
            foreach (SpawnPointInfo point in points)
            {
                if (point.spawnPointID == spawnID)
                {
                    return point.transform;
                }
            }
        }
        return null;
    }
}
