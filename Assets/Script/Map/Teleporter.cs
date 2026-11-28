using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public string targetSceneName;
    public string targetSpawnPointID;
    public int transitionType = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEvent.OnInteractStateChanged(true);
            MapManager.Instance.ChangeMap(targetSceneName, targetSpawnPointID, transitionType);
        }
    }
}
