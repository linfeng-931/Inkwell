using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCell : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public string itemID;
    [SerializeField] private RectTransform faceRectTransform;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject); 
        UpdateInfoPanel();
    }

    public void OnSelect(BaseEventData eventData)
    {
        UpdateInfoPanel();
    }

    private void UpdateInfoPanel()
    {
        GameEvent.OnRefreshItemPage?.Invoke(itemID);
        RectTransform target = faceRectTransform != null ? faceRectTransform : rectTransform;
        GameEvent.OnUpdateSelectionFrame?.Invoke(target);
    }
}
