using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    public string targetItemId;

    public void StartItem()
    {
        ItemManager.Instance.ExecuteItemIntro(targetItemId);
    }
}
