using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Mark : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    
    public Color[] colors;
    public float switchLayerDelay;
    public float switchLayerDelayClose;

    private bool isAct;
    private int index;
    private float oriPosY;
    private Button button;
    private Action onClickAction;

    public void Init(int index, Action onClickCallback)
    {
        this.index = index;
        button = GetComponent<Button>();
        oriPosY = transform.localPosition.y;
        onClickAction = onClickCallback;
        
        button.onClick.AddListener(() => onClickAction?.Invoke());
        SetVisualAct(isAct);
    }

    public void SetVisualAct(bool flag)
    {
        isAct = flag;
        transform.localScale = flag ? Vector3.one : Vector3.one * 0.9f;

        // change color 
        ColorBlock cb = button.colors;
        cb.normalColor = flag ? colors[0] : colors[1];
        button.colors = cb;

        // change action and position
        button.enabled = !flag;
        SetHoverOffset(false);
    }

    private void SetHoverOffset(bool hoveredOrSelected)
    {
        float targetY = (!isAct && hoveredOrSelected) ? oriPosY + 20f : oriPosY;
        transform.localPosition = new Vector3(transform.localPosition.x, targetY, 0);
    }

    // mouse
    public void OnPointerEnter(PointerEventData eventData) => SetHoverOffset(true);
    public void OnPointerExit(PointerEventData eventData) => SetHoverOffset(false);

    // controller
    public void OnSelect(BaseEventData eventData) => SetHoverOffset(true);
    public void OnDeselect(BaseEventData eventData) => SetHoverOffset(false);
}