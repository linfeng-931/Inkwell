using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpawnPointInfo))]
public class SavePoint : MonoBehaviour
{
    public Animator inkAni;

    private string spawnPointID;
    
    void Start()
    {
        spawnPointID = GetComponent<SpawnPointInfo>().spawnPointID;
        GameEvent.OnGameLoaded += Load;
        GameEvent.OnGameSaved += Save;
    }

    void OnDestroy()
    {
        GameEvent.OnGameLoaded -= Load;
        GameEvent.OnGameSaved -= Save;
    }

    public void Trigger(Transform startPoint)
    {
        SaveManager.Instance.currentSave.player.spawnPoint = this.spawnPointID;
        GameEvent.OnPlayerSave.Invoke(startPoint);
        StartCoroutine(HandleAni());
    }

    private IEnumerator HandleAni()
    {
        yield return new WaitForSeconds(0.2f);

        inkAni.ResetTrigger("inkDown"); 
        inkAni.SetTrigger("inkUp");
    }

    private void Load()
    {
        var data = SaveManager.Instance.currentSave.player;
        if(this.spawnPointID == data.spawnPoint)
        {
            inkAni.ResetTrigger("inkDown");
            inkAni.SetTrigger("inkUp");
        }
    }

    private void Save()
    {
        var data = SaveManager.Instance.currentSave.player;
        if(this.spawnPointID != data.spawnPoint)
        {
            inkAni.ResetTrigger("inkUp");
            inkAni.SetTrigger("inkDown");
        }
    }
}
