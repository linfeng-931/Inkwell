using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Transform bloodContainer;
    [SerializeField] private GameObject lowHpEffect;

    private List<GameObject> bloodList = new List<GameObject>();

    void Awake()
    {
        foreach (Transform child in bloodContainer)
        {
            bloodList.Add(child.gameObject);
        }
    }

    public void UpdateVisuals(int current, int max)
    {
        for (int i = 0; i < bloodList.Count; i++)
        {
            bool shouldBeActive = i < current;
            if (bloodList[i].activeSelf != shouldBeActive)
            {
                bloodList[i].SetActive(shouldBeActive);
            }
        }
        if(current <= 1)
        {
            lowHpEffect.SetActive(true);
        }
        else if (lowHpEffect.activeSelf)
        {
            lowHpEffect.SetActive(false);
        }
    }
}
