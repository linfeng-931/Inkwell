using UnityEngine;
using UnityEngine.UI;

public class DissolveEffect : MonoBehaviour
{
    public Material dissolve;
    public float max;
    public float min;
    public float speed;
    public bool isAct;
    public Image image;
    public FlipPage flipPage;
    public bool canAct;

    private bool already;
    private float current;
    private float timer;

    void Start()
    {
        isAct = false;
        already = false;
        timer = 0f;
        current = min;
        canAct = true;
    }

    void Update()
    {
        if (already)
        {
            timer += Time.unscaledDeltaTime;
            if(timer >= 0.65f) isAct = false;
        }

        if (isAct && !already)
        {
            current += Time.unscaledDeltaTime * speed;
            if(current >= max){
                current = max;
                already = true;
                image.enabled = true;
            }
            dissolve.SetFloat("_CutoffHeight", current);
        }

        if(!isAct && already)
        {
            image.enabled = false;

            current -= Time.unscaledDeltaTime * speed;
            if(current <= min){
                current = min;
                already = false;
                canAct = true;
                timer = 0f;
            }
            dissolve.SetFloat("_CutoffHeight", current);
        }
    }

    public void setIsAct(bool flag)
    {
        if(!canAct) return;
        isAct = flag;
        canAct = false;
    }
}
