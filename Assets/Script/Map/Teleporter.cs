using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public string targetSceneName;
    public string targetSpawnPointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MapManager.Instance.ChangeMap(targetSceneName, targetSpawnPointID);
        }
    }
}
