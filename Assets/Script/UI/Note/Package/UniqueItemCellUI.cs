using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UniqueItemCellUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button clickButton;


    /// <summary>
    /// let InventoryView call to fresh
    /// </summary>
    public void SetVisual(Sprite icon, string itemName)
    {
        itemIcon.sprite = icon;


        if (nameText != null)
        {
            nameText.text = itemName;
        }
    }
}
