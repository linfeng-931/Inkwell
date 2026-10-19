using UnityEngine;
using TMPro;

public class ItemIntroViewForNote : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI decs;
    [SerializeField] private TextMeshProUGUI subDecs;

    private void OnEnable()
    {
        GameEvent.OnItemIntroChange += UpdateItemIntroText;
    }

    private void OnDisable()
    {
        GameEvent.OnItemIntroChange -= UpdateItemIntroText;
    }

    public void RefreshItemData(string id)
    {
        ItemManager.Instance.ExecuteItemIntro(id, true);
    }


    public void RefreshItemPage()
    {
        container.SetActive(false);
    }

    private void UpdateItemIntroText()
    {
        ItemData itemData = ItemManager.Instance.currentItem;

        itemName.text = itemData.stageNames[itemData.currentStage];
        this.decs.text = itemData.stageDescs[itemData.currentStage];
        this.subDecs.text = itemData.stageSubDescs[itemData.currentStage];
    }
}
