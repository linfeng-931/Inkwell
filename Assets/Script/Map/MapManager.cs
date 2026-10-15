using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public GameObject player;
    public AudioManager audioManager;
    public event Action OnMapSetupComplete;
    public Animator fadeAni;

    [Header("Start Setting")]
    public string startingMap = "Story_TheCore";
    public string startingSpawnID = "TheCore_1";

    private string currentMap;

    void Awake()
    {
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
                    currentMap = scene.name;
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

            OnMapSetupComplete.Invoke();
        }
        else
        {
            StartCoroutine(FirstLoad(startingMap, startingSpawnID));
        }
    }

    public void ChangeMap(string newMapName, string targetSpawnID)
    {
        StartCoroutine(TransitionToMap(newMapName, targetSpawnID));
    }

    private IEnumerator TransitionToMap(string newMapName, string targetSpawnID)
    {
        //fading ani
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);;

        player.SetActive(false); // diactivate all player action

        //unload map, don't unload if is startingMap
        if(!string.IsNullOrEmpty(currentMap))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentMap);
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }

        //load new map
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newMapName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        Scene newLoadedScene = SceneManager.GetSceneByName(newMapName);
        SceneManager.SetActiveScene(newLoadedScene);

        currentMap = newMapName;

        //handle player position and face
        Transform finalSpawnPos = null;
        SpawnPointInfo[] allSpawnPoints = FindObjectsByType<SpawnPointInfo>(FindObjectsSortMode.None);

        foreach(var spawnPoint in allSpawnPoints)
        {
            if(spawnPoint.spawnPointID == targetSpawnID)
            {
                finalSpawnPos = spawnPoint.transform;
                break;
            }
        }

        player.transform.position = finalSpawnPos.position;
        PlayerController playerController = player.transform.GetComponent<PlayerController>();
        if(finalSpawnPos.localScale.x < 0 && !playerController.isFacingRight)
        {
            player.transform.GetComponent<PlayerController>().HandleTurning();
        }
        else if(finalSpawnPos.localScale.x > 0 && playerController.isFacingRight)
        {
            player.transform.GetComponent<PlayerController>().HandleTurning();
        }
        player.SetActive(true);

        OnMapSetupComplete.Invoke();
        
        GameEvent.OnToggleFade(true);
        playerController.UpdateCheckpoint(finalSpawnPos.position);
    }

    private IEnumerator FirstLoad(string newMapName, string targetSpawnID)
    {
        fadeAni.speed = 0f;
        //處理使用者輸入
        player.SetActive(false); // diactivate all player action

        //unload map, don't unload if is startingMap
        if(!string.IsNullOrEmpty(currentMap))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentMap);
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }

        //load new map
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newMapName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        Scene newLoadedScene = SceneManager.GetSceneByName(newMapName);
        SceneManager.SetActiveScene(newLoadedScene);

        currentMap = newMapName;

        //handle player position and face
        Transform finalSpawnPos = null;
        SpawnPointInfo[] allSpawnPoints = FindObjectsByType<SpawnPointInfo>(FindObjectsSortMode.None);

        foreach(var spawnPoint in allSpawnPoints)
        {
            if(spawnPoint.spawnPointID == targetSpawnID)
            {
                finalSpawnPos = spawnPoint.transform;
                break;
            }
        }

        player.transform.position = finalSpawnPos.position;
        PlayerController playerController = player.transform.GetComponent<PlayerController>();
        if(finalSpawnPos.localScale.x < 0 && !playerController.isFacingRight)
        {
            player.transform.GetComponent<PlayerController>().HandleTurning();
        }
        else if(finalSpawnPos.localScale.x > 0 && playerController.isFacingRight)
        {
            player.transform.GetComponent<PlayerController>().HandleTurning();
        }
        player.SetActive(true);
        OnMapSetupComplete.Invoke();

        yield return new WaitForSeconds(0.5f);

        //fading ani
        fadeAni.speed = 1f;
        yield return new WaitForSeconds(1.2f);

        
        playerController.UpdateCheckpoint(finalSpawnPos.position);
        GameEvent.OnInteractStateChanged(false);
    }
}
