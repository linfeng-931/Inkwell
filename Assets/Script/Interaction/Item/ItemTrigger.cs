using UnityEngine;
using UnityEngine.Events;

public class ItemTrigger : MonoBehaviour
{
    public string targetItemId;
    public int amount = 1;
    public UnityEvent onIntroClosed;

    public void StartItem()
    {
        ItemManager.Instance.ExecuteItemIntro(targetItemId, false);
        GameEvent.currentCloseCallback += OnIntroClosed;
    }

    public void GetItem()
    {
        ItemManager.Instance.PlayerGetItem(targetItemId, true, amount);
    }

    private void OnIntroClosed()
    {
        onIntroClosed?.Invoke();
    }
}
