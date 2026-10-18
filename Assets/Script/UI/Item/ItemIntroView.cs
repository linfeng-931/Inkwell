using UnityEngine;
using TMPro;

public class ItemIntroView : BasePanel
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI decs;
    [SerializeField] private TextMeshProUGUI subDecs;

    public void UpdateItemIntroText(ItemData itemData)
    {
        itemName.text = itemData.stageNames[itemData.currentStage];
        this.decs.text = itemData.stageDescs[itemData.currentStage];
        this.subDecs.text = itemData.stageSubDescs[itemData.currentStage];
    }
}
