using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    public string targetItemId;
    public int amount = 1;

    public void StartItem()
    {
        ItemManager.Instance.ExecuteItemIntro(targetItemId, false);
    }

    public void GetItem()
    {
        ItemManager.Instance.PlayerGetItem(targetItemId, true, amount);
    }
}
