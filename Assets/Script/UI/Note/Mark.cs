using UnityEngine;
using UnityEngine.EventSystems; // 必須引用這個命名空間

public class Mark : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float oriPosY;
    void Start()
    {
        oriPosY = transform.localPosition.y;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //transform.localScale = Vector3.one * 1.1f;
        transform.localPosition = new Vector3(transform.localPosition.x, oriPosY + 20f, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //transform.localScale = Vector3.one;
        transform.localPosition = new Vector3(transform.localPosition.x, oriPosY , 0);
    }
}