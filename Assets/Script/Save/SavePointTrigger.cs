using UnityEngine;

public class SavePointTrigger : MonoBehaviour
{
    public void Trigger(Transform startPoint)
    {
        GameEvent.OnPlayerSave.Invoke(startPoint);
    }
}
