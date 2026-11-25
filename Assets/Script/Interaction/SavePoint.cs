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
        SaveManager.Instance.CurrentSave.player.spawnPoint = this.spawnPointID;
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
        var data = SaveManager.Instance.CurrentSave.player;
        if(this.spawnPointID == data.spawnPoint)
        {
            inkAni.ResetTrigger("inkDown");
            inkAni.SetTrigger("inkUp");
        }
    }

    private void Save()
    {
        var data = SaveManager.Instance.CurrentSave.player;

        if (data == null || inkAni == null)
        {
            if (data == null) Debug.Log("沒有對應的儲存資料");
            if (inkAni == null) Debug.Log("沒有對應的動畫");
            return;
        }


        if (this.spawnPointID != data.spawnPoint)
        {
            inkAni.ResetTrigger("inkUp");
            inkAni.SetTrigger("inkDown");
        }
    }
}
