using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public GameObject player;
    public AudioManager audioManager;
    public event Action OnMapSetupComplete;

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
        ChangeMap(startingMap, startingSpawnID);
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
        
        player.SetActive(false);
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
    }
}
