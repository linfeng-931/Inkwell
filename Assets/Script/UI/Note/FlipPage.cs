using UnityEngine;

public class FlipPage : MonoBehaviour
{
    private float timer;
    private bool canFlip;
    private Animator ani;

    void Start()
    {
        timer = 0f;
        canFlip = true;
        ani = GetComponent<Animator>();
    }
    void Update()
    {
        if(canFlip) return;

        timer+=Time.unscaledDeltaTime;
        if(timer >= 0.13f)
        {
            canFlip = true;
            timer = 0f;
        }
    }

    public void Flip(bool isLeft)
    {
        if(!canFlip) return;

        if (isLeft)
        {
            ani.SetTrigger("left");
        }
        else
        {
            ani.SetTrigger("right");
        }
    }
}
