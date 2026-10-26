using UnityEngine;
using UnityEngine.Events;

public class ItemTrigger : MonoBehaviour
{
    public string targetItemId;
    public int amount = 1;

    [Header("Player Lock")]
    [SerializeField] private bool lockMovement = true;
    [SerializeField] private bool lockCombat = true;

    [SerializeField] private bool keepLockedAfterIntro = false;

    public UnityEvent onIntroClosed;

    public void StartItem()
    {
        ItemManager.Instance.ExecuteItemIntro(targetItemId, false);
        GameEvent.currentCloseCallback += OnIntroClosed;

        LockPlayerMovement();
    }

    public void GetItem()
    {
        ItemManager.Instance.PlayerGetItem(targetItemId, true, amount);
    }

    private void OnIntroClosed()
    {
        onIntroClosed?.Invoke();

        if (!keepLockedAfterIntro)
        {
            UnlockPlayerMovement();
        }
    }

    public void LockPlayerMovement()
    {
        if (lockMovement) PlayerLock.Lock(this);
        if (lockCombat) PlayerLock.Lock(this, PlayerLockType.Combat);
    }

    public void UnlockPlayerMovement()
    {
        PlayerLock.Unlock(this, PlayerLockType.Combat);
        PlayerLock.Unlock(this);
    }
}