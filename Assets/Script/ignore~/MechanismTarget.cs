using UnityEngine;

public class MechanismTarget : MonoBehaviour
{
    public string mechanismID;

    void Start()
    {
        MechanismManager.Instance.Register(mechanismID, gameObject);
    }

    private void OnDestroy()
    {
        MechanismManager.Instance.Unregister(mechanismID, gameObject);
    }
}
