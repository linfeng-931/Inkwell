using UnityEngine;
using TMPro;
using Unity.UI;

public class ItemIntroView : BasePanel
{
    [SerializeField] private UnityEngine.UI.Image cover;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI decs;
    [SerializeField] private TextMeshProUGUI subDecs;

    public void UpdateItemIntroText(ItemData itemData)
    {
        string imagePath = itemData.stageIconPaths[itemData.currentStage];
        Sprite loadedSprite = Resources.Load<Sprite>(imagePath);
        if (loadedSprite != null)
        {
            cover.sprite = loadedSprite;
        }

        itemName.text = itemData.stageNames[itemData.currentStage];
        this.decs.text = itemData.stageDescs[itemData.currentStage];
        if(subDecs != null) this.subDecs.text = itemData.stageSubDescs[itemData.currentStage];
    }
}
