using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum PlayerLockType
{
    Movement, // ¤ô¥­²¾°Ê + ¸õÅD
    Combat    // §ðÀ»¡B½Ä¨ë¡B¹_¯Á¡B®gÀ»
}

public static class PlayerLock
{

    private static readonly Dictionary<PlayerLockType, HashSet<object>> owners =
        new Dictionary<PlayerLockType, HashSet<object>>
        {
            { PlayerLockType.Movement, new HashSet<object>() },
            { PlayerLockType.Combat,   new HashSet<object>() },
        };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        foreach (var set in owners.Values) set.Clear();
    }

    public static void Lock(object owner, PlayerLockType type = PlayerLockType.Movement)
    {
        Debug.Log(owner);
        if (owner == null) return;
        if (owners[type].Add(owner))
        {
            Debug.Log($"[PlayerLock] {type} LOCK by {Name(owner)} (now {owners[type].Count})");
        }
    }

    public static void Unlock(object owner, PlayerLockType type = PlayerLockType.Movement)
    {
        Debug.Log(owner);
        if (owner == null) return;
        if (owners[type].Remove(owner))
        {
            Debug.Log($"[PlayerLock] {type} UNLOCK by {Name(owner)} (now {owners[type].Count})");
        }
    }

    public static void UnlockAll(object owner)
    {
        if (owner == null) return;
        foreach (PlayerLockType type in owners.Keys) Unlock(owner, type);
    }

    public static bool IsLocked(PlayerLockType type = PlayerLockType.Movement)
    {
        HashSet<object> set = owners[type];
        if (set.Count == 0) return false;
        set.RemoveWhere(o => o is Object unityObj && unityObj == null);
        return set.Count > 0;
    }

    public static bool IsLockedBy(object owner, PlayerLockType type = PlayerLockType.Movement)
    {
        return owner != null && owners[type].Contains(owner);
    }

    public static string Describe()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var pair in owners)
        {
            sb.Append(pair.Key).Append(": ");
            if (pair.Value.Count == 0) { sb.Append("(none)"); }
            else
            {
                foreach (object o in pair.Value) sb.Append(Name(o)).Append(", ");
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static string Name(object owner)
    {
        return owner is Object unityObj && unityObj != null ? $"{unityObj.GetType().Name}({unityObj.name})" : owner.ToString();
    }
}