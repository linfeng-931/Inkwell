using UnityEngine;
using System.Collections.Generic;

public class GhostSocket : MonoBehaviour
{
    public List<string> expectedItemIds = new List<string>();
    public List<GameObject> ghostItemVisuals = new List<GameObject>();
    public bool isOccupied = false;
    public string answerItemId;

    private GearActive manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = FindObjectOfType<GearActive>();
        if (ghostItemVisuals != null)
            for (int i = 0; i < ghostItemVisuals.Count; i++) {
                ghostItemVisuals[i].SetActive(false);
            } 
    }

    public void ShowGhost(string incomingItemId)
    {
        int targetId = 0;
        if (isOccupied) return;

        if (expectedItemIds.Contains(incomingItemId))
        {
            for (int i = 0; i < ghostItemVisuals.Count; i++)
            {
                if (ghostItemVisuals[i].name == incomingItemId)
                {
                    targetId = i;
                    break;
                }
            }
            if (ghostItemVisuals[targetId] != null) ghostItemVisuals[targetId].SetActive(true);
        };
    }

    public void NotifyStatusChanged(string itemId)
    {
        if (manager == null) return;

        bool isCorrect = isOccupied && (itemId == answerItemId);

        manager.UpdateSocketStatus(this, isCorrect);
    }

    public void HideGhost()
    {
        if (ghostItemVisuals != null)
            for (int i = 0; i < ghostItemVisuals.Count; i++)
            {
                ghostItemVisuals[i].SetActive(false);
            }
    }
}
