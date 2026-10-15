using UnityEngine;
using System.Collections.Generic;

public class MechanismManager : MonoBehaviour
{
    public static MechanismManager Instance { get; private set;}

    private Dictionary<string, List<GameObject>> mechanismDict = new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// create items list
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj"></param>
    public void Register(string id, GameObject obj)
    {
        if (!mechanismDict.ContainsKey(id)) // spawn list if list hasn't contains id
        {
            mechanismDict[id] = new List<GameObject>();
        }
        mechanismDict[id].Add(obj);
    }

    /// <summary>
    /// remove id when change scene
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj"></param>
    public void Unregister(string id, GameObject obj)
    {
        if (mechanismDict.ContainsKey(id))
        {
            mechanismDict[id].Remove(obj);
        }
    }

    #region Operating instructions
    public void Activate(string id)
    {
        if(mechanismDict.TryGetValue(id, out List<GameObject> objs))
        {
            foreach(GameObject obj in objs)
            {
                obj.SetActive(true);
            }
        }
    }

    public void Deactivate(string id)
    {
        if(mechanismDict.TryGetValue(id, out List<GameObject> objs))
        {
            foreach(GameObject obj in objs)
            {
                obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// turn obj's active status
    /// </summary>
    /// <param name="id"></param>
    public void Toggle(string id)
    {
        if(mechanismDict.TryGetValue(id, out List<GameObject> objs))
        {
            foreach(GameObject obj in objs)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }

    #endregion
}
