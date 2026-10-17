using UnityEngine;
using UnityEngine.UI;

public class DissolveEffect : MonoBehaviour
{
    public Material dissolve;
    public float max = 1f;
    public float min = 0f;
    public float speed = 2f;
    public Image image;

    private bool isAct;
    private bool already;
    private float current;
    private float timer;

    void Start()
    {
        current = min;
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
        else if(!isAct && already)
        {
            image.enabled = false;

            current -= Time.unscaledDeltaTime * speed;
            if(current <= min){
                current = min;
                already = false;
                timer = 0f;
            }
            dissolve.SetFloat("_CutoffHeight", current);
        }
    }

    public void SetIsAct(bool flag)
    {
        isAct = flag;
    }
}
