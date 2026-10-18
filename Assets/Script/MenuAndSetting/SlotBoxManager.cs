using System.Collections.Generic;
using UnityEngine;

public class SlotBoxManager : MonoBehaviour
{
    public List<SlotBox> slotBoxes;

    void Start()
    {
        List<string> allSaveFiles = SaveManager.Instance.GetAllSaveFiles();
        foreach(SlotBox slotBox in slotBoxes)
        {
            if (allSaveFiles.Contains(slotBox.slotName))
            {
                slotBox.Initialize(true);
            }
            else
            {
                slotBox.Initialize(false);
            }
        }
        
    }
}
