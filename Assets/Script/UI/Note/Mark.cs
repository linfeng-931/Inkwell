using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Mark : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isAct;
    public Color[] colors;
    public CurrentPage currentPage;
    public int index;
    public float switchLayerDelay;
    public float switchLayerDelayClose;
    public FlipPage flipPage;

    private float oriPosY;
    private Button button;
    private float oriSwitchLayerDelay;
    private float oriSwitchLayerDelayClose;

    void Start()
    {
        oriPosY = transform.localPosition.y;
        button = GetComponent<Button>();
        
        if (!isAct)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = colors[1];
            button.colors = cb;
            transform.localScale = Vector3.one * 0.8f;
        }
        oriSwitchLayerDelay = switchLayerDelay;
        oriSwitchLayerDelayClose = switchLayerDelayClose;
    }

    public void setAct(bool flag)
    {
        if(flag){
            transform.localScale = Vector3.one;
            ColorBlock cb = button.colors;
            cb.normalColor = colors[1];
            button.colors = cb;
            isAct = true;
            if(currentPage.majorIndex < index)
            {
                if(index == 2){
                    switchLayerDelay = oriSwitchLayerDelayClose;
                    switchLayerDelayClose = oriSwitchLayerDelay;
                }
                else if(index == 1)
                {
                    switchLayerDelay = oriSwitchLayerDelay;
                    switchLayerDelayClose = oriSwitchLayerDelayClose;
                }
                flipPage.Flip(false);
            }
            else
            {
                if(index == 2){
                    switchLayerDelay = oriSwitchLayerDelay;
                    switchLayerDelayClose = oriSwitchLayerDelayClose;
                }
                else if(index == 1){
                    switchLayerDelay = oriSwitchLayerDelayClose;
                    switchLayerDelayClose = oriSwitchLayerDelay;
                }
                flipPage.Flip(true);
            }
            currentPage.majorIndex = index;
            button.enabled = false;
            transform.localPosition = new Vector3(transform.localPosition.x, oriPosY, 0);
        }
        else{
            transform.localScale = Vector3.one * 0.9f;
            ColorBlock cb = button.colors;
            cb.normalColor = colors[1];
            button.colors = cb;
            isAct = false;
            button.enabled = true;
            transform.localPosition = new Vector3(transform.localPosition.x, oriPosY, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!isAct)
        transform.localPosition = new Vector3(transform.localPosition.x, oriPosY + 20f, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!isAct)
        transform.localPosition = new Vector3(transform.localPosition.x, oriPosY , 0);
    }
}